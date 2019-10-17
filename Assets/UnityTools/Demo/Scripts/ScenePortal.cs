using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityTools;
using UnityEditor;
namespace UnityToolsDemo {
    public class ScenePortal : MonoBehaviour
    {
        public string scene;
        public void GoToScene () {
            SceneLoading.LoadSceneAsync (scene, null);
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(ScenePortal))] public class ScenePortalEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Go To Scene") && Application.isPlaying) (target as ScenePortal).GoToScene();
            
        }
    }
    #endif
}
