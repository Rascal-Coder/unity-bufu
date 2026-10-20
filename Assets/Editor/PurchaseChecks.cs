using System;
using System.Collections.Generic;
using System.Linq;
using Bufu.Rebuild;
using UnityEngine;

public static class PurchaseChecks {
    sealed class Gateway:IBillingGateway {
        public event Action<BillingPurchase> PurchaseUpdated;
        public bool Available=>true;
        public bool prices=true,queryOk=true;
        public IReadOnlyList<BillingProduct> Products=>PurchaseCatalog.Products.Select(p=>new BillingProduct{productId=p.id,available=true,localizedPrice=prices?"TEST":""}).ToArray();
        public int launches,finishes;public bool finishOk=true;public Action<BillingPurchase> onFinish;public List<bool> consumed=new List<bool>();
        public void Connect(Action<bool> done)=>done(true);
        public void Purchase(string id){launches++;}
        public void QueryPurchases(Action<bool> done)=>done(queryOk);
        public void Complete(BillingPurchase p,bool consume,Action<bool> done){finishes++;consumed.Add(consume);onFinish?.Invoke(p);done(finishOk);}
        public void Send(string id,string tx,BillingState state=BillingState.Purchased,int quantity=1)=>PurchaseUpdated?.Invoke(new BillingPurchase{productId=id,transactionId=tx,state=state,quantity=quantity});
        public void Dispose(){}
    }
    sealed class Verifier:IPurchaseVerifier {
        public ReceiptValidation result=ReceiptValidation.Verified;public bool delay;public Action<ReceiptValidation> callback;
        public void Verify(BillingPurchase p,Action<ReceiptValidation> done){if(delay)callback=done;else done(result);}
    }
    public static void Verify(Action<bool,string> check) {
        void Test(bool ok,string description)=>check(ok,"Purchases: "+description);
        var model=new GameModel();double initial=model.Data.diamonds;
        using(var absent=new PurchaseService(model,new UnavailableBillingGateway(),new RejectPurchaseVerifier(),()=>{})) {
            absent.Connect();absent.Buy("diamonds_100");absent.Restore();
            Test(!absent.Available&&!absent.Busy&&model.Data.diamonds==initial,"unconnected store never charges or grants rewards");
        }
        Test(PurchaseCatalog.Products.Count==14&&PurchaseCatalog.Products.Count(p=>p.category==StoreCategory.Fragments)==10,"catalog has three diamond packs, one entitlement and ten concrete helper packs");
        var gateway=new Gateway();var verifier=new Verifier();int saves=0;bool failSave=false;
        using(var service=new PurchaseService(model,gateway,verifier,()=>{if(failSave&&model.Data.purchases.receipts.Any(r=>r.transactionId=="disk-failure"))throw new Exception("disk full");saves++;})) {
            service.Connect();gateway.prices=false;service.Buy("diamonds_100");Test(gateway.launches==0,"missing localized price blocks checkout");gateway.prices=true;
            service.Buy("diamonds_100");service.Buy("diamonds_550");
            Test(gateway.launches==1&&service.Busy,"double tap cannot start a second checkout");
            gateway.Send("diamonds_100","",BillingState.Cancelled);
            Test(!service.Busy&&model.Data.diamonds==initial&&gateway.finishes==0,"cancel leaves wallet and store finalization unchanged");
            service.Buy("diamonds_100");gateway.Send("diamonds_100","pending-1",BillingState.Pending);
            Test(service.IsPending("diamonds_100")&&!service.CanBuy(PurchaseCatalog.Find("diamonds_100"))&&model.Data.diamonds==initial,"pending payment is saved and cannot deliver or repurchase the same product");
            var pendingReload=new GameModel(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(model.Data)));
            Test(pendingReload.Data.purchases.pendingProducts.Contains("diamonds_100"),"pending payment survives JSON save reload");
            gateway.onFinish=p=>Test(saves>0&&model.Data.purchases.receipts.Any(r=>r.transactionId==p.transactionId),"durable receipt exists before consume or acknowledge");
            gateway.Send("diamonds_100","order-1");
            Test(model.Data.diamonds==initial+100&&!service.IsPending("diamonds_100")&&gateway.finishes==1,"verified pending-to-purchased update delivers exactly one pack");
            gateway.Send("diamonds_100","order-1");
            Test(model.Data.diamonds==initial+100&&gateway.finishes==2,"duplicate callback retries finalization without duplicate reward");
            int done=gateway.finishes;gateway.Send("diamonds_550","order-1");
            Test(model.Data.diamonds==initial+100&&gateway.finishes==done,"same transaction cannot deliver a different product");
            verifier.result=ReceiptValidation.Rejected;gateway.Send("diamonds_100","rejected");
            Test(model.Data.diamonds==initial+100&&gateway.finishes==done,"rejected receipt cannot grant or consume");
            verifier.result=ReceiptValidation.RetryLater;gateway.Send("diamonds_100","retry");
            Test(!service.Busy&&model.Data.diamonds==initial+100,"verification network failure leaves the purchase retryable");
            Test(service.IsPending("diamonds_100")&&!service.CanBuy(PurchaseCatalog.Find("diamonds_100")),"delayed verification blocks accidental repeat payment for that product");
            verifier.result=ReceiptValidation.Verified;failSave=true;gateway.Send("diamonds_100","disk-failure");
            Test(model.Data.diamonds==initial+100&&!model.Data.purchases.receipts.Any(r=>r.transactionId=="disk-failure")&&gateway.finishes==done,"failed durable save rolls back rewards and does not consume");
            failSave=false;gateway.finishOk=false;gateway.Send("diamonds_100","disk-failure");
            Test(model.Data.diamonds==initial+200&&!service.Busy,"retry delivers once even if store completion needs retry");
            gateway.finishOk=true;gateway.Send("diamonds_100","disk-failure");
            Test(model.Data.diamonds==initial+200,"store-completion retry never duplicates an already saved reward");
            verifier.delay=true;gateway.Send("helper_10_fragments_20","helper-1");gateway.Send("helper_10_fragments_20","helper-1");
            Test(service.Busy&&model.Data.shards[110]==0,"simultaneous duplicate callbacks wait for a single receipt validation");
            verifier.callback(ReceiptValidation.Verified);verifier.callback(ReceiptValidation.Verified);verifier.delay=false;
            Test(!service.Busy&&model.Data.shards[110]==20&&model.Data.shards[101]==0,"fragments enter the selected existing helper inventory exactly once");
            gateway.Send("permanent_no_ads","ads-1");gateway.Send("permanent_no_ads","ads-1");
            Test(model.HasNoAds&&!service.CanBuy(PurchaseCatalog.Find("permanent_no_ads")),"permanent entitlement is owned and cannot be bought twice");
            Test(!gateway.consumed.Last()&&gateway.consumed.First(),"consumable packs are consumed while No Ads is acknowledged");
            done=gateway.finishes;gateway.Send("permanent_no_ads","ads-invalid",quantity:2);gateway.Send("unknown","unknown");gateway.Send("diamonds_100","empty-quantity",quantity:0);
            Test(gateway.finishes==done,"unknown products and invalid quantities cannot settle");
            gateway.Send("diamonds_100","multi",quantity:2);
            Test(model.Data.diamonds==initial+400,"verified multi-quantity consumables grant the exact quantity");
            gateway.Send("diamonds_550","expired",BillingState.Pending);gateway.queryOk=false;service.Restore();
            Test(service.IsPending("diamonds_550"),"failed store query preserves pending payments");gateway.queryOk=true;service.Restore();
            Test(!service.IsPending("diamonds_550"),"a complete store query clears expired payments that are no longer owned");
        }
        var reload=new GameModel(JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(model.Data)));
        Test(reload.HasNoAds&&reload.Data.shards[110]==20&&reload.Data.purchases.receipts.Count==model.Data.purchases.receipts.Count,"entitlement, helper fragments and duplicate protection survive save reload");
        var secondGateway=new Gateway();using(var service=new PurchaseService(reload,secondGateway,new Verifier(),()=>{})) {
            secondGateway.Send("diamonds_100","order-1");Test(reload.Data.diamonds==model.Data.diamonds,"purchase replay after restarting cannot mint diamonds");
        }
        var restored=new GameModel();var ownedGateway=new Gateway();using(var ownedService=new PurchaseService(restored,ownedGateway,new Verifier(),()=>{})) {
            ownedGateway.Send("permanent_no_ads","restored-ownership");Test(restored.HasNoAds&&restored.Data.diamonds==20,"restored non-consumable grants ownership without recreating spent consumables");
        }
        var oldSave=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(model.Data));oldSave.purchases=null;
        var legacy=new GameModel(oldSave);Test(!legacy.HasNoAds&&legacy.Data.purchases.receipts.Count==0&&legacy.Data.diamonds==model.Data.diamonds,"legacy saves gain an empty purchase record without altering their wallet");
    }
}
