

using UnityEngine;
// using UnityEngine.Events;

using UnityTools.EditorTools;
using System.Collections.Generic;

namespace UnityTools.InteractionSystem
{
    public interface IInteractable {
        int GetInteractionMode ();
        void OnInteractableInspectedStart(InteractionPoint interactor);
        void OnInteractableInspectedEnd(InteractionPoint interactor);
        void OnInteractableInspectedUpdate(InteractionPoint interactor);
        void OnInteractableActionDown(InteractionPoint interactor, int action);
        void OnInteractableActionUp(InteractionPoint interactor, int action);
        void OnInteractableAction(InteractionPoint interactor, int action);
        void OnInteractableAvailabilityChange(bool available);
        void AddInteractionHints (List<int> actions, List<string> hints);
        void DecideInteractionName (ref string interactionName);
    }


    public class Interactable : MonoBehaviour
    {
        public const string interactableLayerName = "InteractableTrigger";
        // public string[] actionNames = new string [] { "Use" };
        public bool onlyProximityHover;
        public bool isAvailable = true;        
        public enum UseType { Normal, Scripted };
        public UseType useType;
        HashSet<int> currentHoveringIDs = new HashSet<int>();
        public bool isHovering { get { return currentHoveringIDs.Count != 0; } }
        IInteractable[] listeners;

        string currentInteractionMessage;
        List<int> currentActionOptions;
        List<string> currentActionOptionHints;
        public void GetInteractionHintsAndMessage (int forInteractionMode, out string message, out List<int> actions, out List<string> hints) {
            
            message = null;
            actions = new List<int>();
            hints = new List<string>();
            
            for (int i = 0; i < listeners.Length; i++) {
                if (listeners[i].GetInteractionMode() == forInteractionMode) {
                    listeners[i].AddInteractionHints(actions, hints);
                    listeners[i].DecideInteractionName(ref message);
                }
            }
        }

        

        // List<List<int>> interactionActions = new List<List<int>>();
        // List<List<string>> interactionHints = new List<List<string>>();
        [Tooltip("-1 for any interaction point")]
        public int enforceInteractorController = -1;
        
        // public string interactionMessage;
        

        // [Header("Per Interaction Mode, EG: '0-Drag'")]
        // [NeatArray] public NeatStringList2D actionsAndHints;
        
        // void InitializeActionsAndHints () {

        //     for (int i = 0; i < actionsAndHints.list.Length; i++) {
        //         List<string> actionsAndHintsForInteractionMode = actionsAndHints.list[i];

        //         List<int> actionsList = new List<int>();
        //         List<string> hintsList = new List<string>();
                
        //         interactionActions.Add(actionsList);
        //         interactionHints.Add(hintsList);

        //         for (int x = 0; x < actionsAndHintsForInteractionMode.Count; x++) {
        //             string actionAndHint = actionsAndHintsForInteractionMode[x];

        //             string[] split = actionAndHint.Split('-');
        //             if (split == null || split.Length != 2) {
        //                 Debug.LogError(name + " Interactable: problem with input name Mode: " + i + " Action/Hint: " + x + " '" + actionAndHint + "'");
        //                 continue;
        //             }
        //             actionsList.Add(int.Parse(split[0]));
        //             hintsList.Add(split[1]);
        //         }
        //     }
        // }

        
        
        void InitializeListeners() {
            listeners = GetComponents<IInteractable>();
        }
        void InitializeSubElements () {
            InteractableTrigger[] triggers = GetComponentsInChildren<InteractableTrigger>();
            for (int i = 0; i < triggers.Length; i++) triggers[i].interactable = this;
        }
        void Awake() {
            // InitializeActionsAndHints();
            InitializeListeners();
            InitializeSubElements();
        }

        void Start () {
            SetAvailable(isAvailable);
        }

        public bool IsAvailable (int forInteractionMode) {
            if (!isAvailable) {
                return false;
            }
            for (int i = 0; i < listeners.Length; i++) {
                if (listeners[i].GetInteractionMode() == forInteractionMode) {
                    return true;
                }
            }
            return false;
        }

        public void SetAvailable (bool available) {
            this.isAvailable = available;
            for (int i = 0; i < listeners.Length; i++) listeners[i].OnInteractableAvailabilityChange(available);
        }

        public void OnInspectedStart (InteractionPoint interactor, int forInteractionMode) {
            if (enforceInteractorController != -1 && enforceInteractorController != interactor.associatedController) return;
            currentHoveringIDs.Add(interactor.GetInstanceID());


            Debug.Log("OnInspect STart");
            

            GetInteractionHintsAndMessage ( forInteractionMode, out currentInteractionMessage, out currentActionOptions, out currentActionOptionHints);

            for (int i = 0; i < listeners.Length; i++) 
                if (listeners[i].GetInteractionMode() == forInteractionMode) 
                    listeners[i].OnInteractableInspectedStart(interactor);
        }
        public void OnInspectedEnd (InteractionPoint interactor, int forInteractionMode) {
            if (enforceInteractorController != -1 && enforceInteractorController != interactor.associatedController) return;
            currentHoveringIDs.Remove(interactor.GetInstanceID());
            Debug.Log("OnInspect End");

            for (int i = 0; i < listeners.Length; i++) if (listeners[i].GetInteractionMode() == forInteractionMode) listeners[i].OnInteractableInspectedEnd(interactor);

            currentActionOptions.Clear();
            currentActionOptionHints.Clear();
        }
        public void OnInspectedUpdate(InteractionPoint interactor, int forInteractionMode) {
            int controller = interactor.associatedController;
            if (enforceInteractorController != -1 && enforceInteractorController != controller) return;
            for (int i = 0; i < listeners.Length; i++) 
                if (listeners[i].GetInteractionMode() == forInteractionMode) 
                    listeners[i].OnInteractableInspectedUpdate(interactor);

            

            // for (int i = 0; i < interactionActions[forInteractionMode].Count; i++) {
            //     int action = interactionActions[forInteractionMode][i];
            for (int i = 0; i < currentActionOptions.Count; i++) {
                int action = currentActionOptions[i];

                if (ActionsInterface.GetActionDown(action, controller: controller)) {

                    // Debug.Log("Action Down " + action);
                    for (int x = 0; x < listeners.Length; x++) 
                        if (listeners[x].GetInteractionMode() == forInteractionMode) 
                            listeners[x].OnInteractableActionDown(interactor, action);
                }
                if (ActionsInterface.GetAction(action, controller: controller)) {
                    for (int x = 0; x < listeners.Length; x++) 
                        if (listeners[x].GetInteractionMode() == forInteractionMode) 
                            listeners[x].OnInteractableAction(interactor, action);
                }
                if (ActionsInterface.GetActionUp(action, controller: controller)) {
                    for (int x = 0; x < listeners.Length; x++) 
                        if (listeners[x].GetInteractionMode() == forInteractionMode) 
                            listeners[x].OnInteractableActionUp(interactor, action);
                }

            }
        }
    }
}

