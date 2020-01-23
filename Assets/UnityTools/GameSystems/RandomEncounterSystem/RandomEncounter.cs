
using UnityEngine;
using System;
using UnityEditor;
using UnityTools.RandomEncounters.Internal;
using System.Collections.Generic;
namespace UnityTools.RandomEncounters {

    [Serializable] public class RandomEncounterState : ObjectAttachmentState {
        public string aliasPrefix;
        public RandomEncounterState (RandomEncounter instance) { 
            this.aliasPrefix = instance.aliasPrefix;
        }       
    }

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
            aliasPrefix = null;
        }

        public ObjectAttachmentState GetState () {
            return new RandomEncounterState (this);
        }

        public void LoadState (ObjectAttachmentState state) {
            RandomEncounterState encounterState = state as RandomEncounterState; 
            aliasPrefix = encounterState.aliasPrefix;
        }

        [NonSerialized] public string aliasPrefix;        

        public ObjectLoadState GetSpawnedObject (string key, out object obj) {
            return DynamicObjectManager.GetObjectByAlias(aliasPrefix + key, out obj);
        }

        public void SpawnObjects (string scene, RandomEncounterSpot spot) {
            this.aliasPrefix = scene + "." + spot.name + "." + name + ".";
            RandomEncounterSpawn[] spawns = GetComponentsInChildren<RandomEncounterSpawn>();
            for (int i = 0; i < spawns.Length; i++) {
                RandomEncounterSpawn s = spawns[i];
                Transform t = s.transform;
                var getPrefabsRuntimeSubjects = new Dictionary<string, object> () { { "Encounter", this }, { "EncounterSpot", spot } };
                DynamicObjectManager.AddNewAliasedObject (aliasPrefix + s.name, s.prefab.GetPrefab(getPrefabsRuntimeSubjects), scene, t.position, t.rotation, false, out _);
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
