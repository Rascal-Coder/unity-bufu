using UnityEditor;
using UnityEditor.SceneManagement;

public static class LoadingFlowShotEditor {
    public static void Run() {
        EditorSceneManager.OpenScene("Assets/Scenes/Intro.unity");
        EditorApplication.isPlaying=true;
    }
}
