using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class IntroPlaybackCheck {
    public static void Run() {
        Directory.CreateDirectory("Documentation/IntroCheck");
        File.WriteAllText("Documentation/IntroCheck/result.txt","started\n");
        EditorSceneManager.playModeStartScene=null;
        EditorSceneManager.OpenScene("Assets/Scenes/Intro.unity");
        var view=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        view.Show();
        view.Focus();
        EditorApplication.isPlaying=true;
    }
}
