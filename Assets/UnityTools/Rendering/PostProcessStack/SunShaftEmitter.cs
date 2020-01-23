using UnityEngine;
namespace UnityTools.Rendering.Internal {
    public class SunShaftEmitter : MonoBehaviour {
        static SunShaftEmitter _instance;
        public static SunShaftEmitter instance {
            get {
                if (_instance == null || !_instance.gameObject.activeInHierarchy)
                    _instance = GameObject.FindObjectOfType<SunShaftEmitter>();
                return _instance;
            }
        }
    }
}
