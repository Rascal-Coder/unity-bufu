using System;
using System.Collections.Generic;
using System.Linq;

namespace Bufu.Rebuild {
    public enum BillingState { Pending, Purchased, Cancelled, Failed }
    public enum ReceiptValidation { Verified, Rejected, RetryLater }
    public sealed class BillingProduct {public string productId,localizedPrice;public bool available;}
    public sealed class BillingPurchase {
        public string productId,transactionId;public BillingState state;public int quantity=1;
        // Opaque SDK receipt: handed only to the verifier/adapter, never written to a local save or log.
        public object receipt;
    }
    // All callbacks/events must run on Unity's main thread. Connect must refresh live prices.
    // QueryPurchases replays ALL owned non-consumables and unfinished consumables, including
    // PENDING, before returning true. A partial/offline response must return false.
    public interface IBillingGateway:IDisposable {
        event Action<BillingPurchase> PurchaseUpdated;
        bool Available {get;}
        IReadOnlyList<BillingProduct> Products {get;}
        void Connect(Action<bool> completed);
        void Purchase(string productId);
        void QueryPurchases(Action<bool> completed);
        // Called only after verification and durable reward+ledger save. Consume for consumables,
        // acknowledge for non-consumables. Failure must leave the purchase queryable for retry.
        void Complete(BillingPurchase purchase,bool consumable,Action<bool> completed);
    }
    public interface IPurchaseVerifier {
        void Verify(BillingPurchase purchase,Action<ReceiptValidation> completed);
    }
    public sealed class UnavailableBillingGateway:IBillingGateway {
        public event Action<BillingPurchase> PurchaseUpdated {add{}remove{}}
        public bool Available=>false;
        public IReadOnlyList<BillingProduct> Products=>Array.Empty<BillingProduct>();
        public void Connect(Action<bool> done)=>done(false);
        public void Purchase(string id){}
        public void QueryPurchases(Action<bool> done)=>done(false);
        public void Complete(BillingPurchase purchase,bool consumable,Action<bool> done)=>done(false);
        public void Dispose(){}
    }
    public sealed class RejectPurchaseVerifier:IPurchaseVerifier {
        public void Verify(BillingPurchase purchase,Action<ReceiptValidation> completed)=>completed(ReceiptValidation.Rejected);
    }
    public sealed class PurchaseService:IDisposable {
        public const string UnavailableMessage="Purchases are not available yet. No payment will be taken.";
        readonly GameModel model;readonly IBillingGateway gateway;readonly IPurchaseVerifier verifier;readonly Action persist;
        readonly HashSet<string> processing=new HashSet<string>();
        HashSet<string> querySeen;
        bool connecting,querying,disposed;string requested;
        public event Action Changed;
        public string Message {get;private set;}=UnavailableMessage;
        public bool Available=>gateway.Available&&!connecting;
        public bool Busy=>connecting||querying||requested!=null||processing.Count>0;
        public PurchaseService(GameModel model,IBillingGateway gateway,IPurchaseVerifier verifier,Action persist) {
            this.model=model;this.gateway=gateway;this.verifier=verifier;this.persist=persist;
            model.ValidatePurchases();gateway.PurchaseUpdated+=OnPurchase;
        }
        void Notify(string message){if(disposed)return;Message=message;Changed?.Invoke();}
        public BillingProduct Details(string id)=>gateway.Products.FirstOrDefault(p=>p.productId==id&&p.available&&!string.IsNullOrWhiteSpace(p.localizedPrice));
        public bool IsPending(string id)=>model.Data.purchases.pendingProducts.Contains(id);
        public bool IsOwned(PurchaseProduct product)=>!product.Consumable&&model.HasNoAds;
        public bool CanBuy(PurchaseProduct product)=>!disposed&&product!=null&&Available&&!Busy&&!IsPending(product.id)&&!IsOwned(product)&&Details(product.id)!=null;
        public void Connect() {
            if(disposed||Busy)return;connecting=true;Notify("Connecting to the store...");
            try{gateway.Connect(ok=>{if(disposed)return;connecting=false;Notify(ok&&gateway.Available?"Select a pack to view its details.":UnavailableMessage);if(ok&&gateway.Available)Restore();});}
            catch{connecting=false;Notify("Store connection failed. Please try again.");}
        }
        public void Buy(string id) {
            if(disposed)return;var product=PurchaseCatalog.Find(id);
            if(!CanBuy(product)){if(!Available)Notify(UnavailableMessage);return;}
            requested=id;Notify("Opening secure checkout...");
            try{gateway.Purchase(id);}catch{requested=null;Notify("Checkout could not open. Please try again.");}
        }
        public void Restore() {
            if(disposed||Busy)return;if(!Available){Notify(UnavailableMessage);return;}
            querying=true;querySeen=new HashSet<string>();Notify("Checking your purchases...");
            try{gateway.QueryPurchases(ok=>{if(disposed)return;querying=false;
                if(ok) {
                    // Pending payments cancelled/expired outside the app disappear from a complete
                    // Play query. Do not leave their products permanently blocked on this device.
                    int removed=model.Data.purchases.pendingProducts.RemoveAll(id=>!querySeen.Contains(id));
                    if(removed>0)try{persist();}catch{Notify("Could not save purchase status. Please check purchases again.");return;}
                }
                Notify(!ok?"Could not check purchases. Please try again.":processing.Count>0?"Verifying your purchases...":
                    model.Data.purchases.pendingProducts.Count>0?"A payment is pending. Rewards arrive after confirmation.":"Purchase check complete.");});}
            catch{querying=false;Notify("Could not check purchases. Please try again.");}
        }
        void OnPurchase(BillingPurchase purchase) {
            if(disposed||purchase==null)return;
            if(querying&&(purchase.state==BillingState.Pending||purchase.state==BillingState.Purchased))querySeen.Add(purchase.productId);
            if(requested==purchase.productId)requested=null;
            var product=PurchaseCatalog.Find(purchase.productId);
            if(product==null){Notify("This product is not supported by this build.");return;}
            if(purchase.state==BillingState.Pending) {
                if(!IsPending(product.id)){model.Data.purchases.pendingProducts.Add(product.id);try{persist();}catch{Notify("Payment pending. Purchase status will be checked again.");return;}}
                Notify("Payment pending. No rewards are granted until payment is confirmed.");return;
            }
            if(purchase.state==BillingState.Cancelled||purchase.state==BillingState.Failed) {
                // Only an authoritative cancelled/failed update clears a pending product.
                model.Data.purchases.pendingProducts.Remove(product.id);
                try{persist();}catch{}
                Notify(purchase.state==BillingState.Cancelled?"Purchase cancelled. No rewards were added.":"Purchase failed. Please try again.");return;
            }
            if(purchase.state!=BillingState.Purchased){Notify("Purchase status is not recognized. No rewards were added.");return;}
            if(string.IsNullOrWhiteSpace(purchase.transactionId)||purchase.quantity<1||purchase.quantity>100||(!product.Consumable&&purchase.quantity!=1)) {
                Notify("Purchase details could not be verified. No rewards were added.");return;
            }
            var recorded=model.Data.purchases.receipts.FirstOrDefault(r=>r.transactionId==purchase.transactionId);
            if(recorded!=null&&(recorded.productId!=product.id||recorded.quantity!=purchase.quantity)) {
                Notify("Purchase details do not match. No rewards were added.");return;
            }
            if(!processing.Add(purchase.transactionId))return;
            if(recorded==null&&!IsPending(product.id)) {
                model.Data.purchases.pendingProducts.Add(product.id);
                try{persist();}catch{processing.Remove(purchase.transactionId);Notify("Could not save purchase status. Check purchases to retry.");return;}
            }
            Notify("Verifying your purchase...");
            bool validationDelivered=false;
            try{verifier.Verify(purchase,result=>{
                if(disposed||validationDelivered)return;validationDelivered=true;
                if(result!=ReceiptValidation.Verified) {
                    processing.Remove(purchase.transactionId);
                    if(result==ReceiptValidation.Rejected){model.Data.purchases.pendingProducts.Remove(product.id);try{persist();}catch{}}
                    Notify(result==ReceiptValidation.RetryLater?"Verification is delayed. Check purchases to retry.":"Purchase verification failed. No rewards were added.");return;
                }
                bool pending=model.Data.purchases.pendingProducts.Remove(product.id);
                try {
                    if(!model.FulfillPurchase(product,purchase.transactionId,purchase.quantity,persist)) {
                        if(pending)model.Data.purchases.pendingProducts.Add(product.id);
                        processing.Remove(purchase.transactionId);Notify("Purchase details do not match. No rewards were added.");return;
                    }
                } catch {
                    if(pending)model.Data.purchases.pendingProducts.Add(product.id);
                    processing.Remove(purchase.transactionId);Notify("Could not save this reward. Check purchases to retry.");return;
                }
                try{gateway.Complete(purchase,product.Consumable,ok=>{
                    if(disposed)return;processing.Remove(purchase.transactionId);
                    Notify(ok?"Purchase delivered. Thank you!":"Reward saved. Store confirmation will retry on the next purchase check.");
                });}catch{processing.Remove(purchase.transactionId);Notify("Reward saved. Store confirmation will retry on the next purchase check.");}
            });}catch{processing.Remove(purchase.transactionId);Notify("Verification is delayed. Check purchases to retry.");}
        }
        public void Dispose(){if(disposed)return;disposed=true;gateway.PurchaseUpdated-=OnPurchase;gateway.Dispose();Changed=null;}
    }
}
