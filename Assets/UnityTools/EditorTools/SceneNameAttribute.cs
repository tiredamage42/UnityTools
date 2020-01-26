
using UnityEditor;
namespace UnityTools.EditorTools {
    public class SceneNameAttribute : AssetSelectionAttribute
    {
        #if UNITY_EDITOR
        public SceneNameAttribute ( ) : base(typeof(SceneAsset), true) { }
        #else
        public SceneNameAttribute ( ) : base(typeof(float), true) { }
        #endif
    }
}
