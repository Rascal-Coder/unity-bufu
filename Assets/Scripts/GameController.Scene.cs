using Bufu.Rebuild.UI;
using Data=System.Collections.Generic.Dictionary<string,object>;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>MainSceneVat: scene bounds, landing positions, depth sorting, chain reaction and the shared effect pools.</summary>
    public sealed partial class GameController {
        public RectTransform ActorPool,SmokePool,TipPool;
        /// <summary>RoleManager.sleepCount / isSleepTipActive: only the first three sleepers of a session get the wake-up bubble.</summary>
        public int SleepCount;public bool SleepTipActive;
        public readonly List<HelperActor> HelperActors=new List<HelperActor>();

        /// <summary>coinPool / RolePool / coins_smokePool / huanxingPool of MainSceneVat, stacked in that order inside sceneBoxUI.</summary>
        void BuildScenePools() {
            ActorPool=NewRect("RolePool",Field);Stretch(ActorPool);
            SmokePool=NewRect("coins_smokePool",Field);Stretch(SmokePool);
            TipPool=NewRect("huanxingPool",Field);Stretch(TipPool);
            SleepTipActive=false;
        }

        // ------------------------------------------------------------ bounds (MainSceneVat.ClampToSceneBounds / GetSafeWorldPadding)
        Vector2 SceneHalf {
            get {
                if(!Field)return new Vector2(640,570);
                var size=Field.rect.size;
                return new Vector2(Mathf.Max(size.x*.5f,560f),Mathf.Max(size.y*.5f,440f));
            }
        }
        public Vector2 ClampScene(Vector2 p,float padding) {
            var half=SceneHalf;float safe=Mathf.Min(padding,Mathf.Min(half.x,half.y)*2f*.49f);
            return new Vector2(Mathf.Clamp(p.x,-half.x+safe,half.x-safe),Mathf.Clamp(p.y,-half.y+safe,half.y-safe));
        }

        /// <summary>MainSceneVat.GetRandomLandingPosition: 60..120 px away from the start, kept inside the scene.</summary>
        public Vector2 RandomLanding(Vector2 start) {
            for(int i=0;i<20;i++) {
                float radius=UnityEngine.Random.Range(60f,120f);
                var direction=UnityEngine.Random.insideUnitCircle.normalized;
                var result=ClampScene(start+direction*radius,40);if(!InAvoidRect(result))return result;
            }
            return PushOutAvoidRects(ClampScene(start+UnityEngine.Random.insideUnitCircle*120,40));
        }

        /// <summary>MainSceneVat.GetRandomSpawnPosition: away from the other idle coins.</summary>
        public Vector2 RandomCoinSpawn() {
            var half=SceneHalf;float safe=Mathf.Min(50f,Mathf.Min(half.x,half.y)*2f*.49f);
            Vector2 p=Vector2.zero;
            for(int i=0;i<50;i++) {
                p=new Vector2(UnityEngine.Random.Range(-half.x+safe,half.x-safe),UnityEngine.Random.Range(-half.y+safe,half.y-safe));
                float interval=mainScene.Script("MainScene").F("minSpawnDistance",100);
                bool crowded=false;foreach(var coin in Coins)if(coin&&!coin.Busy&&Vector2.Distance(coin.Rect.anchoredPosition,p)<interval){crowded=true;break;}
                if(!crowded&&!InAvoidRect(p))return p;
            }
            p=new Vector2(UnityEngine.Random.Range(-half.x+safe,half.x-safe),UnityEngine.Random.Range(-half.y+safe,half.y-safe));
            return PushOutAvoidRects(p);
        }

        // MainSceneVat.SpawnPositions are relative to the background, not the scene-box centre.
        static readonly Vector2[] InitialCoinPositions={new Vector2(-95,295),new Vector2(-87,-200),new Vector2(213,-150)};
        public Vector2 InitialCoinSpawn(int index) {
            var bg=mainScene.Ref<RectTransform>("MainScene","bgImage");
            return ActorPool.InverseTransformPoint(bg.TransformPoint(InitialCoinPositions[index]));
        }

        /// <summary>RoleManager.GetSpawnPositionWithInterval: helpers stay at least 50 px apart where possible.</summary>
        public Vector2 RandomHelperSpawn() {
            var half=SceneHalf;float pad=30;Vector2 p=Vector2.zero;
            for(int i=0;i<50;i++) {
                p=new Vector2(UnityEngine.Random.Range(-half.x+pad,half.x-pad),UnityEngine.Random.Range(-half.y+pad,half.y-pad));
                bool crowded=false;foreach(var helper in HelperActors)if(helper&&Vector2.Distance(helper.Rect.anchoredPosition,p)<50f){crowded=true;break;}
                if(!crowded)return p;
            }
            return p;
        }

        // ------------------------------------------------------------ helper target search (RoleVat.FindTargetThrowable)
        public CoinActor FindThrowableForHelper(Vector2 from) {
            var candidates=new List<CoinActor>();
            if(Model.Data.world==2) {
                foreach(var coin in Coins)if(coin&&!coin.Busy&&Model.CanHelperFlip(coin.Quality))candidates.Add(coin);
                return candidates.Count==0?null:candidates[UnityEngine.Random.Range(0,candidates.Count)];
            }
            for(float radius=900;radius<=2000;radius+=300) {
                candidates.Clear();
                foreach(var coin in Coins)if(coin&&!coin.Busy&&Vector2.Distance(coin.Rect.anchoredPosition,from)<=radius&&Model.CanHelperFlip(coin.Quality))candidates.Add(coin);
                if(candidates.Count>0)return candidates[UnityEngine.Random.Range(0,candidates.Count)];
            }
            return null;
        }
        public CoinActor FindNearestThrowable(Vector2 from) {
            CoinActor best=null;float bestDist=float.MaxValue;
            foreach(var coin in Coins) {
                if(!coin||coin.Busy||!Model.CanHelperFlip(coin.Quality))continue;
                float dist=Vector2.SqrMagnitude(coin.Rect.anchoredPosition-from);
                if(dist<bestDist){bestDist=dist;best=coin;}
            }
            return best;
        }

        /// <summary>MainSceneVat.TriggerChainReaction: a landed silver / gold coin throws the idle coins within 100 px.</summary>
        public void ChainReaction(CoinActor landed) {
            int world=Model.Data.world;if(world!=1&&landed.World!=1)return;
            var origin=landed.Rect.anchoredPosition;
            foreach(var other in Coins.ToArray()) {
                if(!other||other==landed||other.Busy)continue;
                if(Vector2.Distance(other.Rect.anchoredPosition,origin)>100f)continue;
                bool trigger=false;
                if(landed.Quality==2&&other.Quality==1&&Model.Level(world==1?10:0,true)>0)trigger=true;
                if(landed.Quality==3&&other.Quality<=2&&Model.Level(11,true)>0&&world==1)trigger=true;
                if(trigger)other.Launch(RandomLanding(other.Rect.anchoredPosition),false);
            }
        }

        // ------------------------------------------------------------ depth order
        // Resting coins are behind helpers, flying coins in front of everything; inside a group the lower one is nearer.
        readonly List<KeyValuePair<float,Transform>> sortBuffer=new List<KeyValuePair<float,Transform>>();
        void SortActors() {
            if(!ActorPool)return;
            sortBuffer.Clear();
            foreach(var coin in Coins)if(coin&&coin.transform.parent==ActorPool)sortBuffer.Add(new KeyValuePair<float,Transform>((coin.Flying?2e4f:0)-coin.Rect.anchoredPosition.y,coin.transform));
            foreach(var helper in HelperActors)if(helper&&helper.transform.parent==ActorPool)sortBuffer.Add(new KeyValuePair<float,Transform>(1e4f-helper.Rect.anchoredPosition.y,helper.transform));
            sortBuffer.Sort((a,b)=>a.Key.CompareTo(b.Key));
            for(int i=0;i<sortBuffer.Count;i++)if(sortBuffer[i].Value.GetSiblingIndex()!=i)sortBuffer[i].Value.SetSiblingIndex(i);
        }

        // ------------------------------------------------------------ one-shot VAT effects (RoleVat.PlaySkillVatEffect / PlayCreateParticleVat)
        public void PlayVatEffect(RectTransform parent,string asset,string animation,float pixelsPerUnit,Vector2 anchoredPosition,bool _) {
            if(!parent)return;
            var rt=NewRect("Effect "+asset,parent);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=anchoredPosition;
            var vat=VatGraphic.AddTo(rt.gameObject);vat.raycastTarget=false;
            try{vat.Load(asset);}catch(Exception){Destroy(rt.gameObject);return;}
            vat.PixelsPerUnit=pixelsPerUnit;
            var go=rt.gameObject;vat.Play(animation,false,()=>{if(go)Destroy(go);});
        }
    }
}
