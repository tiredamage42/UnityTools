using UnityEngine;

namespace UnityTools {
    public class PlayerCamera : MonoBehaviour
    {
        static PlayerCamera _i;
        public static PlayerCamera instance { get { return Singleton.GetInstance<PlayerCamera>(ref _i); } }
        static Camera _camera;
        public static Camera camera {
            get {
                if (_camera == null) {
                    if (instance != null) {
                        _camera = instance.GetComponent<Camera>();
                    }
                }
                return _camera;
            }
        }
    }
}
