// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityTools.EditorTools;
namespace UnityTools {
    [InitializeOnLoad] public class PerksCollectionEditor {
        static PerksCollectionEditor () {
            UnityToolsEditor.AddProjectChangeListener(RefreshPerksList);
        }

        public static void RefreshPerksList () {
            if (Application.isPlaying)
                return;

            PerksCollection instance = PerksCollection.instance;
            // dont update when in play mode or if our game settings object is missing
            if (instance == null) return;

            // update the array of all game settings objects in the project
            Perk[] allScenesInProject = AssetTools.FindAssetsByType<Perk>(log: false, null).ToArray();

            instance.allPerks = allScenesInProject;

            for (int i = 0; i < allScenesInProject.Length; i++) {
                for (int j = i + 1; j < allScenesInProject.Length; i++) {
                    if (allScenesInProject[i].name == allScenesInProject[j].name) {
                        Debug.LogError("Perks with duplicate names: " + allScenesInProject[i].name);
                    }
                }
            }
            for (int i = 0; i < allScenesInProject.Length; i++) {
                for (int j = i + 1; j < allScenesInProject.Length; i++) {
                    if (allScenesInProject[i].displayName == allScenesInProject[j].displayName) {
                        Debug.LogError("Perks with duplicate display names: " + allScenesInProject[i].displayName);
                    }
                }
            }
            EditorUtility.SetDirty(instance);
        }  
    }
}