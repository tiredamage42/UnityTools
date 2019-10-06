using UnityEngine;
using System;
namespace UnityTools {

    /*
        easier way to define transforms in editor and prototype
        
        positions/rotations/scales
    */
    [CreateAssetMenu(menuName="Transform Behavior", fileName="New Transform Behavior")]
    public class TransformBehavior : ScriptableObject {
        
        [System.Serializable] public class Transform {
            public string name;
            public MiniTransform transform;
        }

        public bool position = true;
        public bool rotation = true;
        public bool scale = true;
        
        public Transform[] transforms;
        
    }
}

