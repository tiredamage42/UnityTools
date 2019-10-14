using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityTools.GameSettingsSystem;
using UnityEngine.SceneManagement;
using UnityTools;
namespace UnityToolsDemo {

    [System.Serializable] public class SceneItemState : SceneObjectState {
        public string prefabName;
        
        public SceneItemState (SceneItem instance) : base (instance)
        {
            this.prefabName = instance.basePrefabName;
        }
    }

    public class SceneItem : MonoBehaviour, ISaveableObject<SceneItemState> {
        public void LoadFromSavedObject (SceneItemState savedState) {
            
        }
        public void WarpTo (Vector3 position, Quaternion rotation) {
            transform.position = position;
            transform.rotation = rotation;
        }

        public static PrefabPool<SceneItem> pool = new PrefabPool<SceneItem>();

        public string basePrefabName;

        void OnEnable () 
        {
            pool.AddManualInstance(PrefabReferences.GetPrefabReference<SceneItem>("DemoPrefabs", basePrefabName), this);
        }   
    }
}
