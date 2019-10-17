using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.SceneManagement;

namespace UnityTools {

    public class SceneLoading 
    {
        public static event Action prepareForSceneLoad, endSceneLoad;
        public static event Action<float> onSceneLoadUpdate;
        public static event Action<Scene> onSceneExit;

        static void PrepareForSceneLoad () {
            // show loading progress bar
            if (prepareForSceneLoad != null) {
                prepareForSceneLoad();
            }
        }   
        static void EndSceneLoad () {
            // hide progress bar ui
            if (endSceneLoad != null) {
                endSceneLoad();
            }
        }

        public static void LoadSceneAsync (string scene, Action<Scene> onSceneLoaded) {

            PrepareForSceneLoad();

            if (onSceneExit != null) onSceneExit(SceneManager.GetActiveScene());
            
            AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
            if (operation != null) {
                UpdateManager.instance.StartCoroutine(_LoadSceneAsync(operation, scene, onSceneLoaded));
            }
            else {
                EndSceneLoad();
            }            
        }

        static IEnumerator _LoadSceneAsync(AsyncOperation operation, string scene, Action<Scene> onSceneLoaded)
        {
            Debug.Log("Loading from scene: " + scene);
            
            operation.allowSceneActivation = false;

            float progress = 0;
            while (progress < 1f)
            {

                progress = Mathf.Clamp01(operation.progress / 0.9f);
                if (onSceneLoadUpdate != null) onSceneLoadUpdate(progress);

                Debug.Log("Load Progress: " + progress);

                // text.text = "Loading... " + (int)(progress * 100f) + "%";

                yield return null;
            }

            operation.allowSceneActivation = true;

            // let the scene activate for frame
            yield return null;

            if (onSceneLoaded != null) onSceneLoaded(SceneManager.GetSceneByName(scene));

            EndSceneLoad();

            Debug.Log("Done Loading");
        }
    }
}
