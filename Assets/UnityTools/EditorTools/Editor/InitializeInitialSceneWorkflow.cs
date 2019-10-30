using UnityEditor;
namespace UnityTools.EditorTools {
    [InitializeOnLoad] static class InitializeInitialSceneWorkflow {
        static InitializeInitialSceneWorkflow() {
            EditorApplication.playModeStateChanged += InitialSceneWorkflow.OnPlayModeChanged;
        }
    }
}