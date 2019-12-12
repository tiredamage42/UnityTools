
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace UnityTools.EditorTools {
    [CustomEditor(typeof(PrefabReferenceCollection))]
    public class PrefabReferenceCollectionEditor : UnityEditor.Editor {

        public override void OnInspectorGUI () {
            
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();

            bool updateDynamicObjectReferences = EditorGUI.EndChangeCheck();

            if (GUILayout.Button("Update Dynamic Object Prefab Info"))
                updateDynamicObjectReferences = true;

            if (updateDynamicObjectReferences) 
                UpdateDynamicObjectReferencesInPrefabCollection(target as PrefabReferenceCollection);
            
        }

        public static void UpdateDynamicObjectReferencesInPrefabCollection (PrefabReferenceCollection refObj) {
            for (int i = 0; i < refObj.prefabs.Length; i++) {
                GameObject prefab = refObj.prefabs[i];
                if (prefab != null) {
                    if (prefab.GetComponent<DynamicObject>() != null) {
                        // Modify prefab contents and save it back to the Prefab Asset
                        using (var scope = new EditPrefab(prefab)) {
                            scope.root.GetComponent<DynamicObject>().prefabRef = new PrefabReference(refObj.name, prefab.name);
                        }
                    }
                }
            }
        }
    }


    [InitializeOnLoad] public class PrefabReferencesEditorInit {
        
        static PrefabReferencesEditorInit () {
            UnityToolsEditor.AddProjectChangeListener(UpdateDynamicObjectReferencesInPrefabObjects);
            EditorSceneManager.sceneSaved += UpdateDynamicObjectReferencesInPrefabObjects;
        }
        static void UpdateDynamicObjectReferencesInPrefabObjects(Scene scene) {
            UpdateDynamicObjectReferencesInPrefabObjects();
        }
        static void UpdateDynamicObjectReferencesInPrefabObjects () {
            if (Application.isPlaying)
                return;

            List<PrefabReferenceCollection> refs = AssetTools.FindAssetsByType<PrefabReferenceCollection>(logToConsole: false);
            for (int i = 0; i < refs.Count; i++) 
                PrefabReferenceCollectionEditor.UpdateDynamicObjectReferencesInPrefabCollection(refs[i]);            
        }
    }
}