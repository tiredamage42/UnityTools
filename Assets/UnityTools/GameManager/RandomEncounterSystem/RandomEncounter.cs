
using UnityEngine;
using System;
using UnityEditor;
using UnityTools.RandomEncounters.Internal;
namespace UnityTools.RandomEncounters {

    [Serializable] public class RandomEncounterState : ObjectAttachmentState {
        public string spotName;
        public RandomEncounterState (RandomEncounter instance) { 
            this.spotName = instance.spotName;
        }       
    }

    // [Serializable] public class RandomEncounterArray : NeatArrayWrapper<RandomEncounter> { }

    public class RandomEncounter : DynamicObjectScript<RandomEncounter>, IObjectAttachment
    {

        void OnDrawGizmos () {
            Gizmos.color = new Color (1, 0, 0, .5f);
            Gizmos.DrawCube(transform.position + Vector3.up, new Vector3(.2f, 2, .2f));
        }

        public int InitializationPhase () {
            return 0;
        }

        public void InitializeDefault () {

        }

        public void Strip () {
            spotName = null;
        }

        public ObjectAttachmentState GetState () {
            return new RandomEncounterState (this);
        }

        public void LoadState (ObjectAttachmentState state) {
            RandomEncounterState encounterState = state as RandomEncounterState; 
            spotName = encounterState.spotName;
        }

        [NonSerialized] public string spotName;        

        string keyPrefix { get { return spotName + "."; } }
        public TrackedObjectState GetSpawnedObject (string key, out DynamicObject dynamicObject, out DynamicObjectState objectState) {
            TrackedObjectState objState = DynamicObjectManager.GetTrackedObject(keyPrefix + key, out dynamicObject, out objectState);
            switch (objState) {
                case TrackedObjectState.NotFound: 
                    Debug.LogError("Cant Find Object In Random Encounter: '" + name + "', Spot: '" + spotName + "', Key: '" + key + "'");
                    break;
                case TrackedObjectState.Loaded: break;
                case TrackedObjectState.Unloaded: break;
            }
            return objState;
        }

        public void SpawnObjects (string scene, RandomEncounterSpot spot) {
            this.spotName = scene + "." + spot.name;

            RandomEncounterSpawn[] spawns = GetComponentsInChildren<RandomEncounterSpawn>();

            for (int i = 0; i < spawns.Length; i++) {
                RandomEncounterSpawn s = spawns[i];
                Transform t = s.transform;
                DynamicObjectManager.AddTrackedObject(
                    keyPrefix + s.name, s.prefab.GetPrefab(gameObject, spot.gameObject), scene, t.position, t.rotation, s.spawnOptions, out _, out _
                );
            }
        }
    }

    
    #if UNITY_EDITOR
    [CustomEditor(typeof(RandomEncounter))] class RandomEncounterEditor : Editor {
        public override void OnInspectorGUI() {
            // base.OnInspectorGUI();
        }
    }
    #endif

    
}
