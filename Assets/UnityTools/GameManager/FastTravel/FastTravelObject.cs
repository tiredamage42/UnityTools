using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using UnityTools.InteractionSystem;
using UnityTools.Spawning;


namespace UnityTools.FastTravelling {
    public class FastTravelObject : MonoBehaviour, IInteractable
    {
        [Action] public int travelAction;
        public FastTravelLocation location;
        public void DoFastTravel () {
            FastTravel.FastTravelTo (location);
        }
        
        public int GetInteractionMode () {
            return 0;   
        }
        public void OnInteractableInspectedStart(InteractionPoint interactor) { }
        public void OnInteractableInspectedEnd(InteractionPoint interactor) { }
        public void OnInteractableInspectedUpdate(InteractionPoint interactor) { }
        public void OnInteractableActionDown(InteractionPoint interactor, int action) { 
            if (action == travelAction) {
                DoFastTravel();
            }
        }
        public void OnInteractableActionUp(InteractionPoint interactor, int action) { }
        public void OnInteractableAction(InteractionPoint interactor, int action) { }
        public void OnInteractableAvailabilityChange(bool available) { }

        public void DecideInteractionName (ref string interactionName) {
            if (string.IsNullOrEmpty(interactionName)) {
                interactionName = location.location.scene;
            }   
        }
        public void AddInteractionHints (List<int> actions, List<string> hints) {
            actions.Add(travelAction);
            hints.Add("Travel");
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(FastTravelObject))] 
    public class FastTravelObjectEditor : Editor {

        bool tick;            
        void OverrideSpawnLocation () {
            SerializedProperty location = serializedObject.FindProperty("location");
            if (tick) {
                tick = false;
                FastTravelLocationEditor.RestoreLocationValues(location);
            }
            if(!Application.isPlaying) {
                if (!FastTravelLocationEditor.LocationIsPrefabOverride(location)) {
                    tick = true;
                    FastTravelLocationEditor.AdjustLocationValues(location);
                }
            }
        }
            
        public override void OnInspectorGUI() {
            OverrideSpawnLocation();
            base.OnInspectorGUI();
        }
    }
    #endif
}