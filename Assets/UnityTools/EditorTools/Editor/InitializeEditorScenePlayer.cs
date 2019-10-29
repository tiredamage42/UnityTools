using UnityEditor;

namespace UnityTools.EditorTools {

    [InitializeOnLoad] static class InitializeEditorScenePlayer {
        static InitializeEditorScenePlayer()
        {
            EditorApplication.playModeStateChanged += EditorScenePlayer.OnPlayModeChanged;
        }
    }
}
