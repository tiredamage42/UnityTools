using UnityEngine;

namespace UnityTools.InteractionSystem {

    /*
        add to collider sub elements of any interactable, so they'll trigger interactor checks
    */
    

    public class InteractableTrigger : MonoBehaviour
    {
        [HideInInspector] public Interactable interactable;
        void Awake () {
            if (!Application.isPlaying) {
                return;
            }
            
            //initialize physics to trigger and kinematic rigidbody
            GetComponent<Collider>().isTrigger = true;

            // initialize rb (add if none)
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent <Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            //set layer
            int interactableLayer;
            if (Layers.LayerExists(Interactable.interactableLayerName, out interactableLayer)) {
                gameObject.layer = interactableLayer;
            }
        }
    }
}
