

using UnityEngine;
using UnityEditor;

using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;

namespace UnityTools.Internal {
    
    // [CreateAssetMenu()]
    public class GameManagerSettings : GameSettingsObjectSingleton<GameManagerSettings> {
        
        [Header("Environment")]
        public LayerMask environmentMask = Physics.DefaultRaycastLayers; 
        

        public int maxSaveSlots = 6;
        public GameObject playerPrefab;
        public ActionsInterfaceController actionsController;
        public Location newGameLocation;

        /*
            cache scene architecture..., 
            initialization scenes (loaded once on application start)
            main menu additive scenes (loaded whenever main menu scene is loaded)
        */
        [HideInInspector] public string[] initSceneNames, mmSceneNames;

        #if UNITY_EDITOR
        // keep serialized... (but only in editor)
        public string mainMenuScenePath;
        #endif
    }



    #if UNITY_EDITOR
    [CustomEditor(typeof(GameManagerSettings))]
    public class GameManagerSettingsEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        }
    }
    #endif
}