using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bufu.Rebuild.UI;
using UnityEngine;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        readonly HashSet<int> evolvingQualities=new HashSet<int>();
        int actorGeneration;
        internal static float EvolutionMergeEase(float t){t=Mathf.Clamp01(t);return t*t*(3-2*t);}
        internal static Vector2 EvolutionShakeOffset(float seconds)=>new Vector2(Mathf.Sin(seconds*52)*12,Mathf.Cos(seconds*52*1.3f)*6.6f);
        internal static float EvolutionShrinkEase(float t){t=Mathf.Clamp01(t);return 1-(1-t)*(1-t);}

        bool StartCoinEvolution(int quality) {
            if(evolvingQualities.Contains(quality)||!Model.CanEvolve(quality))return false;
            evolvingQualities.Add(quality);
            var old=Coins.Where(c=>c&&c.Quality==quality).ToArray();
            int tier=Model.State.evolution[Model.BaseId+quality+1],world=Model.Data.world;
            if(!Model.Evolve(quality)){evolvingQualities.Remove(quality);return false;}
            StartCoroutine(EvolutionSequence(quality,tier,world,actorGeneration,old));return true;
        }

        IEnumerator EvolutionSequence(int quality,int oldTier,int world,int generation,CoinActor[] old) {
            var center=ClampScene(Vector2.zero,40);
            var positions=old.Select(c=>c?c.Rect.anchoredPosition:center).ToArray();
            foreach(var coin in old)if(coin)coin.Evolving=true;
            string prefix=(world==1?"coin_":"dice_")+CoinKinds[quality-1];
            // The original creates every retained entity at the centre before merging the old population.
            var results=new List<CoinActor>();
            for(int i=0;i<Model.Count(quality);i++) {
                var rect=NewRect("Evolving "+quality+" "+i,ActorPool);rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.sizeDelta=new Vector2(CoinSize,CoinSize);
                rect.anchoredPosition=center;rect.localScale=Vector3.one*3;
                var result=rect.gameObject.AddComponent<CoinActor>();result.Init(this,quality,world,prefix+(oldTier==0?"":"_"+(oldTier+1)));result.Evolving=true;Coins.Add(result);results.Add(result);
            }
            float elapsed=0;
            while(elapsed<1f&&generation==actorGeneration&&Model.Data.world==world) {
                elapsed+=Time.deltaTime;float t=Mathf.Clamp01(elapsed);
                for(int i=0;i<old.Length;i++)if(old[i])old[i].Rect.anchoredPosition=Vector2.Lerp(positions[i],center,EvolutionMergeEase(t));
                foreach(var result in results)if(result)result.Rect.anchoredPosition=center+EvolutionShakeOffset(elapsed);
                yield return null;
            }
            if(generation!=actorGeneration||Model.Data.world!=world)yield break;
            foreach(var coin in old)if(coin){Coins.Remove(coin);Destroy(coin.gameObject);}
            var effects=new List<RectTransform>();int pendingEffects=0;
            foreach(var result in results)if(result) {
                result.Rect.anchoredPosition=center;
                var effect=NewRect("Original Evolution Effect",SmokePool);effect.anchoredPosition=center;effect.sizeDelta=new Vector2(200,200);
                effects.Add(effect);
                var graphic=effect.gameObject.AddComponent<SpineGraphic>();
                if(graphic.Load("Uieff_coin_upgrade")){pendingEffects++;graphic.Play("effect",false,()=>pendingEffects--);}
            }
            while(pendingEffects>0&&generation==actorGeneration&&Model.Data.world==world)yield return null;
            foreach(var effect in effects)if(effect)Destroy(effect.gameObject);
            if(generation!=actorGeneration||Model.Data.world!=world)yield break;
            foreach(var result in results)if(result)result.Visual.Load(prefix+"_"+(oldTier+2));
            elapsed=0;
            while(elapsed<.5f&&generation==actorGeneration&&Model.Data.world==world) {
                elapsed+=Time.deltaTime;
                foreach(var result in results)if(result)result.Rect.localScale=Vector3.one*Mathf.Lerp(3,1,EvolutionShrinkEase(elapsed/.5f));
                yield return null;
            }
            if(generation!=actorGeneration||Model.Data.world!=world)yield break;
            foreach(var result in results)if(result){result.Rect.localScale=Vector3.one;result.Evolving=false;}
            evolvingQualities.Remove(quality);SyncActors();
        }
    }
}
