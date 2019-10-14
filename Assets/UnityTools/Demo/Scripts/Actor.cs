using System.Collections.Generic;
using UnityEngine;

using UnityTools;


using UnityTools.GameSettingsSystem;
namespace UnityToolsDemo {
    [System.Serializable] public class ActorState : SceneObjectState {
        public string prefabName;
        public bool isPlayer;
        public float health;

        public ActorState (Actor instance) : base (instance)
        {
            this.prefabName = instance.basePrefabName;
            this.health = instance.health;
            this.isPlayer = instance.isPlayer;
        }
    }

    // used for npc's or player
    [System.Serializable] public class Actor : MonoBehaviour, ISaveableObject<ActorState>
    {
        public void LoadFromSavedObject (ActorState savedActor) {
            health = savedActor.health;
        }
        
        public void WarpTo (Vector3 position, Quaternion rotation) {
            transform.position = position;
            transform.rotation = rotation;
        }

        public static PrefabPool<Actor> pool = new PrefabPool<Actor>();
        
        public string basePrefabName;


        public static Actor playerActor;

        public bool isPlayer;
        public float health;

        void OnEnable () {
            if (isPlayer) {
                if (playerActor != null) {
                    Destroy(gameObject);
                    return;
                }
                playerActor = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                pool.AddManualInstance(PrefabReferences.GetPrefabReference<Actor>("DemoPrefabs", basePrefabName), this);
            }
        }
    }
}
