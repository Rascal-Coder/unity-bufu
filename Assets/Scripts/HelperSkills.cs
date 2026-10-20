using System;
using System.Collections;
using System.Collections.Generic;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    // RoleVat's additional skills. All distances below are original design-canvas pixels.
    public static class HelperSkills {
        static float lastIron=-60f;
        public const float IronCellSize=120f; // MainSceneVat.prefab spatialIndexCellSize, converted to design pixels.
        public static string RoleVat(int id,int awake) {
            if(awake>0&&id==1)return "zgygy_role_toughguy_juexing";
            if(awake>0&&id==2)return "zgygy_role_miracle_juexing";
            if(awake>0&&id==3)return "zgygy_role_magician_juexing";
            switch(id) {
                case 1:return "zgygy_role_toughguy";case 2:return "zgygy_role_miracle";
                case 3:return "zgygy_role_magician";case 4:return "zgygy_role_kungfu";
                case 5:return "zgygy_role_jikun";case 6:return "zgygy_role_dashe";
                case 7:return "zgygy_role_jiuwei";case 8:return "zgygy_role_gangtieren";
                case 9:return "zgygy_role_halande";case 10:return "zgygy_role_dushen";
                default:return "zgygy_role";
            }
        }
        public static float DiceMoveDistance(HelperRole role,int level) {
            // dushen_DiceMove.prefab overrides the C# default 300 with 400.
            return role==null||role.baseSkillNum<=0?400f:400f*(1+GameModel.SkillRank(role,level)*role.upgradeSkillNum/role.baseSkillNum);
        }
        public static int BasketballBounces(int awake)=>awake>2?3:awake>1?2:awake>0?1:0;
        public static float DiceMoveWidth(GameModel model) {
            // DuShenDiceMove.OnEnable reads the dice talent tree even when this helper is used in the coin world.
            int level=model.Data.worlds[1].talents[305];
            if(level>0)foreach(var talent in model.diceTalents)if(talent.id==305)return 1+(float)talent.upgradeResult[Math.Min(level-1,talent.upgradeResult.Length-1)];
            return 1;
        }
        public static void Cast(HelperActor actor,CoinActor aim) {
            var game=actor.Game;int role=actor.RoleId,awake=game.Model.HelperAwake(role);
            float skill=game.Model.HelperSkillValue(role),factor=game.Model.HelperConfig(role)?.skillFactor1??1;
            Vector2 from=actor.Rect.anchoredPosition,to=aim?aim.Rect.anchoredPosition:from+UnityEngine.Random.insideUnitCircle.normalized*200;
            int count=Mathf.Max(1,Mathf.RoundToInt(skill));
            switch(role) {
                case 2: Fan(game,role,from,to,count,awake>0?1500:1000,500*factor*(awake>0?1.5f:1),15,awake>0?"zgygy_roleskill_miracle_1_":"zgygy_roleskill_miracle_",awake>0?0:1,150,SkillShot.Kind.Tornado,0,false,0);break;
                case 3:
                    foreach(var coin in Nearby(game,from,factor*200,awake>0)) {
                        if(count<=0)break;
                        if(Toss(game,coin,role)){count--;Burst(game,coin.Rect.anchoredPosition,"zgygy_roleskill_magician_",0,2);}
                    }
                    break;
                case 4: Palm(game,role,to,skill,awake,"zgygy_roleskill_kungfu");break;
                case 5: Fan(game,role,from,to,count,1000,0,30,"bullet_jikun",-1,60,SkillShot.Kind.Basketball,BasketballBounces(awake),false,0);break;
                case 6: Tentacles(game,from,count,Mathf.Max(300,factor*100),awake);break;
                case 7: Fan(game,role,from,to,count,800,factor>0?500*factor:500,15,"bullet_jiuwei_",0,80,SkillShot.Kind.Heart,0,awake>0,awake>1?150:0);break;
                case 8: Run(game,Iron(game,from,count,awake));break;
                case 9:
                    Palm(game,role,to,skill,awake,"zgygy_roleskill_halande");
                    if(awake>=2)Fan(game,role,from,to,awake>=3?2:1,1000,0,30,"bullet_halande",-1,60,SkillShot.Kind.Football,0,false,0);
                    break;
                case 10: Run(game,DiceBombs(game,awake));break;
            }
        }
        static void Palm(GameController game,int role,Vector2 center,float skill,int awake,string asset) {
            float scale=awake>1?2.5f:awake>0?2:1.5f;
            var visual=Vat(game,center,asset,100*scale);
            string animation=role==9&&awake>0&&visual.Has("skill_juexing")?"skill_juexing":"skill";
            visual.Play(animation,false,()=>{if(visual)UnityEngine.Object.Destroy(visual.gameObject);});
            TossAround(game,center,skill*120*scale,10,role,false,0,1,1,false);
        }
        static void Fan(GameController game,int role,Vector2 from,Vector2 to,int count,float speed,float distance,float spread,string frames,int frameStart,float hit,SkillShot.Kind kind,int bounces,bool comeBack,float suck) {
            var direction=to-from;if(direction.sqrMagnitude<.001f)direction=UnityEngine.Random.insideUnitCircle;
            if(direction.sqrMagnitude<.001f)direction=Vector2.right;
            for(int i=0;i<count;i++) {
                var shot=Shot(game,from,Rotate(direction.normalized,(i-(count-1)*.5f)*spread),speed,distance,hit,role,1,frames,frameStart);
                shot.SetMode(kind);shot.bounces=bounces;shot.comeBack=comeBack;shot.suckRadius=suck;
                shot.transform.localScale=Vector3.one*(kind==SkillShot.Kind.Tornado?2:kind==SkillShot.Kind.Heart?1.5f:1);
            }
        }
        static void Tentacles(GameController game,Vector2 from,int count,float radius,int awake) {
            foreach(var coin in Nearby(game,from,radius,awake>2)) {
                if(count<=0)break;
                if(Vector2.Distance(from,coin.Rect.anchoredPosition)<=100)continue;
                count--;
                Run(game,Tentacle(game,from+Vector2.up*100,coin));
            }
        }
        static IEnumerator Tentacle(GameController game,Vector2 from,CoinActor coin) {
            if(!coin)yield break;
            Vector2 delta=coin.Rect.anchoredPosition-from;
            var rt=Prefab(game,"dashehandani",from,false);rt.pivot=new Vector2(0,.5f);
            rt.localEulerAngles=new Vector3(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
            rt.localScale=new Vector3(1,UnityEngine.Random.value>.5f?-1:1,1);
            var anim=rt.GetComponent<FrameAnim>();
            rt.sizeDelta=new Vector2(delta.magnitude,210);
            bool completed=false;int world=game.Model.Data.world;
            // DaSheHand.OnAnimComplete throws on the actual final sequence step, including frame quantization.
            anim.Completed=()=>{
                completed=true;
                if(game&&game.Model.Data.world==world&&coin&&!coin.Busy)Toss(game,coin,6);
                if(rt)UnityEngine.Object.Destroy(rt.gameObject);
            };
            while(rt&&!completed)yield return null;
        }
        static IEnumerator Iron(GameController game,Vector2 from,int count,int awake) {
            if(awake>2&&Time.time-lastIron>=20) {
                lastIron=Time.time;
                var missile=SpriteEffect(game,Vector2.up*1500,"bullet_gangtieren_2_",0,1);
                yield return Move(missile,Vector2.up*1500,Vector2.zero,1,t=>t);
                if(missile)UnityEngine.Object.Destroy(missile.gameObject);
                Burst(game,Vector2.zero,"zgygy_roleskill_gangtieren_2_",1,2);
                TossAround(game,Vector2.zero,3000,int.MaxValue,8,false,0,1,1,false);
                yield break;
            }
            var targets=IronTargets(game,awake>0);
            for(int i=0;i<count;i++) {
                Vector2 target=targets.Count>0?targets[UnityEngine.Random.Range(0,targets.Count)]:Vector2.zero;
                Vector2 start=from+Vector2.right*((i-(count-1)*.5f)*50);
                Run(game,Missile(game,start,target));
            }
        }
        static IEnumerator Missile(GameController game,Vector2 start,Vector2 target) {
            var rt=SpriteEffect(game,start,"bullet_gangtieren_1_",0,2);
            yield return new WaitForSeconds(UnityEngine.Random.Range(.1f,.3f));
            yield return Move(rt,start,start+Vector2.up*1500,.5f,t=>1-(1-t)*(1-t));
            if(!rt)yield break;
            rt.localEulerAngles=new Vector3(0,0,180);
            yield return Move(rt,target+Vector2.up*1500,target,.4f,t=>t);
            if(rt)UnityEngine.Object.Destroy(rt.gameObject);
            Burst(game,target,"zgygy_roleskill_gangtieren_1_",1,2);
            TossAround(game,target,300,5,8,false,0,1,1,false);
        }
        static List<Vector2> IronTargets(GameController game,bool dense) {
            var targets=new List<Vector2>();var cells=new Dictionary<Vector2Int,List<Vector2>>();var order=new List<Vector2Int>();Vector2 best=Vector2.zero;int most=0;
            foreach(var coin in game.Coins) {
                if(!coin)continue;
                var p=coin.Rect.anchoredPosition;targets.Add(p);
                if(!dense||coin.Busy)continue;
                var key=new Vector2Int(Mathf.FloorToInt(p.x/IronCellSize),Mathf.FloorToInt(p.y/IronCellSize));
                if(!cells.TryGetValue(key,out var cell)){cell=new List<Vector2>();cells.Add(key,cell);order.Add(key);}
                cell.Add(p);
            }
            foreach(var key in order)if(cells[key].Count>most){var sum=Vector2.zero;foreach(var p in cells[key])sum+=p;most=cells[key].Count;best=sum/most;}
            if(dense&&most>0) {
                var nearby=new List<Vector2>();
                foreach(var coin in game.Coins)if(coin&&!coin.Busy&&Vector2.Distance(best,coin.Rect.anchoredPosition)<=300)nearby.Add(coin.Rect.anchoredPosition);
                if(nearby.Count>0)return nearby;
            }
            return targets;
        }
        static IEnumerator DiceBombs(GameController game,int awake) {
            yield return DiceBomb(game,awake,awake>=2?.5f:.7f);
            if(awake>=2){yield return new WaitForSeconds(.3f);yield return DiceBomb(game,awake,.5f);}
        }
        static IEnumerator DiceBomb(GameController game,int awake,float fallDuration) {
            var half=game.Field?game.Field.rect.size*.5f:new Vector2(720,640);
            Vector2 hit=game.ClampScene(new Vector2(UnityEngine.Random.Range(-half.x+100,half.x-100),UnityEngine.Random.Range(-half.y+100,half.y-100)),0);
            var visual=Vat(game,hit+Vector2.up*1500,"zgygy_roleskill_dushen",200);
            visual.Play("idle",true);
            yield return Move((RectTransform)visual.transform,hit+Vector2.up*1500,hit,fallDuration,t=>1-(1-t)*(1-t));
            if(!visual)yield break;
            int point=awake>=3?6:UnityEngine.Random.Range(1,7);float reward=awake>=1?point:1;
            visual.Play("num_"+point,false,()=>{if(visual)UnityEngine.Object.Destroy(visual.gameObject);});
            TossAround(game,hit,200,10,10,false,0,reward,1,false);
            Run(game,DiceMoves(game,hit,reward));
        }
        static IEnumerator DiceMoves(GameController game,Vector2 at,float reward) {
            yield return new WaitForSeconds(.3f);
            var role=game.Model.HelperConfig(10);var state=game.Model.HelperOf(10);
            float distance=DiceMoveDistance(role,state==null?1:state.level);
            foreach(var dir in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right}) {
                float size=DiceMoveWidth(game.Model);
                var shot=Shot(game,at,dir,600,distance,100*size,10,reward,null,-1,"zgygy_roleskill_dushen_1");shot.SetMode(SkillShot.Kind.Dice);
                shot.transform.localScale=Vector3.one*(.4f*size);
            }
        }
        public static void RepeatHaalandThrow(GameController game,CoinActor coin) {
            if(!game||!coin||coin.World!=1)return;
            int world=game.Model.Data.world;
            Action<CoinActor> landed=null;
            landed=flipped=>{
                // Remove before rethrowing: this is one additional flip, never a recurring effect.
                flipped.Landed-=landed;
                if(game&&flipped&&!flipped.Busy&&game.Model.Data.world==world)Toss(game,flipped,9);
            };
            coin.Landed+=landed;
        }
        public static List<CoinActor> Nearby(GameController game,Vector2 at,float radius,bool richFirst) {
            var list=new List<CoinActor>();
            foreach(var coin in game.Coins)if(coin&&!coin.Busy&&game.Model.CanHelperFlip(coin.Quality)&&Vector2.Distance(at,coin.Rect.anchoredPosition)<=radius)list.Add(coin);
            if(richFirst)list.Sort((a,b)=>b.Quality.CompareTo(a.Quality));
            return list;
        }
        public static bool Toss(GameController game,CoinActor coin,int role,float reward=1,bool suppressLandingEffects=false) {
            if(!coin||coin.Busy)return false;
            // Native DicePoint belongs to CoinVat; DiceVat has no bomb-point multiplier in its settlement path.
            if(role==10&&coin.World!=1)reward=1;
            var at=coin.Rect.anchoredPosition;
            // Skill tosses scatter locally; the base throw uses the world's ordinary landing rule.
            var landing=role==6?at:game.ClampScene(at+UnityEngine.Random.insideUnitCircle*50,0);
            int awake=game.Model.HelperAwake(role);
            bool launched=coin.Launch(landing,false,role,0,reward,coin.World==1&&role==6&&awake>0?2:1,coin.World==1&&role==6&&awake>1,suppressLandingEffects);
            if(launched&&coin.World==1&&(role==4||role==6))coin.SetThrowDelayTimer(0);
            return launched;
        }
        public static void TossAround(GameController game,Vector2 center,float radius,int max,int role,bool richFirst,float critAdd,float rewardScale,float speed,bool instant) {
            HelperTossQueue queue=null;
            if(role==8||role==10){queue=game.SmokePool.GetComponentInChildren<HelperTossQueue>();if(!queue){var rt=Rect(game,"Skill coin toss queue",Vector2.zero);queue=rt.gameObject.AddComponent<HelperTossQueue>();queue.Begin(game);}}
            foreach(var coin in Nearby(game,center,radius,richFirst)) {
                if(max<=0)break;
                bool accepted=queue&&coin.World==1?queue.Enqueue(coin,role,rewardScale):Toss(game,coin,role,rewardScale);
                if(accepted)max--;
            }
        }
        static SkillShot Shot(GameController game,Vector2 from,Vector2 dir,float speed,float distance,float hit,int role,float reward,string frames,int start,string vat=null) {
            string prefab=role==2?(start==0?"coins_tornado2":"coins_tornado"):role==5?"lanqiu":role==7?"aixinqiu":role==9?"zuqiu":null;
            var rt=prefab==null?Rect(game,"Skill",from):Prefab(game,prefab,from,false);var shot=rt.gameObject.AddComponent<SkillShot>();
            shot.Begin(game,dir,speed,distance,hit,0,role,0,reward,1,false,frames,start,null,vat);return shot;
        }
        public static RectTransform Rect(GameController game,string name,Vector2 at) {
            var rt=GameController.NewRect(name,game.SmokePool);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=at;return rt;
        }
        static VatGraphic Vat(GameController game,Vector2 at,string asset,float scale) {
            var rt=Rect(game,asset,at);var visual=VatGraphic.AddTo(rt.gameObject);visual.raycastTarget=false;visual.PixelsPerUnit=scale;visual.Load(asset);return visual;
        }
        static RectTransform SpriteEffect(GameController game,Vector2 at,string prefix,int start,float scale) {
            var rt=Prefab(game,prefix=="bullet_gangtieren_1_"?"iron_bullet":"iron_bullet2",at,false);rt.localScale=Vector3.one*scale;return rt;
        }
        static void Burst(GameController game,Vector2 at,string prefix,int start,float scale) {
            string prefab=prefix=="zgygy_roleskill_magician_"?"magic_ani":prefix=="zgygy_roleskill_gangtieren_1_"?"iron_boom":"iron_boom2";
            var rt=Prefab(game,prefab,at,true);rt.localScale=Vector3.one*scale;
        }
        static RectTransform Prefab(GameController game,string name,Vector2 at,bool once) {
            var view=UiPrefab.Instantiate("HelperSkills/"+name,game.SmokePool);
            var rt=view.Rect;rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);rt.anchoredPosition=at;
            for(int i=0;i<view.Nodes.Length;i++) {
                var node=view.Nodes[i];var sequence=node.GetComponent<UISpriteAnimationSequence>();
                var data=view.Script("UISpriteAnimationSequence",i);
                if(data==null)continue;
                if(sequence)sequence.enabled=false;
                var image=node.GetComponent<Image>();if(!image)image=node.AddComponent<Image>();
                var frames=new List<Sprite>();foreach(Dictionary<string,object> item in data.Arr("spriteFrames")){var sprite=UiAssets.Sprite(item.Str("sprite"));if(sprite)frames.Add(sprite);}
                var animation=node.AddComponent<FrameAnim>();animation.ResizeFrames=false;
                animation.Configure(image,frames.ToArray(),data.F("frameInterval",.04f),once,data.B("isLooping",true));
            }
            foreach(var graphic in rt.GetComponentsInChildren<Graphic>(true))graphic.raycastTarget=false;
            return rt;
        }
        static IEnumerator Move(RectTransform rt,Vector2 from,Vector2 to,float duration,Func<float,float> ease) {
            float time=0;
            while(rt&&time<duration){time+=Time.deltaTime;rt.anchoredPosition=Vector2.LerpUnclamped(from,to,ease(Mathf.Clamp01(time/duration)));yield return null;}
            if(rt)rt.anchoredPosition=to;
        }
        public static void Run(GameController game,IEnumerator routine) {
            var rt=Rect(game,"Helper skill sequence",Vector2.zero);var run=rt.gameObject.AddComponent<HelperSkillSequence>();run.Begin(game,routine);
        }
        static Vector2 Rotate(Vector2 d,float degrees){float r=degrees*Mathf.Deg2Rad,c=Mathf.Cos(r),s=Mathf.Sin(r);return new Vector2(d.x*c-d.y*s,d.x*s+d.y*c);}
    }

    public sealed class HelperSkillSequence:MonoBehaviour {
        GameController game;int world;
        public void Begin(GameController game,IEnumerator routine){this.game=game;world=game.Model.Data.world;StartCoroutine(Sequence(routine));}
        IEnumerator Sequence(IEnumerator routine){yield return routine;if(this)Destroy(gameObject);}
        void Update(){if(!game||game.Model.Data.world!=world)Destroy(gameObject);}
    }

    // MainSceneVat batches at most sixteen queued skill tosses per frame and deduplicates coins.
    public sealed class HelperTossQueue:MonoBehaviour {
        struct Entry {public CoinActor Coin;public int Role;public float Reward;}
        readonly Queue<Entry> pending=new Queue<Entry>();readonly HashSet<CoinActor> reserved=new HashSet<CoinActor>();
        GameController game;int world;
        public void Begin(GameController game){this.game=game;world=game.Model.Data.world;}
        public bool Enqueue(CoinActor coin,int role,float reward) {
            if(!coin||coin.Busy||!reserved.Add(coin))return false;
            pending.Enqueue(new Entry{Coin=coin,Role=role,Reward=reward});return true;
        }
        void Update() {
            if(!game||game.Model.Data.world!=world){Destroy(gameObject);return;}
            Tick();
        }
        public void Tick() {
            int budget=16;
            while(budget>0&&pending.Count>0){var next=pending.Dequeue();reserved.Remove(next.Coin);if(HelperSkills.Toss(game,next.Coin,next.Role,next.Reward,true))budget--;}
        }
    }

    public sealed class SkillShot:MonoBehaviour {
        public enum Kind{Tornado,Basketball,Heart,Football,Dice}
        public Kind mode;public int bounces;public bool comeBack;public float suckRadius;public Action<Vector2> done;
        GameController game;int world,role;Vector2 direction,origin;float speed,remaining,hitRadius,reward=1,pause=-1,timer;bool returning;
        public bool Finished {get;private set;}
        readonly HashSet<int> seen=new HashSet<int>();readonly Dictionary<int,Vector2> suckOffsets=new Dictionary<int,Vector2>();
        public void SetMode(Kind kind){mode=kind;timer=kind==Kind.Dice?.02f:0;RotateVisual();}
        public void Begin(GameController game,Vector2 direction,float speed,float distance,float hit,int bounces,int role,float critAdd,float reward,float tossSpeed,bool instant,string frames,int frameStart,Action<Vector2> done,string vatAsset=null) {
            this.game=game;world=game.Model.Data.world;this.direction=direction.normalized;this.speed=speed;remaining=distance;hitRadius=hit;this.bounces=bounces;this.role=role;this.reward=reward;this.done=done;
            origin=((RectTransform)transform).anchoredPosition;
            if(!string.IsNullOrEmpty(vatAsset)) {
                var visual=VatGraphic.AddTo(gameObject);visual.raycastTarget=false;visual.PixelsPerUnit=100;visual.Load(vatAsset);visual.Play("effect",true);
            } else if(GetComponentsInChildren<Graphic>(true).Length==0) {
                var image=gameObject.AddComponent<Image>();image.raycastTarget=false;image.enabled=false;
                if(frameStart>=0)gameObject.AddComponent<FrameAnim>().Begin(image,frames,frameStart,false);
                else {var sprite=UiAssets.Sprite(frames);if(sprite){image.sprite=sprite;image.SetNativeSize();image.enabled=true;}}
            }
            RotateVisual();
        }
        void RotateVisual(){if(mode==Kind.Basketball||mode==Kind.Football||mode==Kind.Dice)transform.localEulerAngles=new Vector3(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);}
        void Update(){Tick(Time.deltaTime);}
        public void Tick(float delta) {
            if(Finished)return;
            if(!game||game.Model.Data.world!=world){Destroy(gameObject);return;}
            var rt=(RectTransform)transform;
            if(pause>=0) {
                pause+=delta;if(suckRadius>0)Suck(delta);
                if(pause>=.5f){if(!comeBack){Finish();return;}pause=-1;returning=true;seen.Clear();suckOffsets.Clear();}
                return;
            }
            float step=speed*delta;
            if(returning) {
                rt.anchoredPosition=Vector2.MoveTowards(rt.anchoredPosition,origin,step);Hit(rt.anchoredPosition);
                if(Vector2.Distance(rt.anchoredPosition,origin)<.01f)Finish();return;
            }
            Vector2 next=rt.anchoredPosition+direction*step;
            if(mode==Kind.Basketball) {
                var clamped=game.ClampScene(next,30);bool hitWall=(next-clamped).sqrMagnitude>.001f;
                if(hitWall){if(Mathf.Abs(next.x-clamped.x)>.001f)direction.x=-direction.x;if(Mathf.Abs(next.y-clamped.y)>.001f)direction.y=-direction.y;next=clamped;if(bounces--<=0){Finish();return;}RotateVisual();}
            } else if(mode==Kind.Football) {
                // Native football expires only once it leaves the boundary plus its thirty-pixel margin.
                if((game.ClampScene(next,-30)-next).sqrMagnitude>.001f){Finish();return;}
            }
            rt.anchoredPosition=next;remaining-=step;
            // TornadoAnimation ends its movement before the final frame's collision poll.
            if(mode==Kind.Tornado&&remaining<=0){Finish();return;}
            timer+=delta;
            float interval=mode==Kind.Tornado?.1f:mode==Kind.Dice?.02f:0;
            if(timer>=interval){timer=0;Hit(next);}
            if(mode==Kind.Basketball||mode==Kind.Football)return;
            if(remaining<=0){if(mode==Kind.Heart)pause=0;else Finish();}
        }
        void Hit(Vector2 p) {
            bool limited=mode==Kind.Basketball||mode==Kind.Football;
            if(limited&&seen.Count>=999){Finish();return;}
            foreach(var coin in game.Coins) {
                if(!coin||coin.Busy||!game.Model.CanHelperFlip(coin.Quality))continue;
                int id=coin.GetInstanceID();if((mode!=Kind.Tornado&&seen.Contains(id))||Vector2.Distance(p,coin.Rect.anchoredPosition)>hitRadius)continue;
                if(HelperSkills.Toss(game,coin,role,reward)&&mode!=Kind.Tornado) {
                    seen.Add(id);
                    if(limited&&seen.Count>=999){Finish();break;}
                }
            }
        }
        void Suck(float delta) {
            var at=((RectTransform)transform).anchoredPosition;
            foreach(var coin in HelperSkills.Nearby(game,at,suckRadius,false)) {
                int id=coin.GetInstanceID();if(seen.Contains(id))continue;
                if(!suckOffsets.TryGetValue(id,out var offset)){offset=UnityEngine.Random.insideUnitCircle.normalized*UnityEngine.Random.Range(40f,100f);suckOffsets[id]=offset;}
                Vector2 target=game.ClampScene(at+offset,40);
                coin.Rect.anchoredPosition=game.ClampScene(Vector2.MoveTowards(coin.Rect.anchoredPosition,target,400*delta),40);
            }
        }
        void Finish(){Finished=true;done?.Invoke(((RectTransform)transform).anchoredPosition);if(Application.isPlaying)Destroy(gameObject);else enabled=false;}
    }

    public sealed class FrameAnim:MonoBehaviour {
        Image image;Sprite[] frames;float timer;int index;bool once;
        public bool Loop=true;public bool Finished {get;private set;}public Action Completed;
        public bool ResizeFrames=true;public float Interval=.04f;public float Duration=>frames==null?0:frames.Length*Interval;
        public void Configure(Image image,Sprite[] frames,float interval,bool destroyAtEnd,bool repeat=true) {
            this.image=image;this.frames=frames;Interval=interval;once=destroyAtEnd;Loop=repeat&&!destroyAtEnd;Finished=false;index=0;timer=0;image.enabled=frames!=null&&frames.Length>0;
            if(image.enabled){image.sprite=frames[0];if(ResizeFrames)image.SetNativeSize();}
        }
        public void Begin(Image image,string prefix,int start,bool destroyAtEnd) {
            this.image=image;image.enabled=false;once=destroyAtEnd;Loop=!destroyAtEnd;Finished=false;var list=new List<Sprite>();
            for(int i=0;i<256;i++){var sprite=UiAssets.Sprite(prefix+(start+i).ToString("00"));if(!sprite)break;list.Add(sprite);}
            frames=list.ToArray();if(frames.Length>0){image.sprite=frames[0];if(ResizeFrames)image.SetNativeSize();image.enabled=true;}
        }
        void Update(){Tick(Time.deltaTime);}
        public void Tick(float delta) {
            if(Finished)return;
            if(frames==null||frames.Length==0){if(once)Destroy(gameObject);return;}
            timer+=delta;
            if(timer<Interval)return;timer=0;index++;
            if(index>=frames.Length) {
                if(!Loop) {
                    index=frames.Length-1;Finished=true;
                    var callback=Completed;Completed=null;callback?.Invoke();
                    if(once&&Application.isPlaying)Destroy(gameObject);
                    return;
                }
                index=0;
            }
            image.sprite=frames[index];
        }
    }
    public sealed class SkillDelay:MonoBehaviour {
        public static void After(GameController game,float seconds,Action action){HelperSkills.Run(game,Wait(seconds,action));}
        static IEnumerator Wait(float seconds,Action action){yield return new WaitForSeconds(seconds);action?.Invoke();}
    }
}
