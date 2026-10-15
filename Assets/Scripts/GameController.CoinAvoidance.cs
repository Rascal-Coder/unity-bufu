using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        readonly List<Rect> sceneAvoidRects=new List<Rect>(),nextAvoidRects=new List<Rect>();
        readonly Vector3[] avoidCorners=new Vector3[4];
        void RefreshSceneAvoidRects() {
            if(!Field||promotionView==null)return;nextAvoidRects.Clear();
            foreach(string field in new[]{"earnMoneyButton","inspireButton","instantMoneyButton","challengeBtn"}) {
                var go=PromotionNode(field);if(!go.activeInHierarchy)continue;
                ((RectTransform)go.transform).GetWorldCorners(avoidCorners);
                Vector2 min=new Vector2(float.PositiveInfinity,float.PositiveInfinity),max=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
                foreach(var corner in avoidCorners){Vector2 p=Field.InverseTransformPoint(corner);min=Vector2.Min(min,p);max=Vector2.Max(max,p);}
                nextAvoidRects.Add(Rect.MinMaxRect(min.x-30,min.y-30,max.x+30,max.y+30));
            }
            if(sceneAvoidRects.SequenceEqual(nextAvoidRects))return;
            sceneAvoidRects.Clear();sceneAvoidRects.AddRange(nextAvoidRects);
            foreach(var coin in Coins)if(coin&&!coin.Busy)coin.Rect.anchoredPosition=PushOutAvoidRects(coin.Rect.anchoredPosition);
        }
        bool InAvoidRect(Vector2 point)=>sceneAvoidRects.Exists(rect=>rect.Contains(point));
        Vector2 PushOutAvoidRects(Vector2 point) {
            foreach(var rect in sceneAvoidRects)if(rect.Contains(point)) {
                float left=Mathf.Abs(point.x-rect.xMin),right=Mathf.Abs(rect.xMax-point.x),bottom=Mathf.Abs(point.y-rect.yMin),top=Mathf.Abs(rect.yMax-point.y);
                float distance=Mathf.Min(left,right,bottom,top);
                if(Mathf.Approximately(distance,left))point.x=rect.xMin-1;
                else if(Mathf.Approximately(distance,right))point.x=rect.xMax+1;
                else if(Mathf.Approximately(distance,bottom))point.y=rect.yMin-1;
                else point.y=rect.yMax+1;
            }
            return ClampScene(point,40);
        }
    }
}
