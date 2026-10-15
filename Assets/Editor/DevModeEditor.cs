using UnityEditor;
using UnityEngine;
using Bufu.Rebuild;

public static class DevModeEditor {
    const string Menu="不服开一局/开发模式（独立存档）";
    [MenuItem(Menu)]
    static void Toggle() {
        bool enabled=!EditorPrefs.GetBool(DevMode.EditorPreference,false);
        EditorPrefs.SetBool(DevMode.EditorPreference,enabled);
        Debug.Log("开发模式"+(enabled?"已开启":"已关闭")+"，下次进入 Play 生效；使用独立开发存档。");
    }
    [MenuItem(Menu,true)]
    static bool Validate(){UnityEditor.Menu.SetChecked(Menu,EditorPrefs.GetBool(DevMode.EditorPreference,false));return true;}
}
