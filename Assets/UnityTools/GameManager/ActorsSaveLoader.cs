using System.Collections.Generic;
using UnityEngine;

using UnityTools;
using UnityTools.GameSettingsSystem;
namespace UnityToolsDemo {
    public class ActorsSaveLoader : SaveLoadObjectSceneHandler<Actor, ActorState, ActorsSaveLoader> {

        protected override void SaveObjects (Actor[] activeObjects, List<ActorState> savedObjects, bool manualSave) {
            for (int i = 0; i < activeObjects.Length; i++) {
                
                // only save player actor if doing a manual save
                if (activeObjects[i].isPlayer && !manualSave)
                    continue;

                savedObjects.Add(new ActorState(activeObjects[i]));
                
                // give to pool again (if disabling scene and not player actor)
                if (!manualSave && !activeObjects[i].isPlayer) {
                    activeObjects[i].gameObject.SetActive(false);
                }
            }
        }
        protected override void LoadObjects (Actor[] activeObjects, List<ActorState> savedObjects, bool isLoadingSaveSlot) {
            
            for (int i = 0; i < activeObjects.Length; i++) {
                // if not player, give to pool
                if (!activeObjects[i].isPlayer) {
                    activeObjects[i].gameObject.SetActive(false);
                }
            }

            // build the scene items as they were saved before
            for (int i = 0; i < savedObjects.Count; i++) {
                
                Actor loadedActor = null;
                if (savedObjects[i].isPlayer) {
                    
                    // only load player actor values if we're manually loading manual load
                    if (!isLoadingSaveSlot) 
                        continue;

                    loadedActor = Actor.playerActor;
                }
                else {
                    loadedActor = Actor.pool.GetAvailable(PrefabReferences.GetPrefabReference<Actor>(Actor.ActorPrefabsObjectName, savedObjects[i].prefabName), null);              
                }

                OnObjectLoad (loadedActor, loadedActor, savedObjects[i]);
            }
        }
    }
}