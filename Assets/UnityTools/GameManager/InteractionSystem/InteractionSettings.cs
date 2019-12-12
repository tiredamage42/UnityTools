
using UnityEngine;
using UnityTools.GameSettingsSystem;
namespace UnityTools.InteractionSystem {


    // [CreateAssetMenu()]
    public class InteractionSettings : GameSettingsObjectSingleton<InteractionSettings> {
        public bool useProximityCheck = true;
        public float proximityRadius = .1f;
        public bool useRayCheck = true;
        public float defaultRaycheckDistance = 1f;

        [Tooltip("Be able to interact using ray check, with interactables labeled as only proximity usage")]
        public bool ignoreOnlyProximity;

    }
}