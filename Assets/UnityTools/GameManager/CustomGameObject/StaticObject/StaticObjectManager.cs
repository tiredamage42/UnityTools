


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
        

        // disabling souldnt be happening, since these saver loader objects are 
        // persistent throughout the game
        protected override void Awake() {
            base.Awake();
            if (!thisInstanceErrored) {
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneLoading.onSceneExit += OnSceneExit;
                SceneLoading.onSceneUnload += OnSceneUnload;
                SaveLoad.onSaveGame += OnSaveGame;
            }
        }

        void GetObjectStatesInScene (string scene) {
            // get all the loaded objects in the current scene, that are active and available
            List<StaticObject> staticObjectsList;
            if (StaticObject.activeObjects.TryGetValue(scene, out staticObjectsList)) {

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
            else {
                Debug.LogError("No Static Objects Found For Scene: " + scene);
            }
        }

        void OnSaveGame (List<string> allActiveLoadedScenes) {

            for (int i = 0; i < allActiveLoadedScenes.Count; i++)
                GetObjectStatesInScene ( allActiveLoadedScenes[i] );

            SaveLoad.gameSaveState.UpdateSaveState (saveKey, scene2States);
        }

        // TODO: clear tracked on new game
        
        void LoadObjectStates () {
            scene2States.Clear();
            if (SaveLoad.gameSaveState.SaveStateContainsKey(saveKey)) {
                scene2States = (Dictionary<string, Dictionary<string, ObjectState>>)SaveLoad.gameSaveState.LoadSaveStateObject(saveKey);
            }
        }

        void OnSceneExit (List<string> allActiveLoadedScenes) {

            if (GameManager.isInMainMenuScene) return;
            // dont save if we're exiting scene from manual loading another scene
            if (SaveLoad.isLoadingSaveSlot) return;

            // save the objects in this scene if we're going to another one,
            // e.g we're going to an indoor area that's a different scene, then save the objects "outdoors"
            
            for (int i = 0; i < allActiveLoadedScenes.Count; i++)
                GetObjectStatesInScene ( allActiveLoadedScenes[i] );
        }
            
        void OnSceneUnload (string unloadedScene) {

            if (GameManager.isInMainMenuScene) return;
            // dont save if we're exiting scene from manual loading another scene
            if (SaveLoad.isLoadingSaveSlot) return;

            // save the objects in this scene if we're unloading it,
            // e.g we're out of range of an open world "cell"
            GetObjectStatesInScene ( unloadedScene );
        }

        void OnSceneLoaded (Scene scene, LoadSceneMode mode)
        {
            string sceneName = scene.name;

            if (GameManager.IsMainMenuScene(sceneName))
                return;

            if (SaveLoad.isLoadingSaveSlot) {
                if (mode == LoadSceneMode.Single) {
                    Debug.LogError("LoadObjectStates: Loaded Game Scene: " + sceneName);
                    LoadObjectStates ();
                }
            }

            if (scene2States.ContainsKey(sceneName)) {
                // get all the active objects that are default in the current scene
                List<StaticObject> staticObjectsList;
                if (StaticObject.activeObjects.TryGetValue(sceneName, out staticObjectsList)) {
                    Dictionary<string, ObjectState> states = scene2States[sceneName];

                    for (int i = 0; i < staticObjectsList.Count; i++) {
                        ObjectState state;
                        if (states.TryGetValue(staticObjectsList[i].name, out state)) {
                            staticObjectsList[i].Load(state);
                        }
                    }
                }
                else {
                    Debug.LogError("No Static Objects Found For Scene: " + sceneName);
                }
            }
        }
    }
}