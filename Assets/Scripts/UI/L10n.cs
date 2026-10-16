using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Bufu.Rebuild.UI {
    /// <summary>Presentation-only Chinese/English catalog. Gameplay configuration and saves retain their original values.</summary>
    public static class L10n {
        sealed class Pattern {public Regex regex;public string english;public string[] args;}
        static readonly Dictionary<string,string> english=new Dictionary<string,string>(),keys=new Dictionary<string,string>(),cache=new Dictionary<string,string>();
        static readonly List<Pattern> patterns=new List<Pattern>();
        static readonly Regex variables=new Regex(@"\{([^{}]+)\}|[sS]{3}");
        static bool loaded;static string pref="bufu.language";
        public static string Language {get;private set;}="zh-CN";
        public static bool IsEnglish=>Language=="en";
        public static CultureInfo Culture=>CultureInfo.GetCultureInfo(IsEnglish?"en-US":"zh-CN");
        public static event Action Changed;

        public static void Initialize(bool developer,bool verification) {
            pref=(developer?"dev.":"")+"bufu.language";
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"--language");
            Language=NormalizeLanguage(at>=0&&at+1<args.Length?args[at+1]:verification?"zh-CN":PlayerPrefs.GetString(pref,"zh-CN"));
            Load();cache.Clear();
        }
        static string NormalizeLanguage(string value)=>string.Equals(value,"en",StringComparison.OrdinalIgnoreCase)||string.Equals(value,"en-US",StringComparison.OrdinalIgnoreCase)?"en":"zh-CN";
        public static void SetLanguage(string value,bool persist=true) {
            value=NormalizeLanguage(value);if(value==Language)return;
            Language=value;cache.Clear();
            if(persist&&!GameController.VerificationMode){PlayerPrefs.SetString(pref,Language);PlayerPrefs.Save();}
            Changed?.Invoke();
        }
        static string Normalize(string value)=>(value??"").Replace("\\n","\n").Replace("\r","").Trim();
        public static bool HasChinese(string value)=>!string.IsNullOrEmpty(value)&&value.Any(c=>c>='\u4e00'&&c<='\u9fff');
        public static string Source(string key) {Load();return keys.TryGetValue(key,out var value)?value:key;}
        public static string Text(string key)=>Translate(Source(key));
        public static string FormatSource(string key,params object[] args)=>string.Format(CultureInfo.InvariantCulture,Source(key),args);

        static void Load() {
            if(loaded)return;loaded=true;
            var file=Resources.Load<TextAsset>("Localization/catalog");if(!file)return;
            var root=(Dictionary<string,object>)MiniJson.Parse(file.text);
            var entries=root.Arr("entries").Cast<Dictionary<string,object>>().ToArray();
            foreach(var entry in entries) {
                string source=Normalize(entry.Str("zh")),translation=entry.Str("en");
                keys[entry.Str("key")]=entry.Str("zh");english[source]=translation;
            }
            foreach(var entry in entries.GroupBy(x=>Normalize(x.Str("zh"))).Select(g=>g.Last()).OrderByDescending(x=>variables.Replace(Normalize(x.Str("zh")),"").Length)) {
                string source=Normalize(entry.Str("zh"));var matches=variables.Matches(source);if(matches.Count==0)continue;
                var names=new List<string>();string regex="^";int end=0;
                foreach(Match match in matches) {
                    regex+=Regex.Escape(source.Substring(end,match.Index-end))+"(.+?)";
                    names.Add(match.Value);end=match.Index+match.Length;
                }
                regex+=Regex.Escape(source.Substring(end))+"$";
                patterns.Add(new Pattern{regex=new Regex(regex,RegexOptions.Singleline|RegexOptions.CultureInvariant,TimeSpan.FromMilliseconds(50)),english=entry.Str("en"),args=names.ToArray()});
            }
        }

        public static string Translate(string source)=>Translate(source,0);
        static string Translate(string source,int depth) {
            if(!IsEnglish||string.IsNullOrEmpty(source)||!HasChinese(source))return source;
            Load();if(cache.TryGetValue(source,out var cached))return cached;
            string normalized=Normalize(source),result=source;
            if(english.TryGetValue(normalized,out var exact))result=exact;
            else if(depth<5)foreach(var pattern in patterns) {
                var match=pattern.regex.Match(normalized);if(!match.Success)continue;
                result=pattern.english;
                for(int i=0;i<pattern.args.Length;i++)result=result.Replace(pattern.args[i],Translate(match.Groups[i+1].Value,depth+1));
                break;
            }
            if(cache.Count>4096)cache.Clear();cache[source]=result;return result;
        }
    }
}
