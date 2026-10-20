using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Bufu.Rebuild {
    /// <summary>
    /// CoinVat (coin world) and DiceVat (dice world) of the original game.
    /// All distances are canvas pixels of the 1440 wide design canvas (the original VAT scene uses 100 pixels per world unit).
    /// </summary>
    public sealed class CoinActor:MonoBehaviour {
        public const float S=2f;                 // visual scale of the VAT graphics on the 1440 canvas
        public const float JumpHeight=500f;      // CoinConfig.JumpHeight
        public const float DiceJumpHeight=360f;  // DiceVat.jumpHeight 3.6 world units
        public const float CoinFlipDelay=.3f;    // CoinVat.JumpToTarget: the flip animation starts 0.3 s after take-off
        public const float DiceThrowDelay=.15f;  // DiceVat.throwDelay
        public int Quality,World; public VatGraphic Visual; public GameController Game;
        public bool Busy {get{return flying||Evolving;}}
        public bool Evolving {get;set;}
        public int CurrentFace=>face;
        public event Action<CoinActor> Landed;
        public bool Flying {get{return flying&&delay<=0;}}
        public RectTransform Rect=>(RectTransform)transform;

        bool flying,flipStarted,manual,critical,frontShowing=true,luckyEcho;
        bool suppressLandingEffects;
        double dicePayoutSnapshot=double.NaN;
        int guidedCoinFace=-1;
        Vector2 start,target;float elapsed,delay,echoIn,speedScale=1,rewardScale=1,critAdd;int face=1,throwRole;SmokeEffect smoke;

        public void Init(GameController game,int quality,int world,string asset) {
            Game=game;Quality=quality;World=world;Visual=VatGraphic.AddTo(gameObject);Visual.raycastTarget=true;
            Visual.Load(asset);Visual.PixelsPerUnit=(World==1?95:85)*S;
            ShowFace(1);
        }

        /// <summary>CoinVat.SetCoinClick / DiceWorldController click: a throw by the player at a random landing spot.</summary>
        public bool Throw(bool byHand) {
            if(flying)return false;
            return Launch(Game.RandomLanding(Rect.anchoredPosition),byHand);
        }

        /// <summary>JumpToTarget / TryThrow. A manual throw has no flip delay in the coin world (SetCoinClick sets throwDelayTimer=0).</summary>
        public bool Launch(Vector2 landing,bool byHand,int role=0,float extraCrit=0,float rewardMul=1,float speed=1,bool instant=false,bool suppressLandingEffects=false) {
            if(Busy||Game.IsBackgrounded)return false;
            if(!Game.PrepareDiceThrow(this,byHand))return false;
            if(World==1&&role==6){if(Game.Model.HelperAwake(6)>0)speed=Mathf.Max(speed,2);if(Game.Model.HelperAwake(6)>1)instant=true;}
            this.suppressLandingEffects=suppressLandingEffects;
            throwRole=role;critAdd=extraCrit;rewardScale=Mathf.Max(0,rewardMul);speedScale=Mathf.Max(.2f,speed);
            manual=byHand;start=Rect.anchoredPosition;target=landing;elapsed=0;flipStarted=false;
            RollOutcome();
            if(instant){if(World==1)LandCoin();else LandDice();return true;}
            flying=true;
            if(World==1) {
                delay=byHand?0:CoinFlipDelay;
                Game.PlayCoinSound();
                ShowFace(frontShowing?1:0);
                if(delay<=0){flipStarted=true;PlayFlip();}
                if(byHand)Game.GuideCoinTossed(this);
            } else {
                delay=DiceThrowDelay;
                Game.PlayDiceUp();
                Visual.Play(Visual.Has("rolling")?"rolling":Visual.Data.segments[0].animation,true);
            }
            return true;
        }
        void RollOutcome() {
            guidedCoinFace=-1;
            if(World==1) {
                if(manual&&Game.GuideForcedFace(this,out int guided))guidedCoinFace=guided;
            } else {
                face=Game.RollDiceFace(this);
                dicePayoutSnapshot=Game.Model.SnapshotDiceValue(Quality,face);
            }
        }
        bool RollCritical()=>UnityEngine.Random.value<Mathf.Clamp01(throwRole==1?1f:Game.Model.CritChance()+critAdd);

        void PlayFlip(){Visual.Play(Visual.Has("flip")?"flip":Visual.Data.segments[0].animation,true);}
        public void SetThrowDelayTimer(float seconds){delay=Mathf.Max(0,seconds);if(flying&&World==1&&!flipStarted&&delay<=0){flipStarted=true;PlayFlip();}}

        /// <summary>The idle pose: coins freeze the "flip" clip on its last (front) or first (back) frame, dice show the clip of the face.</summary>
        void ShowFace(int value) {
            if(World==1) {
                frontShowing=value==1;
                if(Visual.Has("front")&&Visual.Has("back"))Visual.Pose(frontShowing?"front":"back");
                else Visual.Pose(Visual.Has("flip")?"flip":Visual.Data.segments[0].animation,frontShowing?1:0);
            } else {
                face=value;
                if(Visual.Has(value.ToString()))Visual.Pose(value.ToString());
                else Visual.Pose(Visual.Data.segments[0].animation,0);
            }
        }

        void Update() {
            if(Game.IsBackgrounded||Evolving)return;
            float dt=Time.deltaTime;
            if(echoIn>0&&!flying){echoIn-=dt;if(echoIn<=0)Echo();}
            if(!flying)return;
            if(World==1) {
                if(!flipStarted){delay-=dt;if(delay<=0){flipStarted=true;PlayFlip();}}
                elapsed+=dt;float duration=Math.Max(.01f,Game.Model.FlightSeconds()/speedScale);float t=Mathf.Clamp01(elapsed/duration);
                Rect.anchoredPosition=Vector2.Lerp(start,target,t)+Vector2.up*(Mathf.Sin(t*Mathf.PI)*JumpHeight);
                if(t>=1)LandCoin();
            } else {
                if(delay>0){delay-=dt;if(delay>0)return;}
                elapsed+=dt;float duration=Math.Max(.01f,Game.Model.FlightSeconds()/speedScale);float t=Mathf.Clamp01(elapsed/duration);
                Rect.anchoredPosition=Vector2.Lerp(start,target,t)+Vector2.up*(Mathf.Sin(t*Mathf.PI)*DiceJumpHeight);
                if(t>=1)LandDice();
            }
        }

        /// <summary>The seconds a helper waits after a throw (IRoleThrowableVat.RoleThrowDuration).</summary>
        public float RoleThrowDuration=>World==1?.8f:DiceThrowDelay+Math.Max(.01f,Game.Model.FlightSeconds());

        void Settle() {
            // The default VAT coin path calculates floating text separately before wallet settlement.
            if(World==1&&!suppressLandingEffects&&(face==1||Game.Model.TalentValue(309)>0)) {
                bool displayCritical=RollCritical();
                double display=Game.Model.SettlementValue(Quality,face,manual,displayCritical,throwRole,rewardScale);
                Game.FloatReward(Rect.anchoredPosition,display,displayCritical,face);
            }
            critical=RollCritical();
            double reward=Game.Model.Settle(World,Quality,face,manual,critical,Time.unscaledTime,throwRole,World==1?1:rewardScale,dicePayoutSnapshot);
            if(World==2&&!suppressLandingEffects)Game.FloatReward(Rect.anchoredPosition,reward,critical,face);
        }

        // CoinVat.FinishThrowAnimation + Land
        void LandCoin() {
            flying=false;Rect.anchoredPosition=Game.ClampScene(target,40);
            face=UnityEngine.Random.value<Game.Model.FrontChance()?1:0;
            if(guidedCoinFace>=0)face=guidedCoinFace;
            ShowFace(face);Settle();if(!suppressLandingEffects)Game.PlayHit();
            if(suppressLandingEffects){luckyEcho=false;echoIn=0;}
            else {
                Game.ChainReaction(this);
                Game.GuideCoinLanded(this);
                if(!luckyEcho){if(UnityEngine.Random.value<Game.Model.EchoChance()){echoIn=.3f;luckyEcho=true;}}else luckyEcho=false;
                PlaySmoke(1.3f);
            }
            Landed?.Invoke(this);
            // CoinVat.ResetToIdle runs after its callbacks, including Haaland's attempted rethrow.
            flying=false;throwRole=0;rewardScale=1;critAdd=0;suppressLandingEffects=false;
        }

        // DiceVat.FinishThrow
        void LandDice() {
            flying=false;Rect.anchoredPosition=Game.ClampScene(target,40);
            ShowFace(face);Settle();if(!suppressLandingEffects){Game.PlayDiceLand();PlaySmoke(2f);}Game.DiceLanded(this);
            if(!luckyEcho){if(UnityEngine.Random.value<Game.Model.EchoChance()){echoIn=.3f;luckyEcho=true;}}else luckyEcho=false;
            Landed?.Invoke(this);
        }

        void Echo() {
            if(flying)return;
            if(World==1)Launch(Game.RandomLanding(Rect.anchoredPosition),false);
            else Launch(Game.ClampScene(target+new Vector2(UnityEngine.Random.Range(-30f,30f),UnityEngine.Random.Range(-30f,30f)),40),manual,throwRole);
        }

        void PlaySmoke(float scale){if(!smoke)smoke=SmokeEffect.Create(Game.SmokePool);smoke.Play(Rect.anchoredPosition,scale);}
        void OnDestroy(){if(smoke)Destroy(smoke.gameObject);}
    }

    /// <summary>coins_smoke.prefab: twelve sprite frames played once where the coin landed.</summary>
    public sealed class SmokeEffect:MonoBehaviour {
        const int Frames=12;const float Interval=.033f;
        static Sprite[] frames;Image image;float timer;int index;
        public static SmokeEffect Create(RectTransform pool) {
            var rt=GameController.NewRect("CoinsSmoke",pool);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);
            var effect=rt.gameObject.AddComponent<SmokeEffect>();effect.image=rt.gameObject.AddComponent<Image>();effect.image.raycastTarget=false;
            rt.gameObject.SetActive(false);return effect;
        }
        public void Play(Vector2 position,float scale) {
            Preload();
            var rt=(RectTransform)transform;rt.anchoredPosition=position;rt.localScale=Vector3.one*scale;
            timer=0;index=0;gameObject.SetActive(true);Apply();
        }
        // Each frame is padded on the GPU and read back. Twelve of them on the first landing stall a phone.
        public static void Preload() {
            if(frames==null){frames=new Sprite[Frames];for(int i=0;i<Frames;i++)frames[i]=Bufu.Rebuild.UI.UiAssets.Sprite("zgygy_coins_smoke_"+(i+1).ToString("00"));}
        }
        void Apply(){var sprite=frames[Math.Min(index,Frames-1)];if(!sprite){gameObject.SetActive(false);return;}image.sprite=sprite;image.SetNativeSize();}
        void Update() {
            timer+=Time.deltaTime;
            while(timer>=Interval){timer-=Interval;index++;if(index>=Frames){gameObject.SetActive(false);return;}Apply();}
        }
    }

    /// <summary>RoleVat of the basic helper (roleType 0): walks to an idle coin, throws it, rests, and sometimes falls asleep.</summary>
    public sealed class HelperActor:MonoBehaviour,IPointerClickHandler {
        enum Step{IdleWait,NoCoinWait,Walking,Throwing,AfterThrowWait,SkillCdWait,Sleeping}
        public GameController Game;public VatGraphic Visual;public int RoleId;string loadedAsset="";
        const float BaseMoveSpeed=200f,ReachThreshold=10f,CoinMinDistance=60f,WaitAfterThrow=.5f,SleepCheckInterval=5f;
        Step step=Step.IdleWait;float stepTimer,sleepTimer,sleepElapsed,sleepDuration;
        CoinActor target;Vector2 targetPosition;bool facingFront;string currentAnim="";bool currentLoop;
        GameObject tip;RectTransform tipRect;float tipPhase;
        public RectTransform Rect=>(RectTransform)transform;
        public bool Sleeping{get{return step==Step.Sleeping;}}

        public void Init(GameController game,int roleId) {
            Game=game;RoleId=roleId;Visual=VatGraphic.AddTo(gameObject);Visual.raycastTarget=false;Visual.PixelsPerUnit=72*CoinActor.S;
            LoadRole();
            facingFront=UnityEngine.Random.value>.5f;
            Rect.anchoredPosition=Game.RandomHelperSpawn();
            step=Step.IdleWait;stepTimer=UnityEngine.Random.Range(.1f,.6f);
            PlayIdle();Game.PlayVatEffect(Rect,"zgygy_fight_calling","animation",Visual.PixelsPerUnit,Vector2.zero,false);
        }
        void LoadRole() {
            string asset=HelperSkills.RoleVat(RoleId,Game.Model.HelperAwake(RoleId));
            if(asset==loadedAsset)return;loadedAsset=asset;Visual.Load(asset);
        }

        float MoveSpeed=>BaseMoveSpeed*(1f+Game.Model.Level(Game.Model.BaseId+9,true)*.05f)*Game.Model.SupportSpeedMultiplier;

        void Update() {
            if(!Game||Game.Model==null)return;
            LoadRole();
            float dt=Time.deltaTime;
            TickSleepTimer(dt);TickBehavior(dt);
            if(tip)TickTip(dt);
        }

        // ------------------------------------------------------------ sleep (RoleVat.TickSleepTimer / EnterSleeping / WakeUp)
        void TickSleepTimer(float dt) {
            if(step==Step.Sleeping)return;
            sleepTimer+=dt;if(sleepTimer<SleepCheckInterval)return;
            sleepTimer=0;if(UnityEngine.Random.value<Game.Model.SleepChance())EnterSleeping();
        }
        void EnterSleeping() {
            step=Step.Sleeping;sleepElapsed=0;target=null;Visual.raycastTarget=true;
            Play("sleep",true);ShowSleepTip();
            sleepDuration=Game.Model.SleepSeconds();
        }
        void WakeUp(bool effect) {
            step=Step.IdleWait;stepTimer=UnityEngine.Random.Range(.1f,.5f);sleepElapsed=0;sleepTimer=0;
            HideSleepTip();Visual.raycastTarget=false;
            if(effect){Game.PlayRevive();Game.PlayVatEffect(Game.TipPool,"zgygy_hammer_wakeup","hit",100,Rect.anchoredPosition+new Vector2(0,80),true);}
            PlayIdle();
        }
        public void OnPointerClick(PointerEventData eventData){if(eventData==null&&step==Step.Sleeping)WakeUp(true);}

        // "Role_huanxing": the bubble above the sleeping helper, shown for the first three sleepers of a session.
        void ShowSleepTip() {
            if(Game.SleepCount>=3||Game.SleepTipActive)return;
            var sprite=Bufu.Rebuild.UI.UiAssets.Sprite("xsyd_huanxing");if(!sprite)return;
            var rt=GameController.NewRect("SleepTip",Game.TipPool);rt.anchorMin=rt.anchorMax=new Vector2(.5f,.5f);
            var image=rt.gameObject.AddComponent<Image>();image.sprite=sprite;image.SetNativeSize();
            rt.gameObject.AddComponent<Button>().onClick.AddListener(()=>{if(step==Step.Sleeping)WakeUp(true);});
            tip=rt.gameObject;tipRect=rt;tipPhase=0;PlaceTip(0);
            Game.SleepTipActive=true;Game.SleepCount++;
        }
        void PlaceTip(float bob){tipRect.anchoredPosition=Rect.anchoredPosition+new Vector2(0,110+bob);}
        void TickTip(float dt){tipPhase+=dt;PlaceTip(Mathf.PingPong(tipPhase,.4f)/.4f*15f);}  // DOMoveY(+15, 0.4s) yoyo loop
        void HideSleepTip(){if(tip){Destroy(tip);tip=null;}Game.SleepTipActive=false;}
        void OnDestroy(){if(tip)Destroy(tip);}

        // ------------------------------------------------------------ behaviour (RoleVat.TickBehavior)
        void TickBehavior(float dt) {
            switch(step) {
                case Step.IdleWait:case Step.NoCoinWait:
                    PlayIdle();stepTimer-=dt;if(stepTimer<=0)TryFindThrowableAndStart();break;
                case Step.Walking:TickWalking(dt);break;
                case Step.Throwing:
                    stepTimer-=dt;if(stepTimer<=0){step=Step.AfterThrowWait;stepTimer=WaitAfterThrow;}break;
                case Step.AfterThrowWait:
                    PlayIdle();stepTimer-=dt;if(stepTimer<=0)EnterSkillCdWait();break;
                case Step.SkillCdWait:
                    PlayIdle();stepTimer-=dt;if(stepTimer<=0){step=Step.IdleWait;stepTimer=UnityEngine.Random.Range(.1f,.4f);}break;
                case Step.Sleeping:
                    sleepElapsed+=dt;if(sleepElapsed>=sleepDuration)WakeUp(false);break;
            }
        }

        void TryFindThrowableAndStart() {
            var found=RoleId==1&&Game.Model.HelperAwake(1)>0?Game.FindNearestThrowable(Rect.anchoredPosition):Game.FindThrowableForHelper(Rect.anchoredPosition);
            if(found==null){step=Step.NoCoinWait;stepTimer=.5f;return;}
            target=found;targetPosition=StandPosition(found);
            step=Step.Walking;PlayWalk(targetPosition.y<=Rect.anchoredPosition.y);
        }

        Vector2 StandPosition(CoinActor coin) {
            Vector2 coinPos=coin.Rect.anchoredPosition,direction=Rect.anchoredPosition-coinPos;
            if(direction.sqrMagnitude<.001f)direction=UnityEngine.Random.insideUnitCircle;
            return Game.ClampScene(coinPos+direction.normalized*CoinMinDistance,30);
        }

        static bool Available(CoinActor coin)=>coin&&!coin.Busy;

        void TickWalking(float dt) {
            if(!Available(target)){step=Step.IdleWait;stepTimer=.2f;target=null;return;}
            targetPosition=StandPosition(target);
            Vector2 position=Rect.anchoredPosition;
            var next=Vector2.MoveTowards(position,targetPosition,MoveSpeed*dt);Rect.anchoredPosition=next;
            Vector2 delta=targetPosition-position;
            if(Mathf.Abs(delta.x)>.01f){var s=Rect.localScale;s.x=Mathf.Abs(s.x)*(delta.x<0?-1:1);Rect.localScale=s;}
            PlayWalk(targetPosition.y<=position.y);
            if(Vector2.Distance(next,targetPosition)<=ReachThreshold)EnterThrowing(target);
        }

        void EnterThrowing(CoinActor coin) {
            if(!Available(coin)){step=Step.IdleWait;stepTimer=.2f;return;}
            step=Step.Throwing;facingFront=coin.Rect.anchoredPosition.y<=Rect.anchoredPosition.y;
            stepTimer=coin.RoleThrowDuration+.5f;
            Play(facingFront?"throw_front":"throw_back",false);
            if(!coin.Launch(Game.RandomLanding(coin.Rect.anchoredPosition),false,RoleId)){step=Step.IdleWait;stepTimer=.2f;target=null;return;}
            if(RoleId>1) {
                string skill=facingFront?"skill_front":"skill_back";
                if(Visual.Has(skill))Play(skill,false);
                HelperSkills.Cast(this,coin);
            }
            if(RoleId==9&&coin.World==1&&Game.Model.HelperAwake(9)>=1)HelperSkills.RepeatHaalandThrow(Game,coin);
            target=null;
        }

        void EnterSkillCdWait() {
            step=Step.SkillCdWait;
            float cd=Game.Model.RoleCooldown(RoleId);
            if(cd<=0){step=Step.IdleWait;stepTimer=UnityEngine.Random.Range(.1f,.4f);return;}
            stepTimer=cd;PlayIdle();
        }

        // ------------------------------------------------------------ animation
        void PlayIdle(){Play(facingFront?"idle_front":"idle_back",true);}
        void PlayWalk(bool front){facingFront=front;Play(front?"walk_front":"walk_back",true);}
        void Play(string animation,bool loop){
            if(currentAnim==animation&&currentLoop==loop)return;
            currentAnim=animation;currentLoop=loop;Visual.Play(animation,loop);
        }
    }
}
