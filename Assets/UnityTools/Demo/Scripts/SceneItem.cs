using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityTools.GameSettingsSystem;
using UnityEngine.SceneManagement;
using UnityTools;
namespace UnityToolsDemo {

    [System.Serializable] public class SceneItemState : SaveObjectState {
        // public string prefabName;
        
        public SceneItemState (SceneItem instance) : base (instance)
        {
            // this.prefabName = instance.basePrefabName;
        }
    }

    public class SceneItem : Poolable<SceneItem>, ISaveObject<SceneItemState> {
        public void LoadFromSavedObject (SceneItemState savedState) {
            
        }

        public override bool IsAvailable () {
            return true; //!isequipped to actor....
        }

        public override string PrefabObjectName() {
            return "DemoPrefabs";
        }

        // public void WarpTo (Vector3 position, Quaternion rotation) {
        //     transform.position = position;
        //     transform.rotation = rotation;
        // }

        // public static PrefabPool<SceneItem> pool = new PrefabPool<SceneItem>();
        // public string basePrefabName;


        // void OnEnable () 
        // {
        //     pool.AddManualInstance(PrefabReferences.GetPrefabReference<SceneItem>("DemoPrefabs", basePrefabName), this);
        // }   
    }
}
