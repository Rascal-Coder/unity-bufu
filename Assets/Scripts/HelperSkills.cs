using System;
using System.Collections.Generic;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    /// <summary>
    /// RoleVat.PlayExtra for helper ids 2..10. Projectiles use the exported skill sprites.
    /// Distances are design-canvas pixels: the original scales UI pixels by 0.01 into world units and moves tornadoes at speed*100.
    /// </summary>
    public static class HelperSkills {
        static float lastIron=-100f;

        public static string RoleVat(int id,int awake) {
            if(awake>0&&id==1)return "zgygy_role_toughguy_juexing";
            if(awake>0&&id==2)return "zgygy_role_miracle_juexing";
            if(awake>0&&id==3)return "zgygy_role_magician_juexing";
            switch(id) {
                case 1:return "zgygy_role_toughguy";
                case 2:return "zgygy_role_miracle";
                case 3:return "zgygy_role_magician";
                case 4:return "zgygy_role_kungfu";
                case 5:return "zgygy_role_jikun";
                case 6:return "zgygy_role_dashe";
                case 7:return "zgygy_role_jiuwei";
                case 8:return "zgygy_role_gangtieren";
                case 9:return "zgygy_role_halande";
                case 10:return "zgygy_role_dushen";
                default:return "zgygy_role";
            }
        }

        public static void Cast(HelperActor actor,CoinActor aim) {
            var game=actor.Game;int role=actor.RoleId;int awake=game.Model.HelperAwake(role);
            float skill=game.Model.HelperSkillValue(role),factor=game.Model.HelperConfig(role)?.skillFactor1??1;
            Vector2 from=actor.Rect.anchoredPosition,to=aim?aim.Rect.anchoredPosition:from+UnityEngine.Random.insideUnitCircle.normalized*200f;
            switch(role) {
                case 2: Tornado(game,role,from,to,Mathf.Max(1,Mathf.RoundToInt(skill)),500f*factor*(awake>0?1.5f:1f),(awake>0?15f:10f)*100f,awake>0);break;
                case 3: TossAround(game,from,factor*200f,Mathf.Max(1,Mathf.RoundToInt(skill)),role,awake>0,0,1,1,false);Burst(game,from,"zgygy_roleskill_magician_",0);break;
                case 4: Palm(game,role,to,skill,awake,"zgygy_roleskill_kungfu");break;
                case 5: Fan(game,role,from,to,Mathf.Max(1,Mathf.RoundToInt(skill)),1000f,1000f,30f,"bullet_jikun",awake>2?3:awake>1?2:awake>0?1:0,false,0);break;
                case 6: TossAround(game,from,Mathf.Max(300f,factor*100f),Mathf.Max(1,Mathf.RoundToInt(skill)),role,awake>2,0,1,awake>0?2f:1f,awake>1);Burst(game,from,"bullet_dashe-1_",0);break;
                case 7: Hearts(game,role,from,to,Mathf.Max(1,Mathf.RoundToInt(skill)),500f*Mathf.Max(1f,factor),awake);break;
                case 8: Iron(game,role,from,Mathf.Max(1,Mathf.RoundToInt(skill)),awake);break;
                case 9: Palm(game,role,to,skill,awake,"zgygy_roleskill_halande");if(awake>=2)Fan(game,role,from,to,awake>=3?2:1,1000f,1000f,30f,"bullet_halande",0,false,0);break;
                case 10: DiceBomb(game,role,awake);break;
            }
        }

        static void Palm(GameController game,int role,Vector2 center,float skill,int awake,string vat) {
            float scale=awake>1?2.5f:awake>0?2f:1.5f;
            game.PlayVatEffect(game.SmokePool,vat,"skill",90,center,false);
            TossAround(game,center,skill*120f*scale,10,role,false,0,1,1,false);
        }

        static void Tornado(GameController game,int role,Vector2 from,Vector2 to,int count,float distance,float speed,bool awake) {
            Vector2 dir=(to-from).sqrMagnitude<.01f?Vector2.right:(to-from).normalized;
            float step=15f,start=-(count-1)*step/2f;
            for(int i=0;i<count;i++) Shot(game,from,Rotate(dir,start+i*step),speed,distance,120f,0,role,0,1,1,false,awake?"zgygy_roleskill_miracle_1_":"zgygy_roleskill_miracle_",awake?0:1,null);
        }

        static void Fan(GameController game,int role,Vector2 from,Vector2 to,int count,float speed,float distance,float spread,string sprite,int bounces,bool comeBack,float suck) {
            Vector2 dir=(to-from).sqrMagnitude<.01f?UnityEngine.Random.insideUnitCircle.normalized:(to-from).normalized;
            float start=-(count-1)*spread/2f;
            for(int i=0;i<count;i++) {
                var shot=Shot(game,from,Rotate(dir,start+i*spread),speed,distance,70f,bounces,role,0,1,1,false,sprite,sprite.EndsWith("_")?0:-1,null);
                if(comeBack)shot.comeBack=true;
                if(suck>0)shot.done=p=>TossAround(game,p,suck,8,role,false,0,1,1,false);
            }
        }

        static void Hearts(GameController game,int role,Vector2 from,Vector2 to,int count,float distance,int awake) {
            Fan(game,role,from,to,count,800f,Mathf.Max(distance,800f),15f,"bullet_jiuwei_",0,awake>0,awake>1?150f:0);
        }

        static void Iron(GameController game,int role,Vector2 from,int count,int awake) {
            if(awake>2&&Time.time-lastIron>=20f){lastIron=Time.time;Burst(game,Vector2.zero,"zgygy_roleskill_gangtieren_2_",1);TossAround(game,Vector2.zero,3000f,999,role,false,0,1,1,false);return;}
            for(int i=0;i<count;i++) {
                Vector2 target=Pick(game,from,awake>0);
                Vector2 start=from+new Vector2((i-(count-1)/2f)*50f,0);
                Burst(game,start,"bullet_gangtieren_1_",0);
                SkillDelay.After(game,.7f,()=>{
                    Burst(game,target,"zgygy_roleskill_gangtieren_1_",1);
                    TossAround(game,target,300f,5,role,false,0,1,1,false);
                });
            }
        }

        static void DiceBomb(GameController game,int role,int awake) {
            int bombs=awake>=2?2:1;
            for(int i=0;i<bombs;i++) {
                var half=game.Field?game.Field.rect.size*.5f:new Vector2(720,640);
                Vector2 hit=game.ClampScene(new Vector2(UnityEngine.Random.Range(-half.x+80,half.x-80),UnityEngine.Random.Range(-half.y+80,half.y-80)),40);
                int point=awake>=3?6:UnityEngine.Random.Range(1,7);
                float scale=awake>=1?point:1;
                game.PlayVatEffect(game.SmokePool,"zgygy_roleskill_dushen","idle",80,hit+Vector2.up*400,false);
                SkillDelay.After(game,.7f,()=>{
                    TossAround(game,hit,200f,10,role,false,0,scale,1,false);
                    foreach(var dir in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right})
                        Shot(game,hit+dir*90f,dir,520f,1400f,80f,0,role,0,scale,1,false,null,-1,null,"zgygy_roleskill_dushen_1");
                });
            }
        }

        static Vector2 Pick(GameController game,Vector2 from,bool dense) {
            if(!dense){var any=game.FindThrowableForHelper(from);return any?any.Rect.anchoredPosition:from;}
            CoinActor best=null;int bestNear=-1;
            foreach(var coin in game.Coins) {
                if(!coin||coin.Busy)continue;int near=0;
                foreach(var other in game.Coins)if(other&&!other.Busy&&Vector2.Distance(other.Rect.anchoredPosition,coin.Rect.anchoredPosition)<180f)near++;
                if(near>bestNear){bestNear=near;best=coin;}
            }
            return best?best.Rect.anchoredPosition:from;
        }

        public static void TossAround(GameController game,Vector2 center,float radius,int max,int role,bool richFirst,float critAdd,float rewardScale,float speed,bool instant) {
            var list=new List<CoinActor>();
            foreach(var coin in game.Coins)if(coin&&!coin.Busy&&game.Model.CanHelperFlip(coin.Quality)&&Vector2.Distance(coin.Rect.anchoredPosition,center)<=radius)list.Add(coin);
            if(richFirst)list.Sort((a,b)=>b.Quality.CompareTo(a.Quality));
            int n=Math.Min(max,list.Count);
            for(int i=0;i<n;i++)list[i].Launch(game.RandomLanding(list[i].Rect.anchoredPosition),false,role,critAdd,rewardScale,speed,instant);
        }

        static SkillShot Shot(GameController game,Vector2 from,Vector2 dir,float speed,float distance,float hit,int bounces,int role,float critAdd,float reward,float tossSpeed,bool instant,string frames,int frameStart,Action<Vector2> done,string vatAsset=null) {
            var rt=GameController.NewRect("Skill",game.SmokePool);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=from;
            var shot=rt.gameObject.AddComponent<SkillShot>();
            shot.Begin(game,dir,speed,distance,hit,bounces,role,critAdd,reward,tossSpeed,instant,frames,frameStart,done,vatAsset);
            return shot;
        }

        static void Burst(GameController game,Vector2 at,string prefix,int start) {
            var rt=GameController.NewRect("Burst",game.SmokePool);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=at;
            var image=rt.gameObject.AddComponent<Image>();image.raycastTarget=false;
            var anim=rt.gameObject.AddComponent<FrameAnim>();anim.Begin(image,prefix,start,true);
        }

        static Vector2 Rotate(Vector2 d,float degrees){float r=degrees*Mathf.Deg2Rad,c=Mathf.Cos(r),s=Mathf.Sin(r);return new Vector2(d.x*c-d.y*s,d.x*s+d.y*c);}
    }

    /// <summary>A moving skill sprite that throws idle coins it passes.</summary>
    public sealed class SkillShot:MonoBehaviour {
        GameController game;Vector2 dir,origin;float speed,remain,hitRadius,reward=1,tossSpeed=1,critAdd;int bounces,role;bool instant,returning;
        readonly HashSet<int> seen=new HashSet<int>();
        public bool comeBack;public Action<Vector2> done;
        public void Begin(GameController game,Vector2 direction,float speed,float distance,float hit,int bounces,int role,float critAdd,float reward,float tossSpeed,bool instant,string frames,int frameStart,Action<Vector2> done,string vatAsset=null) {
            this.game=game;dir=direction.sqrMagnitude<.001f?Vector2.right:direction.normalized;this.speed=speed;remain=distance;hitRadius=hit;this.bounces=bounces;this.role=role;this.critAdd=critAdd;this.reward=reward;this.tossSpeed=tossSpeed;this.instant=instant;this.done=done;
            origin=((RectTransform)transform).anchoredPosition;
            if(!string.IsNullOrEmpty(vatAsset)) {
                var visual=VatGraphic.AddTo(gameObject);visual.raycastTarget=false;visual.PixelsPerUnit=vatAsset=="zgygy_roleskill_dushen_1"?36:64;
                visual.Load(vatAsset);visual.Play("effect",true);
                transform.localEulerAngles=new Vector3(0,0,Mathf.Atan2(dir.y,dir.x)*Mathf.Rad2Deg);
                return;
            }
            // A sprite-less uGUI Image renders as a solid white quad.
            var image=gameObject.AddComponent<Image>();image.raycastTarget=false;image.enabled=false;
            if(frameStart>=0)gameObject.AddComponent<FrameAnim>().Begin(image,frames,frameStart,false);
            else {var sprite=UiAssets.Sprite(frames);if(sprite){image.sprite=sprite;image.SetNativeSize();image.enabled=true;}}
        }
        void Update() {
            if(!game){Destroy(gameObject);return;}
            var rt=(RectTransform)transform;float step=Mathf.Min(remain,speed*Time.deltaTime);
            Vector2 next=rt.anchoredPosition+dir*step;Vector2 clamped=game.ClampScene(next,20);
            if(bounces>0&&(clamped-next).sqrMagnitude>1f){if(Mathf.Abs(clamped.x-next.x)>1f)dir.x=-dir.x;if(Mathf.Abs(clamped.y-next.y)>1f)dir.y=-dir.y;bounces--;next=clamped;}
            remain-=step;rt.anchoredPosition=next;Hit(next);
            if(remain>0)return;
            if(comeBack&&!returning){returning=true;Vector2 back=origin-rt.anchoredPosition;dir=back.sqrMagnitude<.01f?Vector2.right:back.normalized;remain=back.magnitude;comeBack=false;return;}
            done?.Invoke(rt.anchoredPosition);Destroy(gameObject);
        }
        void Hit(Vector2 p) {
            foreach(var coin in game.Coins) {
                if(!coin||coin.Busy||!game.Model.CanHelperFlip(coin.Quality))continue;
                int id=coin.GetInstanceID();if(seen.Contains(id)||Vector2.Distance(coin.Rect.anchoredPosition,p)>hitRadius)continue;
                seen.Add(id);coin.Launch(game.RandomLanding(coin.Rect.anchoredPosition),false,role,critAdd,reward,tossSpeed,instant);
            }
        }
    }

    public sealed class FrameAnim:MonoBehaviour {
        Image image;Sprite[] frames;float timer;int index;bool once;
        public void Begin(Image image,string prefix,int start,bool destroyAtEnd) {
            this.image=image;image.enabled=false;once=destroyAtEnd;var list=new List<Sprite>();
            for(int i=0;i<24;i++){var sprite=UiAssets.Sprite(prefix+(start+i).ToString("00"));if(!sprite)break;list.Add(sprite);}
            frames=list.ToArray();if(frames.Length>0){image.sprite=frames[0];image.SetNativeSize();image.enabled=true;}
        }
        void Update() {
            if(frames==null||frames.Length==0){if(once)Destroy(gameObject);return;}
            timer+=Time.deltaTime;if(timer<.05f)return;timer=0;index++;
            if(index>=frames.Length){if(once){Destroy(gameObject);return;}index=0;}
            image.sprite=frames[index];
        }
    }

    public sealed class SkillDelay:MonoBehaviour {
        public static void After(GameController game,float seconds,Action action) {
            var go=new GameObject("SkillDelay");go.transform.SetParent(game.SmokePool,false);go.AddComponent<SkillDelay>().StartCoroutine(Wait(go,seconds,action));
        }
        static System.Collections.IEnumerator Wait(GameObject go,float seconds,Action action){yield return new WaitForSeconds(seconds);action?.Invoke();if(go)Destroy(go);}
    }
}
