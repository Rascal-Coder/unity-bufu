using System;
using System.IO;
using System.Text;
using UnityEngine;
namespace Bufu.Rebuild {
    public static class SaveStore {
        public static string PathName=>Path.Combine(Application.persistentDataPath,"bufu-local-v1.json");
        public static SaveData Load() {
            foreach(var p in new[]{PathName,PathName+".bak"}) {
                if(!File.Exists(p))continue;
                try {var data=JsonUtility.FromJson<SaveData>(File.ReadAllText(p,Encoding.UTF8));new GameModel(data);return data;}
                catch(Exception e){Debug.LogWarning("存档读取失败，尝试备份: "+e.Message);}
            }
            return new SaveData();
        }
        public static void Save(GameModel game) {
            if(GameController.VerificationMode)return;
            string p=PathName;Directory.CreateDirectory(Path.GetDirectoryName(p));
            foreach(var state in game.Data.worlds)state.lastExit=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            File.WriteAllText(p+".tmp",JsonUtility.ToJson(game.Data,true),new UTF8Encoding(false));
            if(File.Exists(p))File.Replace(p+".tmp",p,p+".bak");else File.Move(p+".tmp",p);
        }
    }
}
