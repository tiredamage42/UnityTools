using System.Collections;
// using System.Collections.Generic;
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



        IEnumerator TogglePauseCoroutine () {
            bool newPauseState = !paused;

            // state considered paused immediately when toggled
            if (newPauseState) paused = !paused;
            
            if (onPauseRoutineStart != null) onPauseRoutineStart(newPauseState, settings.pauseRoutineDelay);
            yield return new WaitForSecondsRealtime(settings.pauseRoutineDelay);
            if (onPauseRoutineEnd != null) onPauseRoutineEnd(newPauseState, settings.pauseRoutineDelay);

            // state considered unpaused after routine ends
            if (!newPauseState) paused = !paused;
        }
            
        public static void TogglePause () {
            instance.StartCoroutine(instance.TogglePauseCoroutine());
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