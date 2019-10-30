using UnityEngine;
using System.Collections;

using UnityTools.GameSettingsSystem;
using UnityTools.Internal;
using System;

using System.Linq;
using System.Reflection;

using UnityTools.EditorTools;
namespace UnityTools {

    public class GameManager : Singleton<GameManager>
    {
        static GameManagerSettings _settings;
        static GameManagerSettings settings {
            get {
                if (_settings == null) _settings = GameSettings.GetSettings<GameManagerSettings>();
                return _settings;
            }
        }

        /*
            start and awake should only happen during the initial scene load
        */
 
        protected override void Awake() {
            base.Awake();
            if (!thisInstanceErrored) {
                SceneLoading.prepareForSceneLoad += PrepareForSceneLoad;
                SceneLoading.endSceneLoad += UnpauseGame;

                if (settings.actionsController != null) settings.actionsController.InitializeActionsInterface();

                // get all types of SaveLoadObjectSceneHandler's
                // teh scripts in charge of saving the save state for certain scene objects
                System.Type[] results = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(SaveLoadObjectSceneHandler<,,>)).ToArray();

                // add tehm to this gameObject
                for (int i = 0; i < results.Length; i++) {
                    gameObject.AddComponent(results[i]);
                }
            }
        }

        void Start () {
            if (!thisInstanceErrored)
                SaveLoad.LoadSettingsOptions();

            #if UNITY_EDITOR
            string skipToScene = InitialSceneWorkflow.SkipToScene;
            StartCoroutine(SkipToScene(skipToScene));
            #endif
        }
            
        #if UNITY_EDITOR
        IEnumerator SkipToScene(string scene) {
            yield return new WaitForSecondsRealtime(3);
            SceneLoading.LoadSceneAsync (scene, null, null);
        }
        #endif

        static void PrepareForSceneLoad (string targetScene) {
            PauseGame();

            if (targetScene == mainMenuScene) 
                DestroyPlayer();
            else 
                BuildPlayer();
        }


         #region PAUSE_GAME
        public static event System.Action<bool, float> onPauseRoutineStart, onPauseRoutineEnd;
        public static bool isPaused { get { return instance.paused; } }
        bool paused;
        static bool inPauseRoutine, toggleBackAfterRoutine;

        public static void PauseGame () {
            if (!isPaused) TogglePause();
        }
        public static void UnpauseGame () {
            if (isPaused) {
                if (!inPauseRoutine)
                    TogglePause();
                else
                    toggleBackAfterRoutine = true;
            }
        }

        IEnumerator TogglePauseCoroutine () {
            bool newPauseState = !paused;
            inPauseRoutine = true;

            // state considered paused immediately when toggled
            if (newPauseState) paused = !paused;
            
            if (onPauseRoutineStart != null) onPauseRoutineStart(newPauseState, settings.pauseRoutineDelay);
            yield return new WaitForSecondsRealtime(settings.pauseRoutineDelay);
            if (onPauseRoutineEnd != null) onPauseRoutineEnd(newPauseState, settings.pauseRoutineDelay);

            // state considered unpaused after routine ends
            if (!newPauseState) paused = !paused;
            inPauseRoutine = false;

            if (toggleBackAfterRoutine && paused) {
                toggleBackAfterRoutine = false;
                TogglePause();
            }
        }
            
        public static void TogglePause () {
            instance.StartCoroutine(instance.TogglePauseCoroutine());
        }
        #endregion


        #region PLAYER
        public static event Action onPlayerCreated, onPlayerDestroyed;
        public static bool playerExists { get { return Actor.playerActor != null; } }
        public static Actor playerActor {
            get {
                if (!playerExists) Debug.LogError("Player Actor not instantiated!!!");
                return Actor.playerActor;
            }
        }
        public static Camera playerCamera { get { return PlayerCamera.camera; } }
        static void BuildPlayer () {
            if (!playerExists) {
                GameObject.Instantiate(settings.playerPrefab);
                if (onPlayerCreated != null) onPlayerCreated();
            }
        }
        static void DestroyPlayer () {
            if (playerExists) {
                GameObject.Destroy(playerActor.gameObject);
                if (onPlayerDestroyed != null) onPlayerDestroyed();
            }
        }
        #endregion

        public static bool isInMainMenuScene { get { return SceneLoading.currentScene.name == mainMenuScene; } }        
        public const string mainMenuScene = "_MainMenuScene";        

        public static void StartNewGame () {
            DestroyPlayer();
            SaveLoad.ClearGameSaveState();
            SceneLoading.LoadSceneAsync (settings.newGameScene, null, null);
        }

        public static void QuitToMainMenu () {
            SaveLoad.ClearGameSaveState();
            SceneLoading.LoadSceneAsync (mainMenuScene, null, null);
        }

        public static void QuitApplication () {
            SaveLoad.SaveSettingsOptions();

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit ();
    #endif
        }
    }   
}