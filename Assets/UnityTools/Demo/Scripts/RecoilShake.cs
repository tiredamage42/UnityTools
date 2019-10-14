using UnityEngine;

using UnityEditor;
using UnityTools;
namespace UnityToolsDemo {

    public class RecoilShake : MonoBehaviour
    {
        public Transform recoilTransform;

        Vector3 originalLocalPos, originalLocalRot;
        void Start () {

            if (recoilTransform == null) 
                return;
            
            originalLocalPos = recoilTransform.localPosition;
            originalLocalRot = recoilTransform.localRotation.eulerAngles;
            
            transformShake.position.onShakeEnd += OnPositionShakeEnd;
            transformShake.rotation.onShakeEnd += OnRotationShakeEnd;
        }

        public SimpleShakeInfo position, rotation;

        TransformShake<SimpleShaker, SimpleShakeInfo> transformShake = new TransformShake<SimpleShaker, SimpleShakeInfo>();

        void OnPositionShakeEnd () {
            recoilTransform.localPosition = originalLocalPos;
        }
        void OnRotationShakeEnd () {
            recoilTransform.localRotation = Quaternion.Euler(originalLocalRot);
        }
        
        void HandleTransformMove (bool positionUpdated, bool rotationUpdated) {
            if (rotationUpdated) recoilTransform.localRotation = Quaternion.Euler(originalLocalRot + transformShake.rotationOffset);
            if (positionUpdated) recoilTransform.localPosition = originalLocalPos + transformShake.positionOffset;
        }
        
        void Update()
        {
            if (recoilTransform == null) 
                return;
            
            bool positionUpdated, rotationUpdated;
            transformShake.Update(out positionUpdated, out rotationUpdated);
            HandleTransformMove(positionUpdated, rotationUpdated);
        }
        void FixedUpdate()
        {
            if (recoilTransform == null) 
                return;
            
            bool positionUpdated, rotationUpdated;
            transformShake.FixedUpdate(out positionUpdated, out rotationUpdated);
            HandleTransformMove(positionUpdated, rotationUpdated);
        }
        void LateUpdate()
        {
            if (recoilTransform == null) 
                return;
            
            bool positionUpdated, rotationUpdated;
            transformShake.LateUpdate(out positionUpdated, out rotationUpdated);
            HandleTransformMove(positionUpdated, rotationUpdated);
        }
        
        public void StartRecoil () {
            if (recoilTransform == null) 
                return;
            
            transformShake.StartShake(position, rotation);
        }
    }

    
    #if UNITY_EDITOR
    [CustomEditor(typeof(RecoilShake))] public class RecoilShakeEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Shake") && Application.isPlaying) (target as RecoilShake).StartRecoil();
        }
    }
    #endif
}
