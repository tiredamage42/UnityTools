
using UnityEngine;

namespace UnityTools {

    public class TransformShake<T, I> 
        where T : Shake<I>, new ()    
        where I : ShakeInfo
    {

        public Vector3 positionOffset { get { return position.currentOffset; } }
        public Vector3 rotationOffset { get { return rotation.currentOffset; } }

        public T position = new T ();
        public T rotation = new T ();

        public void StartShake (I positionShake, I rotationShake) {
            position.StartShake(positionShake);
            rotation.StartShake(rotationShake);
        }

        public void FixedUpdate (out bool positionUpdated, out bool rotationUpdated) {
            positionUpdated = position.FixedUpdate();
            rotationUpdated = rotation.FixedUpdate();
        }
        public void Update(out bool positionUpdated, out bool rotationUpdated) {
            positionUpdated = position.Update();
            rotationUpdated = rotation.Update();
        }    
        public void LateUpdate (out bool positionUpdated, out bool rotationUpdated) {
            positionUpdated = position.LateUpdate();
            rotationUpdated = rotation.LateUpdate();
        }
    }
}