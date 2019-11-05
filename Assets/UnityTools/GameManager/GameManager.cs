using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

        void Update () {
            CheckPauseObjectsForNulls();
        }

        
        public static int maxSaveSlots { get { return settings.maxSaveSlots; } }

        public static bool isQuitting;

        void OnApplicationQuit () {
            isQuitting = true;
        }


        void Start () {
            if (!thisInstanceErrored)
                SaveLoad.LoadSettingsOptions();

            #if UNITY_EDITOR
            string skipToScene = InitialSceneWorkflow.SkipToScene;
            if (!string.IsNullOrEmpty(skipToScene)) {
                StartCoroutine(SkipToScene(skipToScene));
            }
            #endif
        }
            
        #if UNITY_EDITOR
        IEnumerator SkipToScene(string scene) {
            yield return new WaitForSecondsRealtime(3);
            SceneLoading.LoadSceneAsync (scene, null, null);
        }
        #endif


        public static event Action onPrepareMainMenuLoad, onPrepareMainMenuExit;

        static void PrepareForSceneLoad (string targetScene) {
        
            if (targetScene == mainMenuScene) {
                
                if (onPrepareMainMenuLoad != null)
                    onPrepareMainMenuLoad();
                
                DestroyPlayer();
                
            }
            else {

                BuildPlayer();
                
                if (onPrepareMainMenuExit != null)
                    onPrepareMainMenuExit();
            } 
                
        }


         #region PAUSE_GAME
        public static event System.Action<bool, float> onPauseRoutineStart, onPauseRoutineEnd;
        public static bool isPaused { get { return pauseObjects.Count == 0; } }// instance.paused; } }
        // bool paused;
        // static bool inPauseRoutine, toggleBackAfterRoutine;

        // public static void PauseGame () {
        //     if (!isPaused) TogglePause();
        // }
        // public static void UnpauseGame () {
        //     if (isPaused) {
        //         if (!inPauseRoutine)
        //             TogglePause();
        //         else
        //             toggleBackAfterRoutine = true;
        //     }
        // }


        public static event Action<bool> onPause;
        static List<object> pauseObjects = new List<object>();

        static void CheckPauseObjectsForNulls () {

            bool wasPaused = pauseObjects.Count > 0;
            for (int i = pauseObjects.Count -1; i >= 0; i--) {
                if (pauseObjects[i] == null) {
                    pauseObjects.RemoveAt(i);
                }
            }
            if (wasPaused && pauseObjects.Count == 0) {
                if (onPause != null) onPause(false);
            }
        }


        static void DoPause (object pauseObject) {
            bool wasEmpty = pauseObjects.Count == 0;
            pauseObjects.Add(pauseObject);
            if (wasEmpty) {
                if (onPause != null) onPause(true);
            }
        }
        static void DoUnpause (object pauseObject) {
            pauseObjects.Remove(pauseObject);
            if (pauseObjects.Count == 0) {
                if (onPause != null) onPause(false);
            }
        }

        public static void PauseGame (object pauseObject) {
            if (pauseObject == null) return;
            if (!pauseObjects.Contains(pauseObject)) 
                DoPause(pauseObject);
        }

        public static void UnpauseGame (object pauseObject) {
            if (pauseObject == null) return;
            if (pauseObjects.Contains(pauseObject)) 
                DoUnpause(pauseObject);
        }

        public static void TogglePase (object pauseObject) {
            if (pauseObject == null) return;
            if (pauseObjects.Contains(pauseObject)) 
                DoUnpause(pauseObject);
            else 
                DoPause(pauseObject);
        }

        



        // IEnumerator TogglePauseCoroutine (bool newPauseState) {
        //     // bool newPauseState = !paused;
        //     inPauseRoutine = true;

        //     // state considered paused immediately when toggled
        //     if (newPauseState) paused = !paused;
            
        //     if (onPauseRoutineStart != null) onPauseRoutineStart(newPauseState, settings.pauseRoutineDelay);
        //     yield return new WaitForSecondsRealtime(settings.pauseRoutineDelay);
        //     if (onPauseRoutineEnd != null) onPauseRoutineEnd(newPauseState, settings.pauseRoutineDelay);

        //     // state considered unpaused after routine ends
        //     if (!newPauseState) paused = !paused;
        //     inPauseRoutine = false;

        //     if (toggleBackAfterRoutine && paused) {
        //         toggleBackAfterRoutine = false;
        //         TogglePause(false);
        //     }
        // }

        
            
        // public static void TogglePause (bool newPauseState) {

        //     instance.StartCoroutine(instance.TogglePauseCoroutine(newPauseState));
        // }
        #endregion




        #region PLAYER
        // public static event Action onPlayerCreated, onPlayerDestroyed;
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
                // if (onPlayerCreated != null) onPlayerCreated();
            }
        }
        static void DestroyPlayer () {
            if (playerExists) {
                GameObject.Destroy(playerActor.gameObject);
                // if (onPlayerDestroyed != null) onPlayerDestroyed();
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