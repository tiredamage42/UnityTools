


using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityTools.InteractionSystem;

namespace UnityTools.FastTravelling {
    public class FastTravelObject : MonoBehaviour, IInteractable
    {
        [Action] public int travelAction;
        public Location location;
        public void DoFastTravel (DynamicObject obj) {
            DynamicObjectManager.MoveObject(obj, location);
        }

        public bool IsAvailable(int interactionMode) { 
            return enabled && interactionMode == 0;
        }
        public void OnInteractableActionChange (InteractionPoint interactor, int action, StateChange state) { 
            if (state == StateChange.Start && action == travelAction) {
                DoFastTravel(interactor.baseInteractor.dynamicObject);
            }
        }
        public void OnInteractableInspectChange (InteractionPoint interactor, StateChange state) { }
        public void OnInteractableAvailabilityChange(bool available) { }

        public void DecideInteractionName (ref string interactionName) {
            if (string.IsNullOrEmpty(interactionName)) {
                interactionName = location.GetSceneName ();
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
        public override void OnInspectorGUI() {
            LocationPrefabApplyPrevention.PreventPrefabApply ( serializedObject.FindProperty("location"), ref tick );
            base.OnInspectorGUI();
        }
    }
    #endif
}