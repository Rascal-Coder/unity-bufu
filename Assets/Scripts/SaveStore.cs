using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace Bufu.Rebuild {
    public static class SaveStore {
        public static string PathFor(bool developerMode,bool maxed)=>Path.Combine(Application.persistentDataPath,maxed?"bufu-local-max-v1.json":developerMode?"bufu-local-dev-v1.json":"bufu-local-v1.json");
        public static string PathForMode(bool developerMode)=>PathFor(developerMode,false);
        public static string PathName=>PathForMode(DevMode.Enabled);
        public static SaveData Load(bool developerMode=false) {
            string path=PathForMode(developerMode);
            foreach(var p in new[]{path,path+".bak"}) {
                if(!File.Exists(p))continue;
                try {var data=JsonUtility.FromJson<SaveData>(File.ReadAllText(p,Encoding.UTF8));new GameModel(data);return data;}
                catch(Exception e){Debug.LogWarning("存档读取失败，尝试备份: "+e.Message);}
            }
            return new SaveData();
        }
        public static void Save(GameModel game) {
            if(GameController.VerificationMode)return;
            string p=PathFor(game.DeveloperMode,game.Maxed);Directory.CreateDirectory(Path.GetDirectoryName(p));
            foreach(var state in game.Data.worlds)state.lastExit=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            File.WriteAllText(p+".tmp",JsonUtility.ToJson(game.Data,true),new UTF8Encoding(false));
            if(File.Exists(p))File.Replace(p+".tmp",p,p+".bak");else File.Move(p+".tmp",p);
        }
    }
}
