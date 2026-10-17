using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bufu.Rebuild {
    public enum SupportKind { Money,Income,Speed }
    public sealed partial class GameModel {
        sealed class SupportBuff {public SupportKind Kind;public float End;public int Stacks=1;}
        readonly List<SupportBuff> supportBuffs=new List<SupportBuff>();
        public static long SupportNow=>(long)(DateTime.Now-new DateTime(1970,1,1)).TotalMilliseconds;
        public bool SupportUnlocked(SupportKind kind)=>OpenAccess||MainLevel>Factor(kind==SupportKind.Money?58:19);
        public float SupportMinutes(SupportKind kind) {
            if(kind==SupportKind.Money)return Mathf.Max(1,(float)Factor(24));
            float minutes=(float)Factor(kind==SupportKind.Income?12:9);
            if(kind==SupportKind.Speed&&Data.world==2)minutes+=Mathf.Round((float)TalentValue(304)/60f*10f)/10f;
            return minutes;
        }
        public float SupportRemaining(SupportKind kind,long now=0) {
            if(now==0)now=SupportNow;long stamp=State.supportLastTime[2-(int)kind];
            return stamp<=0?0:Mathf.Max(0,SupportMinutes(kind)*60-(now-stamp)/1000f);
        }
        public double SupportMoney(double now) {
            UpdateRate(now);double multiplier=Data.world==1?1+TalentValue(305):(State.talents[307]>0?TalentValue(307):1);
            return Math.Max(State.averagePerMinute*Factor(10),500)*multiplier;
        }
        void AddSupportBuff(SupportKind kind,float duration) {
            float end=Time.time+duration;supportBuffs.RemoveAll(x=>x.End<=Time.time);
            var buff=supportBuffs.Find(x=>x.Kind==kind);
            if(buff==null)supportBuffs.Add(new SupportBuff{Kind=kind,End=end});
            else {buff.Stacks++;buff.End=Math.Max(buff.End,end);}
        }
        public void ResumeSupportBuffs() {
            foreach(var kind in new[]{SupportKind.Income,SupportKind.Speed}) {
                float left=SupportRemaining(kind);if(left>0)AddSupportBuff(kind,left);
            }
        }
        float SupportExtra(SupportKind kind) {
            supportBuffs.RemoveAll(x=>x.End<=Time.time);
            var buff=supportBuffs.Find(x=>x.Kind==kind);
            return buff==null?0:(float)Factor(kind==SupportKind.Income?11:8)*buff.Stacks;
        }
        public float SupportSpeedMultiplier=>1+SupportExtra(SupportKind.Speed);
        float SupportIncomeMultiplier=>1+SupportExtra(SupportKind.Income);
        public bool ClaimSupport(SupportKind kind,double preparedMoney=0) {
            if(!SupportUnlocked(kind)||SupportRemaining(kind)>0)return false;
            if(kind==SupportKind.Money) {
                if(!Finite(preparedMoney)||preparedMoney<=0)return false;
                GrantAmount(1,preparedMoney);UpdateRate(Time.unscaledTime);CheckGrowth();
            } else AddSupportBuff(kind,(float)Factor(kind==SupportKind.Income?12:9)*60);
            State.supportLastTime[2-(int)kind]=SupportNow;Changed?.Invoke();return true;
        }
    }
}
