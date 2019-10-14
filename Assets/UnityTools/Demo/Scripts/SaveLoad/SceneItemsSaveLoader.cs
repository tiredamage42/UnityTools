using System.Collections.Generic;
using UnityEngine;


using UnityTools;
using UnityTools.GameSettingsSystem;
namespace UnityToolsDemo {
    public class SceneItemsSaveLoader : SaveLoadObjectSceneHandler<SceneItem, SceneItemState, SceneItemsSaveLoader> {
        
        protected override void SaveObjects (SceneItem[] activeObjects, List<SceneItemState> savedObjects, bool manualSave) {
            
            for (int i = 0; i < activeObjects.Length; i++) {
                // if equipped to player (persistent), continue

                // if equipped to non persisten actor (ai), unequip and disable (if disabling scene)
                // but dont save to saved scene item list

                savedObjects.Add(new SceneItemState(activeObjects[i]));

                // give to pool again (if disabling scene)
                if (!manualSave) {
                    activeObjects[i].gameObject.SetActive(false);
                }
            }
        }

        protected override void LoadObjects (SceneItem[] activeObjects, List<SceneItemState> savedObjects, bool isLoadingSaveSlot) {
            
            for (int i = 0; i < activeObjects.Length; i++) {
                // if not equipped to actor, give to pool again
                activeObjects[i].gameObject.SetActive(false);                    
            }

            // build the scene items as they were saved before
            for (int i = 0; i < savedObjects.Count; i++) {
                SceneItem loadedItem = SceneItem.pool.GetAvailable(PrefabReferences.GetPrefabReference<SceneItem>("DemoPrefabs", savedObjects[i].prefabName), null);
                OnObjectLoad (loadedItem, loadedItem, savedObjects[i]);
            }
        }    
    }
}
