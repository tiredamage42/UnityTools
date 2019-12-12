

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityTools.GameSettingsSystem;
using UnityTools.Internal;
using System;

using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;

using UnityTools.InitializationSceneWorkflow;

// using UnityTools.Spawning;
using UnityEngine.AI;

using UnityTools.FastTravelling;

namespace UnityTools {

    // added at first initialization...
    public abstract class InitializationSingleTon<T> : Singleton<T> where T : MonoBehaviour { }

    public class GameManager : Singleton<GameManager>
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void _LoadGameManager1()
        {
            Debug.Log("GameManager: Initilializing Game");
            
            // Debug.Log(Application.streamingAssetsPath);
            // Debug.Log("Exists: " + System.IO.Directory.Exists(Application.streamingAssetsPath));

            // Debug.Log("Loading Game Manager");
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void _LoadGameManager2()
        {
            Debug.Log("GameManager: Loading Initialization Scenes");
            InitializationScenes.LoadAdditionalInitializationScenes();
        }


        static GameManagerSettings settings { get { return GameManagerSettings.instance; } }
        public static LayerMask environmentMask { get { return settings.environmentMask; } }



        /*
            start and awake should only happen during the initial scene load
        */
        protected override void Awake() {
            base.Awake();
            if (!thisInstanceErrored) {
                
                SceneLoading.onSceneLoadStart += OnSceneLoadStart;
                SaveLoad.onSaveGame += OnSaveGame;
                SceneManager.sceneLoaded += OnSceneLoaded;
                
                if (settings.actionsController != null) 
                    settings.actionsController.InitializeActionsInterface();
            
                // get all types of InitializationSingleTon's
                Type[] results = SystemTools.FindAllDerivedTypes(typeof(InitializationSingleTon<>));
                
                // add tehm to this gameObject
                for (int i = 0; i < results.Length; i++) {
                    // Debug.Log("Adding Initialization Singletons " + results[i].Name);
                    gameObject.AddComponent(results[i]);
                }

                for (int i = 0; i < settings.initialPrefabSpawns.Length; i++) {
                    Instantiate(settings.initialPrefabSpawns[i]);
                }
            }
        }   

        const string playerSaveKey = "GAMEPLAYER";
        void OnSaveGame (List<string> allActiveLoadedScenes) {
            SaveLoad.gameSaveState.UpdateSaveState (playerSaveKey, player.GetState());
        }

        void OnSceneLoaded (Scene scene, LoadSceneMode mode)
        {
            if (IsMainMenuScene(scene.name))
                return;

            if (!SaveLoad.isLoadingSaveSlot)
                return;

            DynamicObjectState savedPlayer = (DynamicObjectState)SaveLoad.gameSaveState.LoadSaveStateObject(playerSaveKey);
            player.Load(savedPlayer);
            player.transform.WarpTo(savedPlayer.position, savedPlayer.rotation);
        }
        
        public bool showPause;

        void Update () {
            pauseManager.Update();
            showPause = isPaused;
        }
        
        public static int maxSaveSlots { get { return settings.maxSaveSlots; } }
        public static bool isQuitting;
        public const string playerTag = "Player";

        void OnApplicationQuit () {
            isQuitting = true;
        }

        void Start () {
            if (!thisInstanceErrored) {
                SceneLoading.SetActiveScene(InitializationScenes.mainInitializationScene);

                SaveLoad.LoadSettingsOptions();

                // #if UNITY_EDITOR
                StartCoroutine(SkipToScene());
                // #endif

                #if !UNITY_EDITOR
                // just to not get stuck in full screen mode during test builds...
                StartCoroutine(QuitDebug());
                #endif


            }
        }
        IEnumerator QuitDebug() {
            yield return new WaitForSecondsRealtime(60);
            QuitApplication();
        }
            
        // #if UNITY_EDITOR
        IEnumerator SkipToScene() {
            yield return new WaitForSecondsRealtime(3);
            // Debug.Log("Skippng to scene");

            FastTravel.FastTravelTo(settings.editorSkipToSpawn);
            // GameManagerSettings.instance.editorSkipToSpawn.DoFastTravel();
        }

        // #endif


        public static event Action onLoadMainMenu, onExitMainMenu;

        static void OnSceneLoadStart (string targetScene, LoadSceneMode mode) {
            if (mode != LoadSceneMode.Additive) {

                if (targetScene == InitializationScenes.mainInitializationScene) {
                    
                    if (onLoadMainMenu != null)
                        onLoadMainMenu();
                
                    DestroyPlayer();
                }
                else {
                    BuildPlayer();

                    if (isInMainMenuScene) {
                        if (onExitMainMenu != null)
                            onExitMainMenu();
                    }
                } 
            }   
        }

        #region PAUSE_GAME
        static PauseManager pauseManager = new PauseManager ();
        public static bool isPaused { get { return pauseManager.isPaused; } }
        

        public static void PauseGame (object pauseObject) {
            pauseManager.PauseGame(pauseObject);
        }

        public static void UnpauseGame (object pauseObject) {
            pauseManager.UnpauseGame(pauseObject);
        }

        public static void TogglePase (object pauseObject) {
            pauseManager.TogglePase(pauseObject);
        }
        #endregion


        #region PLAYER
        public static bool playerExists { get { return DynamicObject.playerObject != null; } }
        public static DynamicObject player {
            get {
                if (!playerExists) Debug.LogError("Player not instantiated!!!");
                return DynamicObject.playerObject;
            }
        }
        public static Camera playerCamera { get { return PlayerCamera.myCamera; } }
        static void BuildPlayer () {
            if (!playerExists) {
                GameObject.Instantiate(GameManagerSettings.instance.playerPrefab);
            }
        }
        static void DestroyPlayer () {
            if (playerExists) {
                GameObject.Destroy(player.gameObject);
            }
        }
        #endregion

        public static bool IsMainMenuScene(string scene) { return scene == InitializationScenes.mainInitializationScene; }  
        public static bool isInMainMenuScene { get { return IsMainMenuScene(SceneLoading.activeScene); } }        

        public static void StartNewGame () {
            DestroyPlayer();
            SaveLoad.ClearGameSaveState();

            FastTravel.FastTravelTo(settings.newGameSpawn);
            // GameManagerSettings.instance.newGameSpawn.DoFastTravel();
        }

        public static void QuitToMainMenu () {
            SaveLoad.ClearGameSaveState();
            InitializationScenes.LoadInitializationScene();
        }

        // public static string[] GetAllScenes () {
        //     int sceneCount = SceneManager.sceneCountInBuildSettings;
        //     string[] allScenes = new string[sceneCount];
        //     for (int i = 0; i < sceneCount; i++)
        //     {
        //         string path = SceneUtility.GetScenePathByBuildIndex(i);
        //         string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        //         allScenes[i] = sceneName;
        //     }
        //     return allScenes;
        // }


        public static void QuitApplication () {
            SaveLoad.SaveSettingsOptions();

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit ();
    #endif
        }


        public static Vector3 GroundPosition (Vector3 pos, bool stickToGround, bool stickToNavMesh, out Vector3 up) {
            up = Vector3.up;
            if (stickToGround) {
                RaycastHit hit;
                if (Physics.Raycast(pos, Vector3.down, out hit, settings.groundCheckDistance, environmentMask, QueryTriggerInteraction.Ignore)) {
                    pos = hit.point;
                    up = hit.normal;
                }
            }
            if (stickToNavMesh) {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(pos, out hit, settings.navmeshCheckDistance, NavMesh.AllAreas)) 
                    pos = hit.position;
            }
            return pos;
        }


        public static void UnIntersectTransform (Transform transform, Vector3 nudgeDir) {
            transform.WarpTo( UnIntersectColliderGroup(transform.GetComponentsInChildren<Collider>(), transform.position, nudgeDir), transform.rotation);
        }


        public static Vector3 UnIntersectColliderGroup (Collider[] group, Vector3 originalRoot, Vector3 nudgeDir) {
            int tries = 0;

            // Vector3 root = originalRoot;
            Vector3 offset = Vector3.zero;
            while (ColliderGroupIntersects(group, offset)){
                // root += nudgeDir * unIntersectNudge;
                offset += nudgeDir * unIntersectNudge;
                tries++;
                if (tries >= maxUnIntersectTries) {
                    // Debug.LogWarning("Max Intersection Tries Reached, giving up");
                    return originalRoot;
                }
            }
            return originalRoot + offset;
        }

        const int maxUnIntersectTries = 500;
        const float unIntersectNudge = .01f;
        


        const int physicsCheckOverlapCount = 10;
        static Collider[] hits = new Collider[physicsCheckOverlapCount];
        static bool ColliderInIgnores (Collider c, Collider[] ignores) {
            for (int x = 0; x < ignores.Length; x++) {
                if (c == ignores[x]) {
                    return true;
                }
            }
            return false;
        }
        static bool CollidersAreHit (Collider[] ignores, int length) {

            for (int i = 0; i < length; i++) {
                if (hits[i] != null) {
                    if (!ColliderInIgnores(hits[i], ignores)) {
                        return true;
                    }
                }
            }
            return false;
        }
        static bool ColliderGroupIntersects(Collider[] group, Vector3 offset) {
            for (int i = 0; i < group.Length; i++) {
                Collider c = group[i];

                float scale = c.transform.lossyScale.x;
                Vector3 pos = c.transform.position + offset;
                Quaternion rot = c.transform.rotation;

                SphereCollider sphere = c as SphereCollider;
                CapsuleCollider capsule = c as CapsuleCollider;
                BoxCollider box = c as BoxCollider;

                if (sphere != null) {
                    if (CollidersAreHit(group, Physics.OverlapSphereNonAlloc(pos + sphere.center * scale, sphere.radius * scale, hits, environmentMask, QueryTriggerInteraction.Ignore)))
                        return true;
                }
                else if (capsule != null) {
                    // x y z
                    Vector3 dir = capsule.direction == 0 ? c.transform.right : (capsule.direction == 1 ? c.transform.up : c.transform.forward);
                    Vector3 mod0 = pos + capsule.center * scale;
                    Vector3 mod1 = dir * ((capsule.height * .5f - capsule.radius * .5f) * scale);
                    if (CollidersAreHit(group, Physics.OverlapCapsuleNonAlloc(mod0 + mod1, mod0 - mod1, capsule.radius * scale, hits, environmentMask, QueryTriggerInteraction.Ignore)))
                        return true;
                }
                else if (box != null) {
                    if (CollidersAreHit(group, Physics.OverlapBoxNonAlloc(pos, box.size * .5f * scale, hits, rot, environmentMask, QueryTriggerInteraction.Ignore)))
                        return true;
                }
                else {
                    if (CollidersAreHit(group, Physics.OverlapBoxNonAlloc(pos, c.bounds.size * .5f * scale, hits, rot, environmentMask, QueryTriggerInteraction.Ignore)))
                        return true;
                }   
            }
            return false;
        }
    }   
}