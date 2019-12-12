
using System;
using UnityEngine;
using UnityTools.EditorTools;

namespace UnityTools.RandomEncounters {

    [Serializable] public class RandomEncounterSpotState : ObjectAttachmentState {
        public bool hasActiveEncounter;
        public RandomEncounterSpotState (RandomEncounterSpot instance) { 
            this.hasActiveEncounter = instance.hasActiveEncounter;
        }       
    }

    // needs static object script attached
    public class RandomEncounterSpot : MonoBehaviour, IObjectAttachment
    {

        void OnDrawGizmos () {
            Gizmos.color = new Color (0, 0, 1, .5f);
            Gizmos.DrawCube(transform.position + Vector3.up * .5f, Vector3.one);
        }

        public int InitializationPhase () { return 0; }

        public void InitializeDefault () {
            // must happen after scene load.....
            if (! hasActiveEncounter ) {
                // pick random encounter

                PrefabReference prefab = randomEncounters.GetRandomPrefabChoice(gameObject, gameObject);
                // RandomEncounter prefab = randomEncounters.GetRandom(null);
                // if (prefab != null) {

                    RandomEncounter encounter = RandomEncounter.GetAvailableInstance(prefab, transform.position, transform.rotation);
                    
                    if (encounter != null) {

                        encounter.SpawnObjects(gameObject.scene.name, this);

                        hasActiveEncounter = true;
                    }
                // }
            }
        }

        public void Strip () {
            hasActiveEncounter = false;
        }

        public ObjectAttachmentState GetState () {
            return new RandomEncounterSpotState (this);
        }

        public void LoadState (ObjectAttachmentState state) {
            RandomEncounterSpotState encounterState = state as RandomEncounterSpotState; 
            hasActiveEncounter = encounterState.hasActiveEncounter;
        }

        // public RandomPrefabChoice randomEncounters;
        public RandomPrefabChoices randomEncounters;


        // [NeatArray] public RandomEncounterArray randomEncounters;
        [NonSerialized] public bool hasActiveEncounter; //( so it doesnt re spawn everything again when loading cell)       
    }
}