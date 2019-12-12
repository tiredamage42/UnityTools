using UnityEngine;
using System;
using UnityEditor;
namespace UnityTools.EditorTools {
    public class EditPrefab : IDisposable {
        public readonly string assetPath;
        public readonly GameObject root;
        public readonly GameObject prefab;
    
        public EditPrefab(GameObject prefab) {
            this.prefab = prefab;
            assetPath = AssetDatabase.GetAssetPath(prefab);
            root = PrefabUtility.LoadPrefabContents(assetPath);
        }
    
        public void Dispose() {
            PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            PrefabUtility.UnloadPrefabContents(root);
            EditorUtility.SetDirty(prefab);
        }
    }
}