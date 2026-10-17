using UnityEditor;
using UnityEditor.SceneManagement;

public static class MaxedSceneEditor {
    const string Path="Assets/Scenes/Maxed.unity";
    [MenuItem("不服开一局/打开满配场景")]
    static void Open() {
        if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(Path);
    }
}
