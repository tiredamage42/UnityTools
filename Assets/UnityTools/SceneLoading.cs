using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.SceneManagement;

namespace UnityTools {

    public class SceneLoading 
    {

        static SceneLoading sceneLoadingObject = new SceneLoading();

        public static event Action<string> prepareForSceneLoad;
        public static event Action endSceneLoad;
        
        public static event Action<float> onSceneLoadUpdate;
        public static event Action<Scene> onSceneExit;

        static void PrepareForSceneLoad (string targetScene) {
            GameManager.PauseGame(sceneLoadingObject);
            
            if (prepareForSceneLoad != null) {
                prepareForSceneLoad(targetScene);
            }
        }   
        static void EndSceneLoad () {
            if (endSceneLoad != null) {
                endSceneLoad();
            }
            
            GameManager.UnpauseGame(sceneLoadingObject);
        }

        public static Scene currentScene { get { return SceneManager.GetActiveScene(); } }

        public static bool LoadSceneAsync (string scene, Action onSceneStartLoad, Action<Scene> onSceneLoaded) {
            PrepareForSceneLoad(scene);

            if (onSceneExit != null) onSceneExit(currentScene);
            
            AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
            if (operation != null) {
                if (onSceneStartLoad != null) 
                    onSceneStartLoad();
            
                UpdateManager.instance.StartCoroutine(_LoadSceneAsync(operation, scene, onSceneLoaded));
                return true;
            }
            else {
                EndSceneLoad();
                return false;
            }            
        }

        static IEnumerator _LoadSceneAsync(AsyncOperation operation, string scene, Action<Scene> onSceneLoaded)
        {
            // Debug.Log("Loading from scene: " + scene);
            
            operation.allowSceneActivation = false;

            float progress = 0;
            while (progress < 1f)
            {

                progress = Mathf.Clamp01(operation.progress / 0.9f);
                if (onSceneLoadUpdate != null) onSceneLoadUpdate(progress);

                // Debug.Log("Load Progress: " + progress);

                // text.text = "Loading... " + (int)(progress * 100f) + "%";

                yield return null;
            }

            operation.allowSceneActivation = true;

            // let the scene activate for frame
            yield return null;

            if (onSceneLoaded != null) onSceneLoaded(SceneManager.GetSceneByName(scene));

            EndSceneLoad();

            // Debug.Log("Done Loading");
        }
    }
}
