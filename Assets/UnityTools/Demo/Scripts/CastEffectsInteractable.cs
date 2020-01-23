

using UnityEngine;
using System.Collections.Generic;
using UnityTools;
using UnityTools.InteractionSystem;
namespace UnityToolsDemo {
    public class CastEffectsInteractable : MonoBehaviour, IInteractable
    {
        [Action] public int castAction;
        public GameEffectCollection effects;
        public string interactName = "Cast Effects";

        public void OnInteractableAvailabilityChange(bool available) { }
        public bool IsAvailable(int interactionMode) { 
            return enabled && interactionMode == 0;
        }
        public void DecideInteractionName (ref string interactionName) {
            if (string.IsNullOrEmpty(interactionName)) {
                interactionName = interactName;
            }   
        }
        public void AddInteractionHints (List<int> actions, List<string> hints) {
            actions.Add(castAction);
            hints.Add("Use");
        }

        public void OnInteractableInspectChange (InteractionPoint interactor, StateChange state) { }
        public void OnInteractableActionChange (InteractionPoint interactor, int action, StateChange state) { 
            if (state == StateChange.Start) {
                if (action == castAction) {
                    GameEffect.TriggerEffectsOnObject(effects, null, interactor.transform.position, interactor.baseInteractor.dynamicObject);
                }
            }
        }
    }
}