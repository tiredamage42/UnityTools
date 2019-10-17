using System.Collections;
using UnityEngine;

using UnityTools.GameSettingsSystem;
using UnityTools.Internal;
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
        
        public static event System.Action<bool, float> onPauseRoutineStart, onPauseRoutineEnd;

        public static bool isPaused { get { return instance.paused; } }
        bool paused;

        static bool inPauseRoutine, toggleBackAfterRoutine;


        protected override void Awake() {
            base.Awake();

            if (!thisInstanceErrored) {
                SceneLoading.prepareForSceneLoad += PauseGame;
                SceneLoading.endSceneLoad += UnpauseGame;
            }
        }


        public static void PauseGame () {
            if (!isPaused) TogglePause();
        }
        public static void UnpauseGame () {
            if (isPaused) {
                if (!inPauseRoutine) {
                    TogglePause();
                }
                else {
                    toggleBackAfterRoutine = true;
                }
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

        public static bool SceneIsNonSaveable (string scene) {
            for (int i = 0; i < settings.nonSaveScenes.Length; i++) {
                if (settings.nonSaveScenes[i] == scene) {
                    return true;
                }
            }
            return false;
        }

        public static void QuitToMainMenu () {
            SceneLoading.LoadSceneAsync (settings.mainMenuScene, null);
        }

        public static void QuitApplication () {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit ();
    #endif
        }
    }   
}