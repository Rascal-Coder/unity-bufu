using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Bufu.Rebuild.UI {
    /// <summary>Minimal JSON reader producing Dictionary&lt;string,object&gt;, List&lt;object&gt;, double, string, bool and null.</summary>
    public static class MiniJson {
        public static object Parse(string json){int i=0;var value=ReadValue(json,ref i);return value;}

        static void Skip(string s,ref int i){while(i<s.Length&&char.IsWhiteSpace(s[i]))i++;}

        static object ReadValue(string s,ref int i) {
            Skip(s,ref i);
            if(i>=s.Length)throw new FormatException("Unexpected end of JSON");
            char c=s[i];
            if(c=='{')return ReadObject(s,ref i);
            if(c=='[')return ReadArray(s,ref i);
            if(c=='"')return ReadString(s,ref i);
            if(c=='t'&&string.CompareOrdinal(s,i,"true",0,4)==0){i+=4;return true;}
            if(c=='f'&&string.CompareOrdinal(s,i,"false",0,5)==0){i+=5;return false;}
            if(c=='n'&&string.CompareOrdinal(s,i,"null",0,4)==0){i+=4;return null;}
            if(c=='N'&&string.CompareOrdinal(s,i,"NaN",0,3)==0){i+=3;return 0d;}
            if(c=='I'&&string.CompareOrdinal(s,i,"Infinity",0,8)==0){i+=8;return double.MaxValue;}
            if(c=='-'&&i+1<s.Length&&s[i+1]=='I'){i+=9;return double.MinValue;}
            return ReadNumber(s,ref i);
        }

        static Dictionary<string,object> ReadObject(string s,ref int i) {
            var result=new Dictionary<string,object>();i++;
            while(true) {
                Skip(s,ref i);
                if(s[i]=='}'){i++;return result;}
                string key=ReadString(s,ref i);Skip(s,ref i);i++;
                result[key]=ReadValue(s,ref i);Skip(s,ref i);
                if(s[i]==','){i++;continue;}
                if(s[i]=='}'){i++;return result;}
                throw new FormatException("Expected , or } at "+i);
            }
        }

        static List<object> ReadArray(string s,ref int i) {
            var result=new List<object>();i++;
            while(true) {
                Skip(s,ref i);
                if(s[i]==']'){i++;return result;}
                result.Add(ReadValue(s,ref i));Skip(s,ref i);
                if(s[i]==','){i++;continue;}
                if(s[i]==']'){i++;return result;}
                throw new FormatException("Expected , or ] at "+i);
            }
        }

        static string ReadString(string s,ref int i) {
            var sb=new StringBuilder();i++;
            while(i<s.Length) {
                char c=s[i++];
                if(c=='"')return sb.ToString();
                if(c!='\\'){sb.Append(c);continue;}
                char e=s[i++];
                switch(e) {
                    case 'n':sb.Append('\n');break;
                    case 't':sb.Append('\t');break;
                    case 'r':sb.Append('\r');break;
                    case 'b':sb.Append('\b');break;
                    case 'f':sb.Append('\f');break;
                    case 'u':sb.Append((char)Convert.ToInt32(s.Substring(i,4),16));i+=4;break;
                    default:sb.Append(e);break;
                }
            }
            throw new FormatException("Unterminated string");
        }

        static double ReadNumber(string s,ref int i) {
            int start=i;
            while(i<s.Length&&"+-0123456789.eE".IndexOf(s[i])>=0)i++;
            return double.Parse(s.Substring(start,i-start),NumberStyles.Float,CultureInfo.InvariantCulture);
        }
    }

    public static class J {
        public static Dictionary<string,object> Obj(this Dictionary<string,object> d,string key)=>d!=null&&d.TryGetValue(key,out var v)?v as Dictionary<string,object>:null;
        public static List<object> Arr(this Dictionary<string,object> d,string key)=>d!=null&&d.TryGetValue(key,out var v)?v as List<object>:null;
        public static string Str(this Dictionary<string,object> d,string key,string fallback="")=>d!=null&&d.TryGetValue(key,out var v)&&v is string s?s:fallback;
        public static float F(this Dictionary<string,object> d,string key,float fallback=0) {
            if(d==null||!d.TryGetValue(key,out var v)||v==null)return fallback;
            if(v is double x)return (float)x;
            if(v is bool b)return b?1:0;
            return fallback;
        }
        public static int I(this Dictionary<string,object> d,string key,int fallback=0)=>(int)Math.Round(d.F(key,fallback));
        public static bool B(this Dictionary<string,object> d,string key,bool fallback=false)=>d!=null&&d.ContainsKey(key)?d.F(key,fallback?1:0)!=0:fallback;
        public static UnityEngine.Color Color(this Dictionary<string,object> d,string key,UnityEngine.Color fallback) {
            var c=d.Obj(key);if(c==null)return fallback;
            return new UnityEngine.Color(c.F("r",1),c.F("g",1),c.F("b",1),c.F("a",1));
        }
        public static UnityEngine.Vector2 V2(this Dictionary<string,object> d,string key) {var v=d.Obj(key);return v==null?UnityEngine.Vector2.zero:new UnityEngine.Vector2(v.F("x"),v.F("y"));}
        public static UnityEngine.Vector4 V4(this Dictionary<string,object> d,string key) {var v=d.Obj(key);return v==null?UnityEngine.Vector4.zero:new UnityEngine.Vector4(v.F("x"),v.F("y"),v.F("z"),v.F("w"));}
    }
}
