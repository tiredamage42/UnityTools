using UnityEngine;
using System.Collections;
using UnityTools.GameSettingsSystem;
using UnityTools.Internal;
using System;
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
 
        protected override void Awake() {
            base.Awake();
            if (!thisInstanceErrored) {
                SceneLoading.prepareForSceneLoad += PrepareForSceneLoad;
                SceneLoading.endSceneLoad += UnpauseGame;
            }
        }

        void Start () {
            SaveLoad.LoadSettingsOptions();
        }


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