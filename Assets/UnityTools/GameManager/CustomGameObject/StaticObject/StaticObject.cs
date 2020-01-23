
/*
    same as the dynamic object system, just to save states for static objects that wont 
    move, be removed, or added to scenes.

    example:
        the random encounter hotspots or animation markers level designers place in the scene

*/
    
using System.Collections.Generic;
using UnityEditor;
namespace UnityTools {
    public class StaticObject : CustomGameObject {
        public static Dictionary<string, List<StaticObject>> activeObjects = new Dictionary<string, List<StaticObject>>();
        protected override void OnEnable () {
            base.OnEnable();
            string scene = gameObject.scene.name;
            if (activeObjects.ContainsKey(scene)) 
                activeObjects[scene].Add(this);
            else 
                activeObjects[scene] = new List<StaticObject>() { this };
        }
        void OnDisable () {
            string scene = gameObject.scene.name;
            if (activeObjects.ContainsKey(scene)) 
                activeObjects[scene].Remove(this);
        }
        public void Load(ObjectState state) {
            SetLoadedVersion(state);
            LoadAttachedStates(state);        
        }
        public ObjectState GetState () {
            ObjectState state = new ObjectState();
            SetLoadedVersion(state);
            GetAttachedStates (state);
            return state;
        }   
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(StaticObject))] class StaticObjectEditor : Editor {
        public override void OnInspectorGUI() {
            // base.OnInspectorGUI();
        }
    }
    #endif
}