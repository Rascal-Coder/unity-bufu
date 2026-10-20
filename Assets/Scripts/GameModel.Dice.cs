using System;
using System.Linq;
using UnityEngine;

namespace Bufu.Rebuild {
    /// <summary>DiceVat, UpgradeManager.GetItemUpgrade3Effect, DownItem and AdLaoHuJi parity.</summary>
    public sealed partial class GameModel {
        float diceSuperRemaining;
        bool diceMerchantTimerInitialized;
        long diceMerchantRuntimeStart;
        float diceMerchantRuntimeTarget,diceMerchantElapsed;
        public bool DiceSuperUnlocked=>Data.world==2&&State.skills[106]>0;
        public bool DiceSuperActive=>Data.world==2&&diceSuperRemaining>0;
        public float DiceSuperRemaining=>DiceSuperActive?diceSuperRemaining:0;
        public bool DiceMerchantUnlocked=>Data.world==2&&State.skills[107]>0;
        public float DiceSkillEffect(int id) {
            var config=skills.FirstOrDefault(x=>x.id==id);int level=State.skills[id];
            if(config==null)return id==103||id==104||id>=106?0:1;
            float step=(float)config.upgradeResult-1;
            switch(id) {
                case 101:return Mathf.Max(0,1-step*level);
                case 102:return 1+step*level;
                case 103:case 104:return Mathf.Clamp01(step*level);
                case 105:return Mathf.Pow((float)config.upgradeResult,level);
                default:return level>0?1:0;
            }
        }
        public float[] DiceFaceWeights() {
            float low=DiceSkillEffect(101);
            return new[]{low,low,low,1f,1f,DiceSkillEffect(102)};
        }
        /// <summary>Explicit roll permits repeatable boundary tests; forced guide faces override Super 666.</summary>
        public int RollDiceFace(float roll,int forcedFace=0) {
            if(forcedFace>=1&&forcedFace<=6)return forcedFace;
            if(DiceSuperActive)return 6;
            var weights=DiceFaceWeights();float total=weights.Sum();
            if(total<=0)return Mathf.Clamp(1+(int)(Mathf.Clamp01(roll)*6),1,6);
            float pick=Mathf.Clamp(roll,0,.99999994f)*total,acc=0;
            for(int i=0;i<weights.Length;i++){acc+=weights[i];if(pick<acc)return i+1;}
            return 6;
        }
        /// <summary>Applied after the base value (which already contains the upgrade/evolution multiplier).</summary>
        public double DiceValueMultiplier(int quality,int face) {
            double value=face==6?DiceSkillEffect(105):1;
            if(DiceSuperActive)value+=TalentValue(308);
            if(quality==4)value+=TalentValue(106);
            return value;
        }
        public float DiceSuperTarget {
            get {if(State.diceSixTarget<=0)State.diceSixTarget=MainLevel*(float)Factor(73)+(float)Factor(74);return State.diceSixTarget;}
        }
        public bool DiceSuperReady=>DiceSuperUnlocked&&!DiceSuperActive&&State.diceSixCount>=DiceSuperTarget;
        public void RecordDiceLanded(int face) {
            if(face!=6||!DiceSuperUnlocked||DiceSuperActive)return;
            State.diceSixCount=Mathf.Min(DiceSuperTarget,State.diceSixCount+1+(float)TalentValue(313));Changed?.Invoke();
        }
        public float DiceSuperDuration(bool advertisement) {
            float duration=(float)Factor(75)+(float)TalentValue(312);
            return advertisement?duration*((float)Factor(76)+(float)TalentValue(309)):duration;
        }
        public bool ActivateDiceSuper(bool advertisement) {
            if(!DiceSuperReady)return false;
            diceSuperRemaining=DiceSuperDuration(advertisement);State.diceSixCount=0;
            State.diceSixTarget=MainLevel*(float)Factor(73)+(float)Factor(74);Changed?.Invoke();return true;
        }
        public void TickDiceTimers(float deltaTime) {
            float elapsed=Mathf.Max(0,deltaTime);
            // DownItem.Update continues the active Super 666 timer while the other world is shown.
            if(diceSuperRemaining>0) {
                diceSuperRemaining=Mathf.Max(0,diceSuperRemaining-elapsed);
                if(diceSuperRemaining==0)Changed?.Invoke();
            }
            if(DiceMerchantUnlocked&&!DiceMerchantAtLimit) {
                EnsureDiceMerchantTimer(DiceLocalMilliseconds);
                diceMerchantElapsed=Mathf.Min(diceMerchantRuntimeTarget,diceMerchantElapsed+elapsed);
            }
        }
        public void ResetDiceRuntime(){diceSuperRemaining=0;}
        public static long DiceLocalMilliseconds=>(long)(DateTime.Now-new DateTime(1970,1,1)).TotalMilliseconds;
        public void RefreshDiceMerchantDay(int date) {
            if(State.diceMerchantDate==date)return;
            State.diceMerchantDate=date;State.diceMerchantToday=0;
            State.diceMerchantWaitStart=0;State.diceMerchantWaitTarget=0;
            diceMerchantTimerInitialized=false;Changed?.Invoke();
        }
        public int DiceMerchantDailyLimit=>Math.Max(0,(int)Factor(71));
        public bool DiceMerchantAtLimit=>State.diceMerchantToday>=DiceMerchantDailyLimit;
        public float DiceMerchantRemaining(long localMilliseconds) {
            EnsureDiceMerchantTimer(localMilliseconds);
            return Mathf.Max(0,diceMerchantRuntimeTarget-diceMerchantElapsed);
        }
        void EnsureDiceMerchantTimer(long localMilliseconds) {
            // ResumeShangRenWaitCycle uses the saved wall clock once. The live wait then advances
            // only through UpdateShangRenTimer's scaled delta time, never through later clock reads.
            if(diceMerchantTimerInitialized&&diceMerchantRuntimeStart==State.diceMerchantWaitStart&&diceMerchantRuntimeTarget==State.diceMerchantWaitTarget)return;
            diceMerchantTimerInitialized=true;diceMerchantRuntimeStart=State.diceMerchantWaitStart;
            diceMerchantRuntimeTarget=Mathf.Max(0,State.diceMerchantWaitTarget);
            diceMerchantElapsed=diceMerchantRuntimeStart<=0?diceMerchantRuntimeTarget:
                Mathf.Clamp((localMilliseconds-diceMerchantRuntimeStart)/1000f,0,diceMerchantRuntimeTarget);
        }
        public bool DiceMerchantReady(long localMilliseconds)=>DiceMerchantUnlocked&&!DiceMerchantAtLimit&&DiceMerchantRemaining(localMilliseconds)<=0;
        public int DiceMerchantMinimum=>1+(int)TalentValue(306);
        public int DiceMerchantMaximum=>6+(int)TalentValue(306);
        public int ClaimDiceMerchant(int baseDiamonds,float nextWaitRoll,long localMilliseconds,int date) {
            RefreshDiceMerchantDay(date);
            if(!DiceMerchantReady(localMilliseconds))return 0;
            int basis=Mathf.Clamp(baseDiamonds,1,6),award=basis+(int)TalentValue(306);
            Data.diamonds+=award;State.diceMerchantToday+=basis;
            if(DiceMerchantAtLimit){State.diceMerchantWaitStart=0;State.diceMerchantWaitTarget=0;}
            else {
                State.diceMerchantWaitStart=localMilliseconds;
                State.diceMerchantWaitTarget=Mathf.Lerp((float)Factor(69),(float)Factor(70),Mathf.Clamp01(nextWaitRoll));
            }
            diceMerchantTimerInitialized=true;diceMerchantRuntimeStart=State.diceMerchantWaitStart;
            diceMerchantRuntimeTarget=State.diceMerchantWaitTarget;diceMerchantElapsed=DiceMerchantAtLimit?diceMerchantRuntimeTarget:0;
            Changed?.Invoke();return award;
        }
    }
}
