using System;
using System.Collections.Generic;
using System.Linq;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Bufu.Rebuild {
    public sealed partial class GameController {
        // ChallengeMain uses the old Role/CoinBase path, not RoleVat. In particular there
        // is no role-10 extra skill, no critical payout and no dice-world cooldown talent.
        enum ActivityHelperStep {Idle,NoCoin,Walking,WaitFlip,WaitAnimation,AfterThrow,Cooldown,Sleeping}
        sealed class ActivityHelperState {
            public ActivityHelperStep Step;public float Timer,SleepTimer;public bool Front=true,FlipComplete;
            public Vector2 Stop;public ActivityCoinVisual Subscribed;public Action<bool> Handler;
        }
        sealed class ActivitySkillVisual {
            public RectTransform Rect;public int Role,Kind,Bounces;public Vector2 Direction,Origin;
            public float Speed,Remaining,Radius,Timer,Pause=-1,SuckRadius;public bool Return,Returning;
            public readonly HashSet<ActivityCoinVisual> Hits=new HashSet<ActivityCoinVisual>();
            public readonly Dictionary<ActivityCoinVisual,Vector2> SuckOffsets=new Dictionary<ActivityCoinVisual,Vector2>();
        }
        sealed class ActivitySkillMotion {public RectTransform Rect;public float Elapsed,Duration;public Action<float> Apply;public Action Complete;}
        sealed class ActivityRepeatHandler {public ActivityCoinVisual Coin;public Action<bool> Handler;}
        readonly Dictionary<ActivityHelperVisual,ActivityHelperState> activityHelperStates=new Dictionary<ActivityHelperVisual,ActivityHelperState>();
        readonly List<ActivitySkillVisual> activitySkillShots=new List<ActivitySkillVisual>();
        readonly List<ActivitySkillMotion> activitySkillMotions=new List<ActivitySkillMotion>();
        readonly List<FrameAnim> activitySkillFrames=new List<FrameAnim>();
        readonly HashSet<RectTransform> activitySkillObjects=new HashSet<RectTransform>();
        readonly List<ActivityRepeatHandler> activityRepeatHandlers=new List<ActivityRepeatHandler>();
        static float activityLastIron=-60;

        // Actual RoleSpine prefab fields, not Role.cs defaults. The magician is the
        // sole prefab with a different walk/sleep/interaction profile.
        // Evidence: Tools/ParityAssets/prefab-actor-parameters.json, role_type 0..10.
        static float ActivityHelperParameter(int role,string field) {
            bool magician=role==3;
            switch(field) {
                case "baseMoveSpeed":return magician?500:400;
                case "reachThreshold":return 10;
                case "coinMinDistance":return magician?60:75;
                case "waitAfterThrow":return magician?.5f:.8f;
                case "sleepCheckInterval":return magician?5:7;
                case "sleepProbability":return .3f;
                case "sleepDuration":return magician?3:5;
                case "scatterRadius":return 50;
                case "halandradius":return 120;
                default:throw new ArgumentOutOfRangeException(nameof(field),field,"Unknown original challenge helper parameter");
            }
        }
        int ActivityHelperAwake(int id) {
            var role=Model.HelperConfig(id);int level=Model.GlobalHelperLevel(id);
            if(role?.awakeSkillNum==null)return 0;
            for(int i=role.awakeSkillNum.Length-1;i>=0;i--)if(level>=role.awakeSkillNum[i])return i+1;
            return 0;
        }
        float ActivityHelperSkillValue(int id) {
            var role=Model.HelperConfig(id);return role==null?1:role.baseSkillNum+GameModel.SkillRank(role,Model.GlobalHelperLevel(id))*role.upgradeSkillNum;
        }
        void InitializeActivitySkills() {
            ClearActivitySkills();
            foreach(var helper in activityHelpers) {
                var state=new ActivityHelperState();activityHelperStates.Add(helper,state);
                var button=helper.Rect.gameObject.GetComponent<Button>()??helper.Rect.gameObject.AddComponent<Button>();button.targetGraphic=helper.Vat;
                Click(button,()=>{if(state.Step==ActivityHelperStep.Sleeping){state.SleepTimer=0;ActivityHelperPhase(helper,state,ActivityHelperStep.Idle);PlayRevive();}});
                helper.Vat.raycastTarget=false;ActivityHelperAnimation(helper,state,"idle",true);
            }
        }
        void ClearActivitySkills() {
            foreach(var state in activityHelperStates.Values)ActivityUnsubscribe(state);
            foreach(var repeat in activityRepeatHandlers)repeat.Coin.Landed-=repeat.Handler;activityRepeatHandlers.Clear();
            activityHelperStates.Clear();activitySkillShots.Clear();activitySkillMotions.Clear();activitySkillFrames.Clear();
            foreach(var effect in activitySkillObjects)if(effect)Destroy(effect.gameObject);activitySkillObjects.Clear();
        }
        void ActivityUnsubscribe(ActivityHelperState state) {
            if(state.Subscribed!=null&&state.Handler!=null)state.Subscribed.Landed-=state.Handler;
            state.Subscribed=null;state.Handler=null;
        }
        void ActivityHelperAnimation(ActivityHelperVisual helper,ActivityHelperState state,string stem,bool loop) {
            string clip=stem=="sleep"?"sleep":stem+(state.Front?"_front":"_back");
            if(helper.Animation==clip)return;helper.Animation=clip;helper.Vat.Play(clip,loop);
        }
        void ActivityHelperPhase(ActivityHelperVisual helper,ActivityHelperState state,ActivityHelperStep step) {
            state.Step=step;state.Timer=0;helper.Vat.raycastTarget=step==ActivityHelperStep.Sleeping;
            if(step==ActivityHelperStep.Sleeping){helper.Target=null;ActivityHelperAnimation(helper,state,"sleep",true);}
            else if(step!=ActivityHelperStep.Walking&&step!=ActivityHelperStep.WaitFlip&&step!=ActivityHelperStep.WaitAnimation)ActivityHelperAnimation(helper,state,"idle",true);
        }
        bool ActivityCheckSleep(ActivityHelperVisual helper,ActivityHelperState state,float dt) {
            state.SleepTimer+=dt;
            if(state.SleepTimer<ActivityHelperParameter(helper.Id,"sleepCheckInterval"))return false;state.SleepTimer=0;
            if(UnityEngine.Random.value>=ActivityHelperParameter(helper.Id,"sleepProbability"))return false;
            ActivityHelperPhase(helper,state,ActivityHelperStep.Sleeping);return true;
        }
        void TickActivityHelpers(float dt) {
            foreach(var helper in activityHelpers) {
                if(!helper.Rect||!activityHelperStates.TryGetValue(helper,out var state))continue;
                state.Timer+=dt;
                switch(state.Step) {
                    case ActivityHelperStep.Idle:
                    case ActivityHelperStep.NoCoin:
                        if(ActivityCheckSleep(helper,state,dt))break;
                        if(state.Timer<(state.Step==ActivityHelperStep.Idle?.2f:1f))break;
                        if(state.Step==ActivityHelperStep.NoCoin){ActivityHelperPhase(helper,state,ActivityHelperStep.Idle);break;}
                        helper.Target=ActivityFindCoin(helper);
                        if(helper.Target==null){ActivityHelperPhase(helper,state,ActivityHelperStep.NoCoin);break;}
                        var delta=helper.Rect.anchoredPosition-helper.Target.Rect.anchoredPosition;
                        float stopDistance=ActivityHelperParameter(helper.Id,"coinMinDistance");
                        state.Stop=delta.magnitude<stopDistance?helper.Rect.anchoredPosition:helper.Target.Rect.anchoredPosition+delta.normalized*stopDistance;
                        state.Front=helper.Rect.anchoredPosition.y>=state.Stop.y;
                        var scale=helper.Rect.localScale;scale.x=Mathf.Abs(scale.x)*(state.Stop.x<helper.Rect.anchoredPosition.x?-1:1);helper.Rect.localScale=scale;
                        ActivityHelperPhase(helper,state,ActivityHelperStep.Walking);ActivityHelperAnimation(helper,state,"walk",true);break;
                    case ActivityHelperStep.Walking:
                        var to=state.Stop-helper.Rect.anchoredPosition;
                        if(to.magnitude>=ActivityHelperParameter(helper.Id,"reachThreshold")) {
                            // Role.TickWalking halves only the Y component after normalization.
                            var direction=to.normalized;direction.y*=.5f;
                            // Old UpgradeManager.GetRoleSpeedMultiplier always reads upgrade 9,
                            // even when the challenge was entered from the dice-world page.
                            var speed=ActivityHelperParameter(helper.Id,"baseMoveSpeed")*(1+Model.Data.worlds[0].boosts[9]*(float)Model.Config(9,true).upgradeResult)*Model.SupportSpeedMultiplier;
                            helper.Rect.anchoredPosition=ClampActivityPosition(helper.Rect.anchoredPosition+direction*speed*dt,30);break;
                        }
                        helper.Rect.anchoredPosition=ClampActivityPosition(state.Stop,30);
                        if(ActivityCheckSleep(helper,state,dt))break;
                        ActivityBeginHelperThrow(helper,state);break;
                    case ActivityHelperStep.WaitFlip:
                        if(state.FlipComplete||state.Timer>=2.8f){ActivityUnsubscribe(state);ActivityHelperPhase(helper,state,ActivityHelperStep.WaitAnimation);}break;
                    case ActivityHelperStep.WaitAnimation:
                        if(state.Timer>=.3f)ActivityHelperPhase(helper,state,ActivityHelperStep.AfterThrow);break;
                    case ActivityHelperStep.AfterThrow:
                        if(ActivityCheckSleep(helper,state,dt))break;
                        if(state.Timer>=ActivityHelperParameter(helper.Id,"waitAfterThrow"))ActivityHelperPhase(helper,state,ActivityHelperStep.Cooldown);break;
                    case ActivityHelperStep.Cooldown:
                        if(ActivityCheckSleep(helper,state,dt))break;
                        if(state.Timer>=Mathf.Max(0,Model.HelperConfig(helper.Id)?.cdTime??0))ActivityHelperPhase(helper,state,ActivityHelperStep.Idle);break;
                    case ActivityHelperStep.Sleeping:
                        if(state.Timer>=ActivityHelperParameter(helper.Id,"sleepDuration")){state.SleepTimer=0;ActivityHelperPhase(helper,state,ActivityHelperStep.Idle);}break;
                }
            }
        }
        ActivityCoinVisual ActivityFindCoin(ActivityHelperVisual helper) {
            var idle=activityCoins.Where(c=>c.Rect&&!c.Busy).ToArray();
            if(helper.Id==1&&ActivityHelperAwake(1)>0)return idle.OrderBy(c=>(c.Rect.anchoredPosition-helper.Rect.anchoredPosition).sqrMagnitude).FirstOrDefault();
            for(float radius=900;radius<=2000;radius+=300) {
                var candidates=idle.Where(c=>Vector2.Distance(c.Rect.anchoredPosition,helper.Rect.anchoredPosition)<=radius).ToArray();
                if(candidates.Length>0)return candidates[UnityEngine.Random.Range(0,candidates.Length)];
            }
            return null;
        }
        void ActivityBeginHelperThrow(ActivityHelperVisual helper,ActivityHelperState state) {
            ActivityUnsubscribe(state);state.FlipComplete=false;ActivityHelperPhase(helper,state,ActivityHelperStep.WaitFlip);
            var coin=helper.Target;
            if(coin==null||!coin.Rect){ActivityHelperPhase(helper,state,ActivityHelperStep.WaitAnimation);return;}
            state.Front=helper.Rect.anchoredPosition.y>=coin.Rect.anchoredPosition.y;
            var scale=helper.Rect.localScale;scale.x=Mathf.Abs(scale.x)*(coin.Rect.anchoredPosition.x<helper.Rect.anchoredPosition.x?-1:1);helper.Rect.localScale=scale;
            state.Subscribed=coin;state.Handler=front=>state.FlipComplete=true;coin.Landed+=state.Handler;
            ThrowActivityCoin(coin,helper.Id,1,true,.3f);
            CastActivitySkill(helper,coin);ActivityHelperAnimation(helper,state,"throw",false);helper.Target=null;
            if(helper.Id==9&&ActivityHelperAwake(9)>=1)ActivityRegisterHaalandRepeat(coin);
        }
        void ActivityRegisterHaalandRepeat(ActivityCoinVisual coin) {
            var repeat=new ActivityRepeatHandler{Coin=coin};
            repeat.Handler=front=>{
                coin.Landed-=repeat.Handler;activityRepeatHandlers.Remove(repeat);
                // CoinBase's ensuing ResetToIdle cancels this attempted callback-time
                // rethrow. Preserve its coinup/prepare effect without another payout.
                if(coin.Rect&&!coin.Busy)TossActivitySkill(coin,9);
            };
            activityRepeatHandlers.Add(repeat);coin.Landed+=repeat.Handler;
        }
        bool TossActivitySkill(ActivityCoinVisual coin,int role) {
            if(coin==null||!coin.Rect||coin.Busy)return false;
            var at=coin.Rect.anchoredPosition;
            return ThrowActivityCoin(coin,role,1,role==6,role==4||role==6?0:.3f,role==6?at:at+UnityEngine.Random.insideUnitCircle*ActivityHelperParameter(role,"scatterRadius"));
        }
        IEnumerable<ActivityCoinVisual> ActivityNearby(Vector2 at,float radius)=>activityCoins.Where(c=>c.Rect&&!c.Busy&&Vector2.Distance(at,c.Rect.anchoredPosition)<=radius);
        void TossActivityArea(Vector2 at,float radius,int max,int role) {
            foreach(var coin in ActivityNearby(at,radius).ToArray()) {
                if(max<=0)break;
                if(TossActivitySkill(coin,role)){max--;if(role==9&&ActivityHelperAwake(9)>=1)ActivityRegisterHaalandRepeat(coin);}
            }
        }
        void CastActivitySkill(ActivityHelperVisual helper,ActivityCoinVisual aim) {
            int role=helper.Id,awake=ActivityHelperAwake(role),count=Mathf.Max(1,Mathf.RoundToInt(ActivityHelperSkillValue(role)));
            float factor=Model.HelperConfig(role)?.skillFactor1??1;Vector2 from=helper.Rect.anchoredPosition,to=aim.Rect.anchoredPosition;
            switch(role) {
                case 2: ActivityFan(from,to,count,role,0,awake>0?1500:1000,500*factor*(awake>0?1.5f:1),15,150,awake>0?"coins_tornado2":"coins_tornado",2,0,false,0);break;
                case 3:
                    foreach(var coin in ActivityNearby(from,factor*200).ToArray()){if(count--<=0)break;if(TossActivitySkill(coin,3))ActivitySkillPrefab("magic_ani",coin.Rect.anchoredPosition,2,true);}break;
                case 4: ActivityPalm(to,4,awake);break;
                case 5: ActivityFan(from,to,count,role,1,1000,0,30,60,"lanqiu",1,HelperSkills.BasketballBounces(awake),false,0);break;
                case 6:
                    foreach(var coin in ActivityNearby(from,Mathf.Max(300,factor*100)).ToArray()) {
                        if(count<=0)break;if(Vector2.Distance(from,coin.Rect.anchoredPosition)<=100)continue;count--;
                        ActivityTentacle(from+Vector2.up*100,coin);
                    }break;
                case 7: ActivityFan(from,to,count,role,2,800,factor>0?500*factor:500,15,80,"aixinqiu",1.5f,0,awake>0,awake>1?150:0);break;
                case 8: ActivityIron(from,count,awake);break;
                case 9:
                    ActivityPalm(to,9,awake);
                    if(awake>=2)ActivityFan(from,to,awake>=3?2:1,role,3,1000,0,30,60,"zuqiu",1,0,false,0);break;
            }
        }
        void ActivityPalm(Vector2 at,int role,int awake) {
            float scale=awake>1?2.5f:awake>0?2:1.5f;
            var rect=NewRect("Challenge "+(role==4?"Palm":"Haaland"),ActivityEffectParent());rect.anchoredPosition=at;activitySkillObjects.Add(rect);
            var vat=VatGraphic.AddTo(rect.gameObject);vat.raycastTarget=false;vat.PixelsPerUnit=100*scale;vat.Load(role==4?"zgygy_roleskill_kungfu":"zgygy_roleskill_halande");
            vat.Play("skill",false,()=>{if(rect)Destroy(rect.gameObject);});
            TossActivityArea(at,ActivityHelperSkillValue(role)*(role==9?ActivityHelperParameter(role,"halandradius"):120)*scale,10,role);
        }
        Transform ActivityEffectParent()=>activityArena.Ref<RectTransform>("ChallengeMain","effectPool");
        RectTransform ActivitySkillPrefab(string name,Vector2 at,float scale,bool once) {
            var view=UiPrefab.Instantiate("HelperSkills/"+name,ActivityEffectParent());var rect=view.Rect;
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.anchoredPosition=at;rect.localScale=Vector3.one*scale;activitySkillObjects.Add(rect);
            for(int i=0;i<view.Nodes.Length;i++) {
                var data=view.Script("UISpriteAnimationSequence",i);if(data==null)continue;
                var node=view.Nodes[i];var source=node.GetComponent<UISpriteAnimationSequence>();if(source)source.enabled=false;
                var image=node.GetComponent<Image>()??node.AddComponent<Image>();var frames=new List<Sprite>();
                foreach(Dictionary<string,object> item in data.Arr("spriteFrames")){var sprite=UiAssets.Sprite(item.Str("sprite"));if(sprite)frames.Add(sprite);}
                var animation=node.AddComponent<FrameAnim>();animation.ResizeFrames=false;animation.Configure(image,frames.ToArray(),data.F("frameInterval",.04f),false,once?false:data.B("isLooping",true));
                animation.enabled=false;activitySkillFrames.Add(animation);
                if(once)animation.Completed=()=>{if(rect)Destroy(rect.gameObject);};
            }
            foreach(var graphic in rect.GetComponentsInChildren<Graphic>(true))graphic.raycastTarget=false;
            return rect;
        }
        void ActivityTentacle(Vector2 from,ActivityCoinVisual coin) {
            var delta=coin.Rect.anchoredPosition-from;var rect=ActivitySkillPrefab("dashehandani",from,1,false);
            rect.pivot=new Vector2(0,.5f);rect.sizeDelta=new Vector2(delta.magnitude,210);rect.localEulerAngles=new Vector3(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
            rect.localScale=new Vector3(1,UnityEngine.Random.value>.5f?-1:1,1);
            var animation=rect.GetComponent<FrameAnim>();animation.Completed=()=>{TossActivitySkill(coin,6);if(rect)Destroy(rect.gameObject);};
        }
        void ActivityFan(Vector2 from,Vector2 to,int count,int role,int kind,float speed,float distance,float spread,float radius,string prefab,float scale,int bounces,bool returns,float suck) {
            var direction=(to-from).normalized;if(direction==Vector2.zero)direction=Vector2.right;
            for(int i=0;i<count;i++) {
                float angle=(i-(count-1)*.5f)*spread*Mathf.Deg2Rad;var dir=new Vector2(direction.x*Mathf.Cos(angle)-direction.y*Mathf.Sin(angle),direction.x*Mathf.Sin(angle)+direction.y*Mathf.Cos(angle));
                var rect=ActivitySkillPrefab(prefab,from,scale,false);
                var shot=new ActivitySkillVisual{Rect=rect,Role=role,Kind=kind,Direction=dir,Origin=from,Speed=speed,Remaining=distance,Radius=radius,Bounces=bounces,Return=returns,SuckRadius=suck};
                ActivityRotateShot(shot);activitySkillShots.Add(shot);
            }
        }
        void ActivityRotateShot(ActivitySkillVisual shot) {if(shot.Kind==1||shot.Kind==3)shot.Rect.localEulerAngles=new Vector3(0,0,Mathf.Atan2(shot.Direction.y,shot.Direction.x)*Mathf.Rad2Deg);}
        void TickActivitySkills(float dt) {
            // These are driven with the challenge clock so a skill-choice pause freezes
            // tentacle callbacks and missile motion as well as ordinary coin flights.
            for(int i=activitySkillFrames.Count-1;i>=0;i--){var frame=activitySkillFrames[i];if(!frame)activitySkillFrames.RemoveAt(i);else frame.Tick(dt);}
            for(int i=activitySkillMotions.Count-1;i>=0;i--) {
                var motion=activitySkillMotions[i];if(!motion.Rect){activitySkillMotions.RemoveAt(i);continue;}
                motion.Elapsed+=dt;motion.Apply(Mathf.Clamp01(motion.Elapsed/motion.Duration));
                if(motion.Elapsed>=motion.Duration){activitySkillMotions.RemoveAt(i);motion.Complete?.Invoke();}
            }
            for(int i=activitySkillShots.Count-1;i>=0;i--) {
                var shot=activitySkillShots[i];if(!shot.Rect||!TickActivityShot(shot,dt)){if(shot.Rect)Destroy(shot.Rect.gameObject);activitySkillShots.RemoveAt(i);}
            }
            activitySkillObjects.RemoveWhere(rect=>!rect);
        }
        bool TickActivityShot(ActivitySkillVisual shot,float dt) {
            if(shot.Pause>=0) {
                shot.Pause+=dt;ActivitySuck(shot,dt);
                if(shot.Pause>=.5f){if(!shot.Return)return false;shot.Pause=-1;shot.Returning=true;shot.Hits.Clear();shot.SuckOffsets.Clear();}return true;
            }
            float step=shot.Speed*dt;var position=shot.Rect.anchoredPosition;
            if(shot.Returning){shot.Rect.anchoredPosition=Vector2.MoveTowards(position,shot.Origin,step);ActivityHitShot(shot);return Vector2.Distance(position,shot.Origin)>step;}
            var next=position+shot.Direction*step;
            if(shot.Kind==1) {
                var bounds=ActivitySceneBounds();bool bounce=false;
                if(next.x<=bounds.xMin+30||next.x>=bounds.xMax-30){shot.Direction.x=-shot.Direction.x;bounce=true;}
                if(next.y<=bounds.yMin+30||next.y>=bounds.yMax-30){shot.Direction.y=-shot.Direction.y;bounce=true;}
                if(bounce){next=ClampActivityPosition(next,30);ActivityRotateShot(shot);if(shot.Bounces--<=0)return false;}
            } else if(shot.Kind==3) {var bounds=ActivitySceneBounds();if(next.x<bounds.xMin-30||next.x>bounds.xMax+30||next.y<bounds.yMin-30||next.y>bounds.yMax+30)return false;}
            shot.Rect.anchoredPosition=next;shot.Remaining-=step;
            if(shot.Kind==0&&shot.Remaining<=0)return false;
            shot.Timer+=dt;if(shot.Kind!=0||shot.Timer>=.1f){shot.Timer=0;ActivityHitShot(shot);}
            if((shot.Kind==1||shot.Kind==3)&&shot.Hits.Count>=999)return false;
            if(shot.Kind==2&&shot.Remaining<=0)shot.Pause=0;
            return true;
        }
        void ActivityHitShot(ActivitySkillVisual shot) {
            foreach(var coin in ActivityNearby(shot.Rect.anchoredPosition,shot.Radius).ToArray()) {
                if(shot.Kind!=0&&shot.Hits.Contains(coin))continue;
                if(shot.Kind!=0)shot.Hits.Add(coin);
                TossActivitySkill(coin,shot.Role);
                if((shot.Kind==1||shot.Kind==3)&&shot.Hits.Count>=999)break;
            }
        }
        void ActivitySuck(ActivitySkillVisual shot,float dt) {
            if(shot.SuckRadius<=0)return;
            foreach(var coin in ActivityNearby(shot.Rect.anchoredPosition,shot.SuckRadius).ToArray()) {
                if(shot.Hits.Contains(coin))continue;
                if(!shot.SuckOffsets.TryGetValue(coin,out var offset)) {
                    var direction=UnityEngine.Random.insideUnitCircle;if(direction==Vector2.zero)direction=Vector2.right;
                    offset=direction.normalized*UnityEngine.Random.Range(40f,100f);shot.SuckOffsets.Add(coin,offset);
                }
                var target=ClampActivityPosition(shot.Rect.anchoredPosition+offset,40);
                coin.Rect.anchoredPosition=ClampActivityPosition(Vector2.MoveTowards(coin.Rect.anchoredPosition,target,400*dt),40);
            }
        }
        void ActivityIron(Vector2 from,int count,int awake) {
            if(awake>2&&Time.time-activityLastIron>=20) {
                activityLastIron=Time.time;var center=ActivitySceneBounds().center;
                var rect=ActivitySkillPrefab("iron_bullet2",center+Vector2.up*1500,1,false);
                activitySkillMotions.Add(new ActivitySkillMotion{Rect=rect,Duration=1,Apply=t=>{if(rect)rect.anchoredPosition=center+Vector2.up*1500*(1-t);},Complete=()=>{if(rect)Destroy(rect.gameObject);ActivitySkillPrefab("iron_boom2",center,2,true);TossActivityArea(center,3000,int.MaxValue,8);}});return;
            }
            for(int i=0;i<count;i++) {
                var target=activityCoins.Count>0?activityCoins[UnityEngine.Random.Range(0,activityCoins.Count)].Rect.anchoredPosition:RandomActivityPosition();
                var start=from+Vector2.right*((i-(count-1)*.5f)*50);float delay=UnityEngine.Random.Range(.1f,.3f);
                var rect=ActivitySkillPrefab("iron_bullet",start,2,false);float duration=delay+.9f;
                activitySkillMotions.Add(new ActivitySkillMotion{Rect=rect,Duration=duration,Apply=t=>{
                    if(!rect)return;float elapsed=t*duration-delay;
                    if(elapsed<0)return;
                    if(elapsed<.5f){float up=elapsed/.5f;rect.anchoredPosition=start+Vector2.up*1500*(1-(1-up)*(1-up));}
                    else {rect.localEulerAngles=new Vector3(0,0,180);rect.anchoredPosition=target+Vector2.up*1500*(1-Mathf.Clamp01((elapsed-.5f)/.4f));}
                },Complete=()=>{if(rect)Destroy(rect.gameObject);ActivitySkillPrefab("iron_boom",target,2,true);TossActivityArea(target,300,5,8);}});
            }
        }
    }
}
