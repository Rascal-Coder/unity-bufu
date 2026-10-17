using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>Playing the main scene starts at the prerender, then returns to the game.</summary>
[InitializeOnLoad]
static class IntroScenePlayMode {
    const string IntroPath="Assets/Scenes/Intro.unity";
    const string MainPath="Assets/Scenes/Main.unity";

    static IntroScenePlayMode() {
        EditorApplication.delayCall+=Apply;
        EditorSceneManager.activeSceneChangedInEditMode+=(_,__)=>Apply();
    }

    static void Apply() {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        var path=SceneManager.GetActiveScene().path.Replace('\\','/');
        EditorSceneManager.playModeStartScene=path==MainPath?AssetDatabase.LoadAssetAtPath<SceneAsset>(IntroPath):null;
    }
}
