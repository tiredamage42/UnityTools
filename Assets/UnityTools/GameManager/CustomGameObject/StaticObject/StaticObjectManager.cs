


using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
namespace UnityTools {

    /*
        handle saving and loading of all static objects through application life
    */
    public class StaticObjectManager : InitializationSingleTon<StaticObjectManager> 
    {
        static Dictionary<string, Dictionary<string, ObjectState>> scene2States = new Dictionary<string, Dictionary<string, ObjectState>>();
        const string saveKey = "Scene2UnloadedStaticObjects";
        
        protected override void Awake() {
            base.Awake();
            if (!thisInstanceErrored) {
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneLoading.onSceneExit += OnSceneExit;
                SceneLoading.onSceneUnload += OnSceneUnload;

                GameState.onSaveGame += OnSaveGame;
                GameState.onGameLoaded += OnGameLoaded;
                GameManager.onNewGameStart += OnNewGameStart;
            }
        }

        void GetObjectStatesInScene (string scene) {
            // get all the loaded objects in the current scene, that are active and available
            if (!StaticObject.activeObjects.TryGetValue(scene, out List<StaticObject> staticObjectsList)) {
                Debug.LogError("No Static Objects Found For Scene: " + scene);
                return;
            }

            if (staticObjectsList.Count > 0) {
                // teh list of saved objects to populate
                Dictionary<string, ObjectState> states = new Dictionary<string, ObjectState>();
                for (int i = 0; i < staticObjectsList.Count; i++) {
                    states.Add(staticObjectsList[i].name, staticObjectsList[i].GetState());
                }
                // save the state by scene
                scene2States[scene] = states;
            }
        }

        void OnSaveGame (List<string> allActiveLoadedScenes) {

            for (int i = 0; i < allActiveLoadedScenes.Count; i++)
                GetObjectStatesInScene ( allActiveLoadedScenes[i] );

            GameState.gameSaveState.UpdateState (saveKey, scene2States);
        }

        void OnSceneExit (List<string> allActiveLoadedScenes, string targetScene) {

            if (GameManager.isInMainMenuScene) return;
            if (GameManager.IsMainMenuScene(targetScene)) return;
            // dont save if we're exiting scene from manual loading another scene
            if (GameState.isLoadingSave || GameManager.startingNewGame) return;
            
            // save the objects in this scene if we're going to another one,
            // e.g we're going to an indoor area that's a different scene, then save the objects "outdoors"
            
            for (int i = 0; i < allActiveLoadedScenes.Count; i++)
                GetObjectStatesInScene ( allActiveLoadedScenes[i] );
        }
            
        void OnSceneUnload (string unloadedScene) {

            if (GameManager.isInMainMenuScene) return;
            // dont save if we're exiting scene from manual loading another scene
            if (GameState.isLoadingSave || GameManager.startingNewGame) return;

            // save the objects in this scene if we're unloading it,
            // e.g we're out of range of an open world "cell"
            GetObjectStatesInScene ( unloadedScene );
        }

        void OnNewGameStart () {
            scene2States.Clear();
        }

        void OnGameLoaded () {
            scene2States.Clear();
            if (GameState.gameSaveState.ContainsKey(saveKey)) {
                scene2States = (Dictionary<string, Dictionary<string, ObjectState>>)GameState.gameSaveState.Load(saveKey);
            }
        }

        void OnSceneLoaded (Scene scene, LoadSceneMode mode)
        {
            string sceneName = scene.name;

            if (GameManager.IsMainMenuScene(sceneName))
                return;
            if (!scene2States.ContainsKey(sceneName)) 
                return;

            // get all the active objects that are default in the current scene
            if (!StaticObject.activeObjects.TryGetValue(sceneName, out List<StaticObject> staticObjectsList)) {
                Debug.LogError("No Static Objects Found For Scene: " + sceneName);
                return;
            }

            Dictionary<string, ObjectState> states = scene2States[sceneName];
            for (int i = 0; i < staticObjectsList.Count; i++) 
                if (states.TryGetValue(staticObjectsList[i].name, out ObjectState state)) 
                    staticObjectsList[i].Load(state);
        }
    }
}