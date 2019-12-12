
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityTools.Internal;
using UnityTools.Spawning;
namespace UnityTools {

    /*
        TODO:
        - What if object leaves scene on it's own? (gets blown away or thrown out of bounds maybe)
        - be able to move unloaded tracked object around between scenes manually
        - add size, based on prefab painter element probably...
    */

    /*
        handle saving and loading of all dynamic objects through application life
    */

    public enum TrackedObjectState {
        Loaded, Unloaded, NotFound
    }
    public class DynamicObjectManager : InitializationSingleTon<DynamicObjectManager> 
    {
        
        static Dictionary<string, DynamicObjectState> trackedStates = new Dictionary<string, DynamicObjectState>();



        // todo figure out a way to call messages on all objects...
        // so we can get actorstate.GameValue("Health") for instance...
        public static TrackedObjectState AddTrackedObject (
            string key, PrefabReference prefabRef,
            string scene, Vector3 position, Quaternion rotation, 
            SpawnOptions spawnOptions,
            out DynamicObject dynamicObject, out DynamicObjectState state
        ) {
            TrackedObjectState trackedObjState = TrackedObjectState.NotFound;
            if (trackedStates.ContainsKey(key)) {
                state = trackedStates[key];
                dynamicObject = state.isLoaded ? (DynamicObject)state.loadedVersion : null;
                trackedObjState = state.isLoaded ? TrackedObjectState.Loaded : TrackedObjectState.Unloaded;
                Debug.LogError("Already Tracking " + trackedObjState + " DynamicObject instance by key: " + key);
                return trackedObjState;
            }

            bool sceneIsLoaded = SceneLoading.currentLoadedScenes.Contains(scene);
            if (sceneIsLoaded) {

                Vector3 up;
                position = GameManager.GroundPosition(position, spawnOptions.ground, spawnOptions.navigate, out up);
                
                dynamicObject = DynamicObject.GetAvailableInstance (prefabRef, position, rotation);
                
                if (spawnOptions.uncollide) {
                    GameManager.UnIntersectTransform (dynamicObject.transform, up);
                }


                dynamicObject.SetTracked(key);
                state = dynamicObject.GetState();
            }
            // add an unloaded representation, instantiate later when the scene is loaded
            else {
                dynamicObject = null;
                DynamicObject basePrefab = PrefabReferenceCollection.GetPrefabReference<DynamicObject>(prefabRef);            
                state = basePrefab.GetState(spawnOptions, position, rotation);

                // set scene...?

                // add to worldsave objects
                if (scene2States.ContainsKey(scene)) {
                    scene2States[scene].Add(state);
                }
                // scene hasnt been loaded yet add to additive load dictionary, so we dont override default 
                else {
                    scenesLoadObjectsAdditively.Add(scene);
                    scene2States[scene] = new List<DynamicObjectState>() { state };
                }
            }

            state.trackKey = key;
            trackedStates.Add(key, state);
            trackedObjState = state.isLoaded ? TrackedObjectState.Loaded : TrackedObjectState.Unloaded;
            return trackedObjState;
                
        }

        public static void RemoveTrackedObject (string key) {
            if (trackedStates.ContainsKey(key)) {
                DynamicObjectState state = trackedStates[key];
                foreach (var k in scene2States.Keys) {
                    scene2States[k].Remove(state);
                }
                trackedStates.Remove(key);
                if (state.isLoaded) {
                    state.loadedVersion = null;
                }
            }
        }

        public static TrackedObjectState GetTrackedObject (string key, out DynamicObject dynamicObject, out DynamicObjectState state) {
            if (trackedStates.ContainsKey(key)) {
                state = trackedStates[key];
                dynamicObject = state.isLoaded ? (DynamicObject)state.loadedVersion : null;
                return state.isLoaded ? TrackedObjectState.Loaded : TrackedObjectState.Unloaded;
            }
            dynamicObject = null;
            state = null;
            return TrackedObjectState.NotFound;
        }

        static List<string> scenesLoadObjectsAdditively = new List<string>();
        static Dictionary<string, List<DynamicObjectState>> scene2States = new Dictionary<string, List<DynamicObjectState>>();
        
        const string saveKey = "Scene2UnloadedObjects";
        const string saveKeyStaged = saveKey + ".Staged";

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

        void GetObjectStatesInScene (string scene, bool isUnloading) {
            // get all the loaded objects in the current scene, that are active and available
            // (for instance an equipped item is not considered available, that shold be saved by the inventory component...)
            List<DynamicObject> dynamicObjects = SceneLoading.FilterObjectsForScene(scene, DynamicObject.GetInstancesAvailableForLoad());
        
            if (dynamicObjects.Count > 0) {

                // teh list of saved objects to populate
                
                List<DynamicObjectState> states = new List<DynamicObjectState>();
                for (int i = 0; i < dynamicObjects.Count; i++) {
                    DynamicObject dynamicObject = dynamicObjects[i];

                    DynamicObjectState state = dynamicObject.GetState();
                    states.Add(state);
                    
                    state.trackKey = dynamicObject.trackKey;

                    if (dynamicObject.isTracked)
                        trackedStates[state.trackKey] = state;
                    
                    // disabling the scene our object is in
                    // give to pool again (if disabling scene)
                    if (isUnloading)
                        dynamicObject.gameObject.SetActive(false);
                }

                // save the state by scene
                scene2States[scene] = states;
            }
        }


        void OnSaveGame (List<string> allActiveLoadedScenes) {

            for (int i = 0; i < allActiveLoadedScenes.Count; i++)
                GetObjectStatesInScene ( allActiveLoadedScenes[i], false );

            SaveLoad.gameSaveState.UpdateSaveState (saveKey, scene2States);
            SaveLoad.gameSaveState.UpdateSaveState (saveKeyStaged, scenesLoadObjectsAdditively);
        }

        // TODO: clear tracked on new game

        
        void LoadObjectStates () {
            scene2States.Clear();
            scenesLoadObjectsAdditively.Clear();
            trackedStates.Clear();

            bool hasworldSaved = SaveLoad.gameSaveState.SaveStateContainsKey(saveKey);
            
            if (hasworldSaved) {
                scene2States = (Dictionary<string, List<DynamicObjectState>>)SaveLoad.gameSaveState.LoadSaveStateObject(saveKey);
                scenesLoadObjectsAdditively = (List<string>)SaveLoad.gameSaveState.LoadSaveStateObject(saveKeyStaged);

                foreach (var stateList in scene2States.Values) {
                    foreach (var state in stateList) {
                        if (state.isTracked) {
                            trackedStates.Add(state.trackKey, state);
                        }
                    }
                }
            }
        }

        void OnSceneExit (List<string> allActiveLoadedScenes) {

            if (GameManager.isInMainMenuScene) return;
            // dont save if we're exiting scene from manual loading another scene
            if (SaveLoad.isLoadingSaveSlot) return;

            // save the objects in this scene if we're going to another one,
            // e.g we're going to an indoor area that's a different scene, then save the objects "outdoors"
            
            for (int i = 0; i < allActiveLoadedScenes.Count; i++)
                GetObjectStatesInScene ( allActiveLoadedScenes[i], true );
        }
            
        void OnSceneUnload (string unloadedScene) {

            if (GameManager.isInMainMenuScene) return;
            // dont save if we're exiting scene from manual loading another scene
            if (SaveLoad.isLoadingSaveSlot) return;

            // save the objects in this scene if we're unloading it,
            // e.g we're out of range of an open world "cell"
            GetObjectStatesInScene ( unloadedScene, true );
        }

        DynamicObjectState GetStateMatch (DynamicObject dynamicObject, List<DynamicObjectState> states, ref HashSet<int> skipIndicies) {
            for (int x = 0; x < states.Count; x++) {
                if (skipIndicies.Contains(x))
                    continue;

                if (dynamicObject.MatchesState(states[x])) {
                    skipIndicies.Add(x);
                    return states[x];
                }
            }
            return null;
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


            bool isAdditiveScene = scenesLoadObjectsAdditively.Contains(sceneName);
            bool hasStates = scene2States.ContainsKey(sceneName);

            // get all the active objects that are default in the current scene
            // the ones not added to the pool yet...
            List<DynamicObject> defaultDynamicObjectsInScene = SceneLoading.FilterObjectsForScene(sceneName, DynamicObject.GetInstancesNotInPool());
            
            HashSet<int> skipIndicies = new HashSet<int>();
            
            List<DynamicObjectState> states = hasStates ? scene2States[sceneName] : null;
            
            if (!hasStates || isAdditiveScene) {
                for (int i = 0; i < defaultDynamicObjectsInScene.Count; i++) {
                    defaultDynamicObjectsInScene[i].AddInstanceToPool(false);
                }
            }
            else {
                // add them to the pool for availablity
                // only disable if wehave unloaded object representatiosn for this scene
                for (int i = 0; i < defaultDynamicObjectsInScene.Count; i++) {

                    bool disableWhenAddingToPool = false;
                    if (defaultDynamicObjectsInScene[i].IsAvailableForLoad()) {
                        // if we find a match in unloaded objects, just use this instance
                        // instead of disabling this one and asking pool for a new one
                        DynamicObjectState matchingUnloaded = GetStateMatch (defaultDynamicObjectsInScene[i], states, ref skipIndicies);
                        if (matchingUnloaded == null) {
                            disableWhenAddingToPool = true;
                        }
                        else {
                            defaultDynamicObjectsInScene[i].Load(matchingUnloaded);
                        }
                    }
                    defaultDynamicObjectsInScene[i].AddInstanceToPool(disableWhenAddingToPool);
                }
            }

            if (hasStates) {
                for (int i = 0; i < states.Count; i++) {
                    if (!skipIndicies.Contains(i)) {

                        if (states[i].spawnOptions != null) {
                            Vector3 up;
                            
                            states[i].position = GameManager.GroundPosition(states[i].position, states[i].spawnOptions.ground, states[i].spawnOptions.navigate, out up);

                            DynamicObject dynamicObject = DynamicObject.GetAvailableInstance(states[i]);
                            
                            if (states[i].spawnOptions.uncollide)
                                GameManager.UnIntersectTransform (dynamicObject.transform, up);
                            

                            dynamicObject.Load(states[i]);
                        }

                        else {
                            DynamicObject.GetAvailableInstance(states[i]).Load(states[i]);
                        }
                    }
                }
            }

            if (isAdditiveScene) 
                scenesLoadObjectsAdditively.Remove(sceneName);
        }
    }
}