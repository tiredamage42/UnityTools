

using UnityEngine;
using UnityEditor;

using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;

namespace UnityTools.Internal {
    
    // [CreateAssetMenu()]
    public class GameManagerSettings : GameSettingsObjectSingleton<GameManagerSettings> {
        
        [Header("Environment")]
        public float groundCheckDistance = 5;
        public LayerMask environmentMask = Physics.DefaultRaycastLayers; 
        public float navmeshCheckDistance = 2;


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
        const string loadMenuOnPlayKey = "GameManager.LoadMainMenuOnPlay";
        public static bool loadMainMenuOnEditorPlay {
            get { return EditorPrefs.GetBool(loadMenuOnPlayKey, false); }
            set { EditorPrefs.SetBool(loadMenuOnPlayKey, value); }
        }

        static bool _bypassMenuLoad;
        // set to true to temporarily play from current open editor scenes / custom scenes
        public static bool bypassMenuLoadOnEditorPlay {
            get {return _bypassMenuLoad;}
            set {_bypassMenuLoad = value;}
        }

        // keep serialized... (but only in editor)
        public string mainMenuScenePath;
        #endif
    }



    #if UNITY_EDITOR
    [CustomEditor(typeof(GameManagerSettings))]
    public class GameManagerSettingsEditor : Editor {


        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            GUITools.Space(3);
            bool loadMainMenuOnEditorPlay = GameManagerSettings.loadMainMenuOnEditorPlay;
            
            GUI.backgroundColor = loadMainMenuOnEditorPlay ? GUITools.blue : GUITools.white;
            if (GUILayout.Button("Load Main Menu On Editor Play")) {
                GameManagerSettings.loadMainMenuOnEditorPlay = !loadMainMenuOnEditorPlay;
            }
            GUI.backgroundColor = GUITools.white;

            // if (GUILayout.Button("Open Master Scene In Editor"))
            //     EditorSceneManager.OpenScene(InitializationScenes.instance.mainInitializationScenePath);
            
            GUITools.Space();
            
        }
    }
    #endif
}