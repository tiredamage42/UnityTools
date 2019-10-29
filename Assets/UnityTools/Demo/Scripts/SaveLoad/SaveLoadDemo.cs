using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEngine.SceneManagement;
using UnityTools;
namespace UnityToolsDemo {

    public class SaveLoadDemo : MonoBehaviour
    {
        public void SaveGame () {
            SaveLoad.SaveGameState(0);
        }
        public void LoadGame () {
            SaveLoad.LoadGameState(0);
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(SaveLoadDemo))] public class SaveLoadDemoEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Save") && Application.isPlaying) (target as SaveLoadDemo).SaveGame();
            if (GUILayout.Button("Load") && Application.isPlaying) (target as SaveLoadDemo).LoadGame();
        }
    }
    #endif
}

