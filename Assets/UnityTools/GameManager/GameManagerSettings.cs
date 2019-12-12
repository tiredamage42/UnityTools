

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;
using UnityTools.InitializationSceneWorkflow;

using UnityTools.FastTravelling;

namespace UnityTools.Internal {
    
    // [CreateAssetMenu(menuName="Unity Tools/Internal/Game Manager Settings", fileName="UnityTools_GameManagerSettings")]
    public class GameManagerSettings : GameSettingsObjectSingleton<GameManagerSettings> {
        
        // if (Physics.Raycast(pos, Vector3.down, out hit, settings.groundCheckDistance, settings.groundCheckMask, QueryTriggerInteraction.Ignore)) 
        //             pos = hit.point;
        //     }
        //     if (stickToNavMesh) {
        //         NavMeshHit hit;
        //         if (NavMesh.SamplePosition(pos, out hit, settings.navmeshCheckDistance, NavMesh.AllAreas))
        //             pos = hit.position;

        [Header("Environment")]
        public float groundCheckDistance = 5;
        public LayerMask environmentMask = Physics.DefaultRaycastLayers; 
        public float navmeshCheckDistance = 2;
        
        
        [NeatArray] public NeatGameObjectArray initialPrefabSpawns;

        public int maxSaveSlots = 6;
        public GameObject playerPrefab;
        public ActionsInterfaceController actionsController;
        public FastTravelLocation newGameSpawn;
        
        // #if UNITY_EDITOR

        [Header("EDITOR: ")]
        [Tooltip("Use To Skip To A Certain Spawn Automatically After Starting Initial Scene in Editor")]
        public FastTravelLocation editorSkipToSpawn;
        // #endif

    }



    #if UNITY_EDITOR
    [CustomEditor(typeof(GameManagerSettings))]
    public class GameManagerSettingsEditor : Editor {


        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            GUITools.Space(3);
            bool playInitialSceneFirst = InitializationScenes.LoadInitialOnPlay;
            GUI.backgroundColor = playInitialSceneFirst ? GUITools.blue : GUITools.white;
            if (GUILayout.Button("Play Initial Scene First In Editor")) {
                InitializationScenes.LoadInitialOnPlay = !playInitialSceneFirst;
            }
            GUI.backgroundColor = GUITools.white;

            if (GUILayout.Button("Open Master Scene In Editor")) {
                EditorSceneManager.OpenScene(InitializationScenes.instance.mainInitializationScenePath);
            }

            GUITools.Space();
            
        }
    }


    #endif
}