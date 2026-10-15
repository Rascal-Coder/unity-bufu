using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bufu.Rebuild.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class RuntimeVerification {
        IEnumerator VerifyShop() {
            var model=game.Model;var upgrade=Field<UiView>("upgrade");
            model.Data.diamonds=500;model.Data.spinCount=0;model.Data.spinProgress=0;model.Data.spinReadyAt=0;
            model.Data.chestNormal=model.Data.chestRare=0;model.Data.shopDate=GameModel.TodayNumber();
            model.State.averagePerMinute=100;
            Click(upgrade.Ref<Button>("UpgradePanel","button5"),"shop tab opens through actual UI input");
            yield return null;yield return new WaitForSecondsRealtime(.15f);
            var shop=Field<UiView>("shopView");
            Check(shop!=null&&shop.Root.activeInHierarchy,"original shop prefab is active");
            CheckGrowthBadge(false,"shop tab");
            Check(upgrade.Rect.Cast<Transform>().Where(t=>t.name=="upgradeTips").All(t=>!t.gameObject.activeSelf),"item upgrade feedback does not cover other tabs");
            Canvas.ForceUpdateCanvases();
            foreach(string field in new[]{"button201","button202","button203"}) {
                var corners=new Vector3[4];shop.Ref<RectTransform>("ShopTurntablePanel",field).GetWorldCorners(corners);
                float bottom=game.Root.InverseTransformPoint(corners[0]).y+game.Root.rect.height*.5f;
                Check(bottom>=398,"entire shop offer card clears bottom navigation and unlock labels: "+field);
            }
            foreach(string name in new[]{"shop_0","shop_2"}) {
                var graphic=shop.Node(name).GetComponent<SpineGraphic>();var mesh=graphic.canvasRenderer.GetMesh();
                Check(mesh.vertexCount>0,"shop Spine effect produces visible geometry: "+name);
                Check(mesh.uv.All(uv=>uv.x>=0&&uv.x<=1&&uv.y>=0&&uv.y<=1),"shop atlas UVs respect original page size after NPOT import: "+name);
            }
            var goldSlot=shop.Script("ShopRewardItemView",shop.IndexOf(shop.Refs("ShopTurntablePanel","wheelSlots")[0]));
            var goldLabel=shop.Nodes[goldSlot.Obj("valueText").I("node")].GetComponent<TMP_Text>();
            Check(goldLabel.text==GameModel.Format(model.ShopRewardAmount(model.WheelItems[0],0)),"wheel displays actual income-based gold reward");
            yield return Capture("04-shop.png");

            var before=(int[])model.Data.shards.Clone();double diamonds=model.Data.diamonds;
            Click(shop.Ref<Button>("ShopTurntablePanel","button202"),"normal chest can be bought through actual UI input");
            yield return null;
            Check(model.Data.diamonds==diamonds-50,"normal chest deducts the configured diamond price once");
            Check(Enumerable.Range(101,10).Sum(id=>model.Data.shards[id]-before[id])==4,"normal chest grants four usable helper fragments");
            Check(model.Data.shards[101]+model.Data.shards[102]-before[101]-before[102]==2&&model.Data.shards[103]+model.Data.shards[104]-before[103]-before[104]==2,"normal chest preserves N and R fragment quantities");
            var popup=ShopRewardPopup();var receipt=popup.GetComponentsInChildren<RectTransform>().Where(t=>t.name.StartsWith("reward",StringComparison.Ordinal)).ToArray();
            Check(receipt.Length==Enumerable.Range(101,10).Count(id=>model.Data.shards[id]>before[id]),"chest receipt lists precisely the fragments granted");
            foreach(var entry in receipt) {
                int id=int.Parse(entry.name.Substring("reward".Length));
                Check(entry.Find("countTmp").GetComponent<TMP_Text>().text==GameModel.Format(model.Data.shards[id]-before[id])&&entry.Find("Icon").GetComponent<Image>().sprite.name==id.ToString(),"chest receipt icon and count match helper inventory: "+id);
            }
            yield return Capture("05-shop-reward.png");
            Click(popup.Find("backBtn").GetComponent<Button>(),"chest reward receipt closes through actual UI input");
            yield return null;

            // Force a gold wedge while retaining the real spin button, animation and settlement path.
            var wheel=model.WheelItems;var rates=wheel.Select(item=>item.rate).ToArray();
            for(int i=0;i<wheel.Length;i++)wheel[i].rate=i==0?1:0;
            double gold=model.State.gold,prize=model.ShopRewardAmount(wheel[0],0);
            Click(shop.Ref<Button>("ShopTurntablePanel","spinAdButton"),"wheel starts through actual UI input");
            for(int i=0;i<wheel.Length;i++)wheel[i].rate=rates[i];
            Check(model.State.gold==gold+prize&&model.Data.spinCount==1&&model.SpinProgress==1,"wheel settles its displayed prize and progress exactly once before animation");
            Check(!shop.Ref<Button>("ShopTurntablePanel","button202").interactable&&!shop.Ref<Button>("ShopTurntablePanel","button201").interactable,"shop purchases wait for the active spin to finish");
            Click(upgrade.Ref<Button>("UpgradePanel","button1"),"player can leave shop while wheel animates");
            yield return new WaitForSecondsRealtime(6.2f);
            Check(!Field<bool>("spinning"),"hidden shop finishes spinning without waiting for disabled graphics");
            popup=ShopRewardPopup();
            Check(popup.Find("con/reward1")!=null||popup.GetComponentsInChildren<RectTransform>().Any(t=>t.name=="reward1"),"completed wheel displays its settled gold reward");
            Check(model.Data.spinCount==1&&model.SpinProgress==1,"animation completion does not grant a second spin prize");
            Click(popup.Find("backBtn").GetComponent<Button>(),"wheel reward receipt closes through actual UI input");
            yield return null;
            Click(upgrade.Ref<Button>("UpgradePanel","button5"),"shop reopens after hidden spin");
            yield return null;
            Check(shop.Ref<Button>("ShopTurntablePanel","spinAdButton").interactable,"hidden spin does not leave spin button permanently locked");
            Click(upgrade.Ref<Button>("UpgradePanel","button1"),"shop verification returns to item tab");
            yield return null;
            CheckGrowthBadge(true,"item tab after shop");
        }

        Transform ShopRewardPopup() {
            var layer=Field<RectTransform>("popupLayer");
            for(int i=layer.childCount-1;i>=0;i--){var child=layer.GetChild(i);if(child.name=="congratulationPanel"&&child.gameObject.activeInHierarchy)return child;}
            throw new Exception("Shop did not show a reward receipt");
        }
    }
}
