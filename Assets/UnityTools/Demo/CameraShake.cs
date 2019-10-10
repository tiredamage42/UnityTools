﻿using UnityEngine;
using UnityEditor;

using UnityTools;
namespace UnityToolsDemo {

    public class CameraShake : MonoBehaviour
    {

        public DurationShakeInfo positionShake;
        public DurationShakeInfo rotationShake;

        TransformShake<DurationShake, DurationShakeInfo> transformShake = new TransformShake<DurationShake, DurationShakeInfo>();


        public void ShakeCamera () {
            transformShake.StartShake(positionShake, rotationShake);
        }

        void Update()
        {
            transformShake.Update(out _, out _);
            
            transform.localPosition = transformShake.positionOffset;
            transform.localRotation = Quaternion.Euler(transformShake.rotationOffset);
        }
        void FixedUpdate () {
            transformShake.FixedUpdate(out _, out _);
        }    
        void LateUpdate () {
            transformShake.LateUpdate(out _, out _);
        }
            
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(CameraShake))] public class CameraShakeEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Shake") && Application.isPlaying) (target as CameraShake).ShakeCamera();
        }
    }
    #endif
}
