

using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

using UnityTools.GameSettingsSystem;
using UnityTools.Internal;
using UnityTools.FastTravelling;
using UnityTools.DevConsole;

namespace UnityTools {
    /*
        any singletons that inherit from this class are automatically added to
        the game manager object when the application starts
    */
    public abstract class InitializationSingleTon<T> : Singleton<T> where T : MonoBehaviour { }


    /*
        class that is in charge of the overall architecture of the application and game
        handles:
            initializing application
            starting new games
            creating and destroying the player when entering or leaving "game" mode
            quitting to main menu, or quitting application

        application is set up, so the first scene in the build is the 
        "main menu scene"
        so it is the first scene that loads
    */

    public class GameManager : Singleton<GameManager>
    {
        static GameManagerSettings settings { get { return GameManagerSettings.instance; } }


        #region INITIALIZATION
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnApplicationStart()
        {
            GameObject gameManagerObject = new GameObject("GameManager");
         
            // Debug.Log("Adding Game Manager");
            GameManager gameManager = gameManagerObject.AddComponent<GameManager>();
            
            // Debug.Log("Initializing Inputs");
            InitializeInputActionsController();
            
            // Debug.Log("Adding Initialization Scripts");
            AddInitialSingletons(gameManagerObject);
            
            // Debug.Log("Instantiating Initialization Prefabs");
            InstantiateInitialGamePrefabs();

        }

        // main menu is loaded by starting the application, then we initialize the game manager
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnApplicationStartAfterMainMenuLoad()
        {
            // Debug.Log("Setting Main Menu as Active Scene");
            SceneLoading.SetActiveScene(mainMenuScene);
            
            // Debug.Log("Loading Initialization Scenes");
            LoadInitializationScenes();
            
            // Debug.Log("Loading Additional Main Menu Scenes");
            LoadAdditiveMainMenuScenes();
            
            SceneLoading.onSceneLoadStart += OnSceneLoadStart;
        }

        static void LoadInitializationScenes () {
            for (int i = 0; i < settings.initSceneNames.Length; i++) 
                SceneLoading.LoadSceneAsync (settings.initSceneNames[i], null, null, LoadSceneMode.Additive, false);
        }
        
        static void InitializeInputActionsController () {
            if (settings.actionsController != null) 
                settings.actionsController.InitializeActionsInterface();
        }

        static void AddInitialSingletons(GameObject gameObject) {
            // get all types of InitializationSingleTon's and add tehm to this gameObject
            Type[] initSingletons = SystemTools.FindAllDerivedTypes(typeof(InitializationSingleTon<>));
            for (int i = 0; i < initSingletons.Length; i++) 
                gameObject.AddComponent(initSingletons[i]);
        }

        static void InstantiateInitialGamePrefabs () {
            List<InitialGamePrefabs> initialPrefabs = GameSettings.GetSettingsOfType<InitialGamePrefabs>();
            for (int p = 0; p < initialPrefabs.Count; p++) 
                for (int i = 0; i < initialPrefabs[p].prefabs.Length; i++) 
                    Instantiate(initialPrefabs[p].prefabs[i]).DontDestroyOnLoad(true);
        }
        #endregion


        void Update () {
            pauseManager.Update();
        }
        
        
        #region SCENE_ARCHITECTURE
        public static bool IsMainMenuScene(string scene) { return scene == mainMenuScene; }  
        public static bool isInMainMenuScene { get { return IsMainMenuScene(SceneLoading.activeScene); } }        
        
        public static event Action onLoadMainMenu, onExitMainMenu;
        static void OnSceneLoadStart (string targetScene, LoadSceneMode mode) {
            if (mode == LoadSceneMode.Additive)
                return;
            
            // if we're going into the main menu scene
            if (IsMainMenuScene(targetScene)) {
                
                if (onLoadMainMenu != null)
                    onLoadMainMenu();
            
                DestroyPlayer();
            }
            else {

                // build player if it doestn exist
                BuildPlayer();
                // going into "game mode" from teh main menu
                if (isInMainMenuScene) {
                    // Debug.Log("Exiting Main Menu Scene Event");
                    if (onExitMainMenu != null)
                        onExitMainMenu();
                }
            }    
        }

        public static event Action onNewGameStart;        
        public static bool startingNewGame;

        [Command("newgame", "Starts a new game", "Game", false)]
        public static void StartNewGame (bool overrideConfirm=false) {
            
            if (overrideConfirm || isInMainMenuScene) {
                _StartNewGame();
                return;
            } 
            UIEvents.ShowConfirmationPopup("Are You Sure You Want To Start A New Game?\nAny Unsaved Progress Will Be Lost!", _StartNewGame);
        }
        static void _StartNewGame () {
            startingNewGame = true;
            
            DestroyPlayer();

            // Debug.Log("On New Game Start Event");
            if (onNewGameStart != null) 
                onNewGameStart();

            // Debug.Log("Loading New Game Scene");
            DynamicObjectManager.MovePlayer(settings.newGameLocation, true);
            startingNewGame = false;
        }


        [Command("quit", "Quit Application", "Game", false)]
        public static void QuitApplication (bool overrideConfirm=false) {

            if (overrideConfirm) {
                _QuitApplication();
                return;
            }

            string msg = "Are You Sure You Want To Quit To Desktop?";
            if (!isInMainMenuScene)
                msg += "\nAny Unsaved Progress Will Be Lost!";

            UIEvents.ShowConfirmationPopup(msg, _QuitApplication);
        }
        static void _QuitApplication () {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit ();
            #endif
        }

        public static bool isQuitting;
        void OnApplicationQuit () {
            isQuitting = true;
        }

        public const string mainMenuScene = "_MainMenuScene";

        [Command("quitmm", "Quit to main menu", "Game", true)]
        public static void QuitToMainMenu (bool overrideConfirm=false) {
            if (overrideConfirm) {
                _QuitToMainMenu();
                return;
            }
            UIEvents.ShowConfirmationPopup("Are You Sure You Want To Quit To Main Menu?\nAny Unsaved Progress Will Be Lost!", _QuitToMainMenu);
        }
        static void _QuitToMainMenu () {
            // load the additive main menu scenes after we load the main menu scene
            Action<string, LoadSceneMode> onSceneLoaded = (s, m) => LoadAdditiveMainMenuScenes();
            SceneLoading.LoadSceneAsync (mainMenuScene, null, onSceneLoaded, LoadSceneMode.Single, false);
        }
        static void LoadAdditiveMainMenuScenes () {
            for (int i = 0; i < settings.mmSceneNames.Length; i++) 
                SceneLoading.LoadSceneAsync (settings.mmSceneNames[i], null, null, LoadSceneMode.Additive, false);
        }


        [Command("printscenes", "Print all the scenes in the game", "Game", false)]
        static string PrintScenes () {
            return string.Join("\n", GetAllScenes());
        }
        
        public static string[] GetAllScenes () {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            string[] allScenes = new string[sceneCount];
            for (int i = 0; i < sceneCount; i++)
                allScenes[i] = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            return allScenes;
        }
        #endregion


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
        public const string playerTag = "Player";
        public static bool playerExists { get { return DynamicObject.playerObject != null; } }
        public static DynamicObject player {
            get {
                if (!playerExists) 
                    Debug.LogError("Player not instantiated!!!");
                return DynamicObject.playerObject;
            }
        }
        static Camera _playerCamera;
        public static Camera playerCamera { get { return _playerCamera; } } 
        
        public static event Action onPlayerCreate, onPlayerDestroy;
        static void BuildPlayer () {
            if (playerExists) 
                return;

            // Debug.Log("Building Non Existant Player");
            GameObject.Instantiate(GameManagerSettings.instance.playerPrefab);
            _playerCamera = player.GetComponentInChildren<Camera>();

            Type[] camScripts = SystemTools.FindAllDerivedTypes(typeof(CameraScript));
            for (int i = 0; i < camScripts.Length; i++) 
                _playerCamera.gameObject.AddComponent(camScripts[i]);
        
            // Debug.Log("On Player Create Event");
            if (onPlayerCreate != null) 
                onPlayerCreate();
        }
        static void DestroyPlayer () {
            if (!playerExists)
                return;

            // Debug.Log("Destroying Player");
            GameObject.Destroy(player.gameObject);
            DynamicObject.playerObject = null;
            _playerCamera = null;

            
            // Debug.Log("On Player Destroy Event");
            if (onPlayerDestroy != null) 
                onPlayerDestroy();  
        }
        #endregion


        public static void GetSpawnOptionsForObject (DynamicObject obj, out bool ground, out bool navigate, out bool unIntersect) {
            // jsut true if player object has colliders...
            ground = !obj.isPlayer;
            unIntersect = !obj.isPlayer;

            navigate = obj.GetObjectScript<NavMeshAgent>() != null;
        }

        
    }   
}