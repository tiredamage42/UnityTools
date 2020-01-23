using UnityEngine;
using System.Collections.Generic;

using UnityTools.Rendering;
namespace UnityTools.InteractionSystem
{
    public interface IInteractable {
        void OnInteractableInspectChange (InteractionPoint interactor, StateChange state);
        void OnInteractableActionChange (InteractionPoint interactor, int action, StateChange state);        
        bool IsAvailable(int interactionMode);    
        void OnInteractableAvailabilityChange(bool available);
        void AddInteractionHints (List<int> actions, List<string> hints);
        void DecideInteractionName (ref string interactionName);
    }

    public enum StateChange { Start, End, Update }

    public class Interactable : MonoBehaviour
    {
        public bool onlyProximityHover;
        public bool isAvailable = true;        
        public enum UseType { Normal, Scripted };
        public UseType useType;
        HashSet<int> currentHoveringIDs = new HashSet<int>();
        IInteractable[] listeners;


        [Tooltip("-1 for any interaction point")]
        public int enforceInteractorController = -1;
        
        void InitializeListeners() {
            listeners = GetComponents<IInteractable>();
        }
        void InitializeSubElements () {
            InteractableTrigger[] triggers = GetComponentsInChildren<InteractableTrigger>();
            for (int i = 0; i < triggers.Length; i++) triggers[i].interactable = this;
        }
        Renderer[] myRenderers;
        void Awake() {
            InitializeListeners();
            InitializeSubElements();
            myRenderers = GetComponentsInChildren<Renderer>();
        }

        void OnEnable () {
            Outlines.HighlightRenderers(myRenderers, BasicColor.Black, false, "AO");
        }
        void OnDisable () {
            currentHoveringIDs.Clear();
        }

        void Start () {
            SetAvailable(isAvailable);
        }

        bool ListenerOK (IInteractable listener, int forInteractionMode) {
            return listener.IsAvailable(forInteractionMode);
        }

        public bool IsAvailable (int forInteractionMode, int forController) {
            if (!isAvailable) 
                return false;
            
            if (enforceInteractorController != -1 && enforceInteractorController != forController) 
                return false;
            
            for (int i = 0; i < listeners.Length; i++) {
                if (ListenerOK(listeners[i], forInteractionMode)) 
                    return true;
            }
            return false;
        }

        public void SetAvailable (bool available) {
            this.isAvailable = available;
            for (int i = 0; i < listeners.Length; i++) 
                listeners[i].OnInteractableAvailabilityChange(available);
        }
            
        void BuildInteractionHintsAndMessage (int forInteractionMode, out string message, out List<int> actions, out List<string> hints) {            
            message = "";
            actions = new List<int>();
            hints = new List<string>();
            
            for (int i = 0; i < listeners.Length; i++) {
                if (ListenerOK(listeners[i], forInteractionMode)) {
                    listeners[i].AddInteractionHints(actions, hints);
                    listeners[i].DecideInteractionName(ref message);
                }
            }
        }

        public void OnInspectedStart (InteractionPoint interactor, int forInteractionMode, out string message, out List<int> actions, out List<string> hints) {
            BroadcastInspectChange ( interactor, forInteractionMode, StateChange.Start );
            bool wasHovered = currentHoveringIDs.Count > 0;
            currentHoveringIDs.Add(interactor.GetInstanceID());
            BuildInteractionHintsAndMessage ( forInteractionMode, out message, out actions, out hints );
            if (!wasHovered) 
                Tagging.TagRenderers(myRenderers, BasicColor.Green, false, true);   
        }
        public void OnInspectedEnd (InteractionPoint interactor, int forInteractionMode) {
            BroadcastInspectChange ( interactor, forInteractionMode, StateChange.End );
            currentHoveringIDs.Remove(interactor.GetInstanceID());
            if (currentHoveringIDs.Count == 0) 
                Tagging.UntagRenderers(myRenderers);
        }
        public void OnInspectedUpdate(InteractionPoint interactor, int forInteractionMode) {
            BroadcastInspectChange ( interactor, forInteractionMode, StateChange.Update );
        }

        public void BroadcastActionChange (InteractionPoint interactor, int forInteractionMode, int action, StateChange state) {
            for (int x = 0; x < listeners.Length; x++) 
                if (ListenerOK(listeners[x], forInteractionMode)) 
                    listeners[x].OnInteractableActionChange(interactor, action, state);
        }
        void BroadcastInspectChange (InteractionPoint interactor, int forInteractionMode, StateChange state) {
            for (int i = 0; i < listeners.Length; i++) 
                if (ListenerOK(listeners[i], forInteractionMode)) 
                    listeners[i].OnInteractableInspectChange(interactor, state);
        }       
    }
}