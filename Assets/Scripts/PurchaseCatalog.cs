using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Bufu.Rebuild.UI;

namespace Bufu.Rebuild {
    public enum StoreCategory { Diamonds, NoAds, Fragments }
    [Serializable] public sealed class PurchaseProduct {
        public string id; public StoreCategory category; public int rewardId,amount;
        public bool Consumable=>category!=StoreCategory.NoAds;
    }
    public static class PurchaseCatalog {
        static PurchaseProduct[] products;
        public static IReadOnlyList<PurchaseProduct> Products {
            get {
                if(products==null) {
                    var asset=Resources.Load<TextAsset>("Commerce/products");
                    products=asset?JsonUtility.FromJson<Table<PurchaseProduct>>(asset.text).rows:null;
                    if(products==null||products.Length==0||products.Any(p=>p==null||string.IsNullOrEmpty(p.id))||products.Select(p=>p.id).Distinct().Count()!=products.Length)
                        throw new InvalidOperationException("Invalid purchase catalog");
                    foreach(var p in products)if(!Enum.IsDefined(typeof(StoreCategory),p.category)||
                        (p.category==StoreCategory.Diamonds&&(p.rewardId!=2||p.amount<=0))||
                        (p.category==StoreCategory.Fragments&&(p.rewardId<101||p.rewardId>110||p.amount<=0))||
                        (p.category==StoreCategory.NoAds&&(p.amount!=0||p.rewardId!=0)))throw new InvalidOperationException("Invalid purchase reward: "+p.id);
                }
                return products;
            }
        }
        public static PurchaseProduct Find(string id)=>Products.FirstOrDefault(p=>p.id==id);
        public static string Title(PurchaseProduct p,GameModel model) {
            if(p.category==StoreCategory.Diamonds)return p.amount.ToString("N0",L10n.Culture)+" Diamonds";
            if(p.category==StoreCategory.NoAds)return "Permanent No Ads";
            return L10n.Translate(model.helperRoles.First(h=>h.id==p.rewardId-100).helperName)+" Fragments";
        }
        public static string Description(PurchaseProduct p)=>p.category==StoreCategory.NoAds
            ?"Claim every ad reward without watching a video. Tap to claim. Cooldowns and daily limits still apply. One-time purchase."
            :p.category==StoreCategory.Fragments?"Receive "+p.amount+" fragments for this helper. Used for helper upgrades; normal unlock requirements still apply."
            :"Add "+p.amount.ToString("N0",L10n.Culture)+" diamonds to your shared wallet. A repeatable purchase.";
    }
}
