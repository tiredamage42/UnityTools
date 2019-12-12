using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
namespace UnityTools.Internal {

    public class PauseManager 
    {
        public bool isPaused { get { return pauseObjects.Count != 0; } }
        public event Action<bool> onPause;
        List<object> pauseObjects = new List<object>();

        public void Update () {
            CheckPauseObjectsForNulls();
        }

        void CheckPauseObjectsForNulls () {

            bool wasPaused = pauseObjects.Count > 0;
            for (int i = pauseObjects.Count -1; i >= 0; i--) {
                if (pauseObjects[i] == null) {
                    pauseObjects.RemoveAt(i);
                }
            }
            if (wasPaused && pauseObjects.Count == 0) {
                if (onPause != null) 
                    onPause(false);
            }
        }


        void DoPause (object pauseObject) {
            pauseObjects.Add(pauseObject);
            if (pauseObjects.Count == 1) {
                if (onPause != null) 
                    onPause(true);
            }
        }
        void DoUnpause (object pauseObject) {
            pauseObjects.Remove(pauseObject);
            if (pauseObjects.Count == 0) {
                if (onPause != null) 
                    onPause(false);
            }
            // else {
            //     for (int i =0 ; i < pauseObjects.Count; i++) {
            //         Debug.Log(pauseObjects[i]);
            //     }
            // }
        }

        public void PauseGame (object pauseObject) {
            if (pauseObject == null) return;
            if (!pauseObjects.Contains(pauseObject)) {
                DoPause(pauseObject);
            }
        }

        public void UnpauseGame (object pauseObject) {
            if (pauseObject == null) return;
            if (pauseObjects.Contains(pauseObject)) {
                DoUnpause(pauseObject);
            }
        }

        public void TogglePase (object pauseObject) {
            if (pauseObject == null) return;
            if (pauseObjects.Contains(pauseObject)) 
                DoUnpause(pauseObject);
            else 
                DoPause(pauseObject);
        }        
        
    }
}
