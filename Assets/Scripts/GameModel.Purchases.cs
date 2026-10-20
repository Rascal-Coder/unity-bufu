using System;
using System.Collections.Generic;
using System.Linq;

namespace Bufu.Rebuild {
    [Serializable] public sealed class PurchaseReceipt {
        // Stable hash of the Play purchase token; never store or log the raw token here.
        public string transactionId,productId; public int quantity=1;
    }
    [Serializable] public sealed class PurchaseSave {
        public bool noAds; public List<PurchaseReceipt> receipts=new List<PurchaseReceipt>();
        public List<string> pendingProducts=new List<string>();
    }
    public sealed partial class GameModel {
        public bool HasNoAds=>Data.purchases!=null&&Data.purchases.noAds;
        internal void ValidatePurchases() {
            if(Data.purchases==null)Data.purchases=new PurchaseSave();
            if(Data.purchases.receipts==null)Data.purchases.receipts=new List<PurchaseReceipt>();
            Data.purchases.receipts.RemoveAll(r=>r==null||string.IsNullOrWhiteSpace(r.transactionId)||string.IsNullOrWhiteSpace(r.productId));
            if(Data.purchases.pendingProducts==null)Data.purchases.pendingProducts=new List<string>();
        }
        internal bool FulfillPurchase(PurchaseProduct product,string transactionId,int quantity,Action persist) {
            if(product==null||string.IsNullOrWhiteSpace(transactionId)||quantity<1||quantity>100||(!product.Consumable&&quantity!=1))return false;
            var save=Data.purchases;var existing=save.receipts.FirstOrDefault(r=>r.transactionId==transactionId);
            if(existing!=null) {
                if(existing.productId!=product.id||existing.quantity!=quantity)return false;
                persist();return true;
            }
            double diamonds=Data.diamonds;bool ads=save.noAds;
            int shards=product.category==StoreCategory.Fragments?Data.shards[product.rewardId]:0;
            long amount=(long)product.amount*quantity;
            if(product.category==StoreCategory.Fragments&&amount>int.MaxValue-shards)return false;
            var receipt=new PurchaseReceipt{productId=product.id,transactionId=transactionId,quantity=quantity};
            if(product.category==StoreCategory.Diamonds) {
                double total=diamonds+amount;if(!Finite(total)||total<=diamonds)return false;Data.diamonds=total;
            } else if(product.category==StoreCategory.Fragments)Data.shards[product.rewardId]+=(int)amount;
            else save.noAds=true;
            save.receipts.Add(receipt);
            try{persist();}catch {
                Data.diamonds=diamonds;save.noAds=ads;
                if(product.category==StoreCategory.Fragments)Data.shards[product.rewardId]=shards;
                save.receipts.Remove(receipt);throw;
            }
            Changed?.Invoke();return true;
        }
    }
}
