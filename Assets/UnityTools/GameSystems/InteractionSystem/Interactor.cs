
using UnityEngine;

namespace UnityTools.InteractionSystem {
    public class Interactor : DynamicObjectScript<Interactor>
    {        
        public int interactionMode;
        [HideInInspector] public float rayCheckDistance = 1f;
        InteractionPoint[] childInteractors;

        public void SetInteractionMode (int interactionMode, float rayCheckDistance) {
            if (rayCheckDistance <= 0) 
                rayCheckDistance = InteractionSettings.instance.defaultRaycheckDistance;
            
            this.rayCheckDistance = rayCheckDistance;
            this.interactionMode = interactionMode;
        }

        void Awake () {
            childInteractors = GetComponentsInChildren<InteractionPoint>();
            for (int i =0 ; i < childInteractors.Length; i++) {
                childInteractors[i].SetBaseInteractor(this);
            }   
            SetInteractionMode(0, -1);
        }
        
        InteractionPoint GetInteractionPointByControllerID (int controllerIndex) {
            for (int i =0 ; i < childInteractors.Length; i++) {
                if (childInteractors[i].associatedController == controllerIndex)
                    return childInteractors[i];
            }
            return null;
        }
        public InteractionPoint mainInteractor { get { return GetInteractionPointByControllerID(0); } }
    }
}
