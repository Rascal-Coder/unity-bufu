using UnityEditor;
using UnityEditor.SceneManagement;

public static class EffectCheck {
    public static void Run() {
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        EditorApplication.isPlaying = true;
    }
}
