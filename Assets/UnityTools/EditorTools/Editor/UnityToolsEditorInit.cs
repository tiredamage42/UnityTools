
using UnityEditor;
namespace UnityTools.EditorTools {
    [InitializeOnLoad] public class UnityToolsEditorInit {
        static UnityToolsEditorInit () {
            UnityToolsEditor.OnProjectChange();
            EditorApplication.projectChanged += UnityToolsEditor.OnProjectChange;
        }
    }
}