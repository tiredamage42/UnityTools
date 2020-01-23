using UnityEngine;
namespace UnityTools.InteractionSystem {
    /*
        add to collider sub elements of any interactable, so they'll trigger interactor checks
    */
    public class InteractableTrigger : MonoBehaviour
    {
        [HideInInspector] public Interactable interactable;
    }
}
