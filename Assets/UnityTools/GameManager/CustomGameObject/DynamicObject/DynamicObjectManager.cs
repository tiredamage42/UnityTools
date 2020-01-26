
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

using System;

using UnityTools.Internal;
using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;

using UnityTools.DevConsole;

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

    public enum ObjectLoadState { Loaded, Unloaded, NotFound };

    public class DynamicObjectManager : InitializationSingleTon<DynamicObjectManager> 
    {

        const string PLAYER_ALIAS = "Player";

        static void LoadPredefinedAliases () {

            List<ObjectAliases> aliases = GameSettings.GetSettingsOfType<ObjectAliases>();
            for (int i = 0; i < aliases.Count; i++) {
                for (int j = 0; j < aliases[i].aliases.Length; j++) {
                    AddAliasDefenition(aliases[i].aliases[j]);
                }
            }
        }

        // player scene should always be empty, so it doesnt load an extra copy on scene load
        static void AddAliasDefenition (ObjectAlias alias) {

            string scene = null;
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            LocationKey key = alias.location.GetKey();

            if (key != null) {
                scene = key.scene;

                if (!string.IsNullOrEmpty(scene)) {
                    bool loaded = Locations.GetLocationTransform(key, out MiniTransform t);
                    if (loaded) {
                        pos = t.position;
                        rot = Quaternion.Euler(t.rotation);
                    }
                }
            }

            AddNewAliasedObject (alias.alias, alias.prefabRef, scene, pos, rot, true, out _);
            
            GetStateByAlias(alias.alias).isUntouched = true;
        }
                
                
        static List<string> scenesAlreadyLoaded = new List<string>();
        static Dictionary<string, DynamicObjectState> id2ObjectState = new Dictionary<string, DynamicObjectState>();
        static Dictionary<string, string> alias2ID = new Dictionary<string, string>();
        static Dictionary<string, string> id2Alias = new Dictionary<string, string>();
        
        
        [Command("objectaliases", "List all the object aliases available", "Object", true)]
        static string ListAliases () {
            return string.Join("\n", alias2ID.Keys);
        }
        public static bool IDHasAlias (string id, out string alias) {
            return id2Alias.TryGetValue(id, out alias);
        }


        [Command("moveplayer", "move player to a specified location alias", "Game", true)]
        public static void MovePlayer (string alias, bool forceReload) {
            MovePlayer (LocationAliases.GetLocationKey(alias), forceReload);
        }
        [Command("moveobject", "move object to a specified location alias", "Game", true)]
        public static void MoveObject (string key, string alias){
            MoveObject (key, LocationAliases.GetLocationKey(alias));
        }
        public static void MoveObject (DynamicObject key, string alias){
            MoveObject (key, LocationAliases.GetLocationKey(alias));
        }
        
        public static void MovePlayer (Location location, bool forceReload) {
            MovePlayer (location.GetKey(), forceReload);
        }
        public static void MoveObject (string key, Location location){
            MoveObject (key, location.GetKey());
        }
        public static void MoveObject (DynamicObject key, Location location){
            MoveObject (key, location.GetKey());
        }

        public static void MovePlayer (LocationKey lKey, bool forceReload) {
            if (lKey != null)
                MovePlayer(lKey.scene, lKey.name, forceReload);
        }
        public static void MoveObject (string key, LocationKey lKey){
            if (lKey != null)
                MoveObject(key, lKey.scene, lKey.name);
        }
        public static void MoveObject (DynamicObject key, LocationKey lKey){
            if (lKey != null)
                MoveObject(key, lKey.scene, lKey.name);
        }
        
        
        [Command("moveplayer2", "move player to a specified scene and location point", "Game", true)]
        public static void MovePlayer (string scene, string locationName, bool forceReload) {
            if (Locations.GetLocationTransform(scene, locationName, out MiniTransform target))
                MovePlayer(scene, target, forceReload);
        }
        [Command("moveobject2", "move object to a specified scene and location point", "Game", true)]
        public static void MoveObject (string key, string scene, string locationName){
            if (Locations.GetLocationTransform(scene, locationName, out MiniTransform target))
                MoveObject(key, scene, target);
        }
        public static void MoveObject (DynamicObject obj, string scene, string locationName){
            if (Locations.GetLocationTransform(scene, locationName, out MiniTransform target))
                MoveObject(obj, scene, target);
        }

        static bool CheckSceneName (string scene) {
            if (string.IsNullOrEmpty(scene)) {
                Debug.LogError("MoveObject: No Scene Specified");
                return false;
            }
            return true;
        }

        public static void MoveObject (string key, string scene, MiniTransform target) {
            if (!CheckSceneName (scene))
                return;
            
            ObjectLoadState state = GetObjectFromKey(key, out object obj);
            if (state == ObjectLoadState.NotFound)
                return;
            
            MoveObject (state, obj, scene, target);
        }

        static void MoveObject (DynamicObject dynamicObject, string scene, MiniTransform target) {
            if (!CheckSceneName (scene))
                return;

            if (dynamicObject == null)
                return;
            
            MoveObject (ObjectLoadState.Loaded, dynamicObject, scene, target);
        }

        static void MovePlayer ( string scene, MiniTransform target, bool forceReload) {
            if (!CheckSceneName (scene))
                return;
            
            if (forceReload || !GameManager.playerExists || !SceneLoading.currentLoadedScenes.Contains(scene))
                MovePlayerToUnloadedScene ( scene, target );

            MoveDynamicObjectToTransform(GameManager.player, target.position, target.rotation, false);
        }

        static void MovePlayerToUnloadedScene (string scene, MiniTransform target) {
            movingPlayer = true;
            movePlayerTarget = target;
            SceneLoading.LoadSceneAsync(scene, null, null, LoadSceneMode.Single, false);
        }
        static bool movingPlayer;
        static MiniTransform movePlayerTarget;
        
        static void MoveObject (ObjectLoadState state, object obj, string scene, MiniTransform target){
            
            bool sceneIsLoaded = SceneLoading.currentLoadedScenes.Contains(scene);
            if (state == ObjectLoadState.Loaded) {
            
                DynamicObject dynamicObject = (DynamicObject)obj;
                // moving loaded DO to a scene and position thats already loaded
                if (sceneIsLoaded) {
                    MoveDynamicObjectToTransform(dynamicObject, target.position, target.rotation, false);
                }
                // moving loaded DO to an unloaded scene/position
                else {
                    if (dynamicObject.isPlayer) {
                        MovePlayerToUnloadedScene ( scene, target );
                    }
                    else {
                        DynamicObjectState objState = GetStateByID(dynamicObject.GetID());
                        dynamicObject.AdjustState(objState, scene, target.position, target.rotation, false);
                        objState.loadedVersion = null;
                        // give to pool again
                        dynamicObject.gameObject.SetActive(false);
                    }
                }
            }
            else if (state == ObjectLoadState.Unloaded) {
                DynamicObjectState objState = (DynamicObjectState)obj;
                if (objState.id == alias2ID[PLAYER_ALIAS]) {
                    Debug.Log("Cant Move Player When Player Is Unloaded...");
                    return;
                }
                objState.scene = scene;
                objState.position = target.position;
                objState.rotation = target.rotation;
                objState.isUntouched = false;          

                //moving an unloaded object to a loaded scene
                if (sceneIsLoaded)
                    LoadNewDynamicObjectWithState(objState, false);
            }
        }


        static void MoveDynamicObjectToTransform (DynamicObject dynamicObject, Vector3 position, Vector3 rotation, bool preAdjusted) {
            
            if (preAdjusted) {
                dynamicObject.transform.WarpTo(position, Quaternion.Euler(rotation));
            }
            else {

                GameManager.GetSpawnOptionsForObject(dynamicObject, out bool ground, out bool navigate, out bool uncollide);

                Vector3 up;    
                position = PhysicsTools.GroundPosition(position, ground, navigate, out up);

                dynamicObject.transform.WarpTo(position, Quaternion.Euler(rotation));

                if (uncollide)
                    PhysicsTools.UnIntersectTransform (dynamicObject.transform, up);
            }
            
        }

        // set scene, position, rotation, and spawn options of state beforehand
        static void LoadNewDynamicObjectWithState (DynamicObjectState state, bool preAdjusted) {
            
            DynamicObject dynamicObject = GetAvailableInstance(state);

            MoveDynamicObjectToTransform(dynamicObject, state.position, state.rotation, preAdjusted);

            dynamicObject.Load(state);

            state.isUntouched = false;
        }

        public static ObjectLoadState GetObjectFromKey (string key, out object obj) {
            obj = null;
            if (!key.StartsWith("@") && !key.StartsWith("#")) 
                return ObjectLoadState.NotFound;
            
            if (key.StartsWith("@")) 
                return GetObjectByAlias(key.Substring(1), out obj);
            
            return GetObjectByGUID(key.Substring(1), out obj);
        }
        public static DynamicObject GetDynamicObjectFromKey (string key) {
            ObjectLoadState state = GetObjectFromKey(key, out object obj);
            if (state == ObjectLoadState.Loaded)
                return (DynamicObject)obj;
            return null;
        }

        public static string GetNewGUID () {
            string id = StringID.Generate();
            while (id2ObjectState.ContainsKey(id))
                id = StringID.Generate();
            return id;
        }


        static bool suppressStateRemovalOnDisable;
        // called when a dynamic object is disable outside of this system
        public static void RemoveObjectByAlias (string alias) {
            RemoveObjectByGUID(Alias2ID(alias));
        }
        public static void RemoveObjectByGUID(string id, bool disableObject=true, bool logErrorIfPermanent=true) {
            
            if (!suppressStateRemovalOnDisable) {
                DynamicObjectState state = GetStateByID (id, false);
                if (state != null) {
                    if (!state.permanent) {
                        id2ObjectState.Remove(id);
                        if (id2Alias.ContainsKey(id)) {

                            alias2ID.Remove(id2Alias[id]);
                            id2Alias.Remove(id);
                        }

                        if (disableObject) {
                            if (state.isLoaded) {
                                // will call this again, but our guid should be removed so
                                // this wont loop endlessly....
                                state.loadedVersion.gameObject.SetActive(false);
                            }
                        }
                    }
                    else {
                        if (logErrorIfPermanent) {
                            if (id2Alias.ContainsKey(id))
                                Debug.LogError("Cant Delete Object Aliased: '" + id2Alias[id] + "', it is marked permanent");
                            else 
                                Debug.LogError("Cant Delete Object ID: " + id + ", it is marked permanent");
                        }
                    }
                }
            }
        }


        public static string Alias2ID (string alias) {
            if (alias2ID.TryGetValue(alias, out string id))
                return id;
            Debug.LogError("No ID Found For Alias: '" + alias + "'");
            return null;
        }

        public static DynamicObjectState GetStateByID (string id, bool logError = true) {
            if (string.IsNullOrEmpty(id))
                return null;

            if (id2ObjectState.TryGetValue(id, out DynamicObjectState state)) 
                return state;
            
            if (logError)
                Debug.LogError("No State With GUIID: '" + id + "' found...");
            return null;
        }
        public static DynamicObjectState GetStateByAlias (string alias) {
            return GetStateByID (Alias2ID(alias));
        }

        // TODO: player guid ?
        public static ObjectLoadState GetObjectByGUID (string id, out object obj) {
            
            DynamicObjectState state = GetStateByID (id);
            if (state != null) {
                obj = state.isLoaded ? (object)state.loadedVersion : state;
                return state.isLoaded ? ObjectLoadState.Loaded : ObjectLoadState.Unloaded;
            }
            obj = null;
            return ObjectLoadState.NotFound;
        }
        public static ObjectLoadState GetObjectByAlias (string alias, out object obj) {
            return GetObjectByGUID (Alias2ID(alias), out obj);
        }
        
        public static ObjectLoadState AddNewAliasedObject (string alias, PrefabReference prefabRef, string scene, Vector3 pos, Quaternion rot, bool permanent, out object obj) {
        
            if (alias2ID.ContainsKey(alias)) {
                Debug.LogError("Object with alias: '" + alias + "' already exists!");
                obj = null;
                return ObjectLoadState.NotFound;
            }

            string id;
            ObjectLoadState loadState = AddNewObject (prefabRef, scene, pos, rot, permanent, out obj, out id);
            
            alias2ID.Add(alias, id);
            id2Alias.Add(id, alias);
            return loadState;
        }
            

        // todo figure out a way to call messages on all objects...
        // so we can get actorstate.GameValue("Health") for instance...
        public static ObjectLoadState AddNewObject (PrefabReference prefabRef, string scene, Vector3 pos, Quaternion rot, bool permanent, out object obj, out string id) {

            bool sceneIsLoaded = SceneLoading.currentLoadedScenes.Contains(scene);

            id = GetNewGUID ();

            DynamicObjectState objectState = new DynamicObjectState(id, scene, prefabRef, permanent);
            
            id2ObjectState.Add(id, objectState);

            if (sceneIsLoaded) {

                DynamicObject dynamicObject = GetAvailableInstance (prefabRef, pos, rot, id, false, false, false);
                
                MoveDynamicObjectToTransform(dynamicObject, pos, rot.eulerAngles, false);
                
                dynamicObject.AdjustState(objectState, scene, pos, rot.eulerAngles, false);
                
                obj = dynamicObject;
            }
            // add an unloaded representation, instantiate later when the scene is loaded
            else {

                if (!prefabRef.isEmpty) {
                    DynamicObject basePrefab = PrefabReferenceCollection.GetPrefabReference<DynamicObject>(prefabRef);            
                    obj = basePrefab.AdjustState(objectState, scene, pos, rot.eulerAngles, true);
                }
                else {
                    obj = objectState;
                }
            }
            return objectState.isLoaded ? ObjectLoadState.Loaded : ObjectLoadState.Unloaded;
        }

        public static DynamicObject GetAvailableInstance (DynamicObjectState state) {
            return GetAvailableInstance(state.prefabRef, state.position, Quaternion.Euler(state.rotation), state.id, false, false, false);
        }

        public static DynamicObject GetAvailableInstance (PrefabReference prefabRef, Vector3 position, Quaternion rotation, string gUID, bool createState, bool createID, bool permanentState) {
            return GetAvailableInstance(PrefabReferenceCollection.GetPrefabReference<DynamicObject>(prefabRef), position, rotation, gUID, createState, createID, permanentState);
        }

        public static T GetAvailableInstance<T> (PrefabReference prefabRef, Vector3 position, Quaternion rotation, string gUID, bool createState, bool createID, bool permanentState) where T : Component{
            return GetAvailableInstance<T>(PrefabReferenceCollection.GetPrefabReference<DynamicObject>(prefabRef), position, rotation, gUID, createState, createID, permanentState);
        }

        public static T GetAvailableInstance<T> (DynamicObject prefab, Vector3 position, Quaternion rotation, string gUID, bool createState, bool createID, bool permanentState) where T : Component{
            if (prefab == null)
                return null;
            if (prefab.GetComponent<T>() == null)
                return null;
            return GetAvailableInstance(prefab, position, rotation, gUID, createState, createID, permanentState).GetComponent<T>();
        }

        public static PrefabPool<DynamicObject> pool = new PrefabPool<DynamicObject>();
        
        public static DynamicObject GetAvailableInstance (DynamicObject prefab, Vector3 position, Quaternion rotation, string gUID, bool createState, bool createID, bool permanentState) {
            DynamicObject inst = pool.GetAvailable(prefab, null, true, position, rotation, DynamicObject.onPoolCreateAction, null);
            
            if (createID) {
                gUID = GetNewGUID();
            }

            inst.SetID(gUID);

            if (createState) {
                DynamicObjectState objectState = new DynamicObjectState(gUID, null, inst.prefabRef, permanentState);
                id2ObjectState.Add(gUID, objectState);
                inst.AdjustState(objectState, null, position, rotation.eulerAngles, false);
            }

            return inst;
        }


        const string LOADED_SCENES_KEY = "LOADED_SCENES_TRACKER";
        const string OBJECT_STATES_KEY = "OBJECT_STATES";
        const string ALIASES_KEY = "ALIASES";



        protected override void Awake() {
            base.Awake();
            if (!thisInstanceErrored) {
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneLoading.onSceneExit += OnSceneExit;
                SceneLoading.onSceneUnload += OnSceneUnload;
                
                GameState.onSaveGame += OnSaveGame;
                GameState.onGameLoaded += OnLoadGame;
                GameManager.onNewGameStart += OnNewGameStart;
            }
        }


        void GetObjectStatesInScene (string scene, bool isUnloading, bool justDisable) {

            // get all the loaded objects in the current scene, that are active and available
            // (for instance an equipped item is not considered available, that shold be saved by the inventory component...)
            List<DynamicObject> dynamicObjects = FilterObjectsForScene(scene, DynamicObject.GetInstancesAvailableForLoad());
        
            if (dynamicObjects.Count > 0) {

                // teh list of saved objects to populate
                
                for (int i = 0; i < dynamicObjects.Count; i++) {

                    DynamicObject dynamicObject = dynamicObjects[i];

                    if (!justDisable) {

                        DynamicObjectState state = GetStateByID(dynamicObject.GetID());
                        
                        if (state != null) {

                            dynamicObject.AdjustState(state, scene, dynamicObject.transform.position, dynamicObject.transform.rotation.eulerAngles, false);
                        
                            if (isUnloading)
                                state.loadedVersion = null;
                        }
                    }

                    // TODO: add prevent disable on scene transfer (e.g. companions following...)

                    // disabling the scene our object is in
                    // give to pool again (if disabling scene)
                    if (isUnloading)
                        dynamicObject.gameObject.SetActive(false);
                }
            }
        }

        void OnSaveGame (List<string> allActiveLoadedScenes) {

            for (int i = 0; i < allActiveLoadedScenes.Count; i++)
                GetObjectStatesInScene ( allActiveLoadedScenes[i], false, false );

            // handle player saving
            GameManager.player.AdjustState(GetStateByAlias(PLAYER_ALIAS), null, GameManager.player.transform.position, GameManager.player.transform.rotation.eulerAngles, false);
            
            GameState.gameSaveState.UpdateState (LOADED_SCENES_KEY, scenesAlreadyLoaded);
            GameState.gameSaveState.UpdateState (OBJECT_STATES_KEY, id2ObjectState);
            GameState.gameSaveState.UpdateState (ALIASES_KEY, alias2ID);
        }

        void OnNewGameStart () {
            ClearAll();
            LoadPredefinedAliases();
        }

        void ClearAll () {
            scenesAlreadyLoaded.Clear();
            id2ObjectState.Clear();
            alias2ID.Clear();
            id2Alias.Clear();
        }


        void OnSceneExit (List<string> allActiveLoadedScenes, string targetScene) {

            if (GameManager.isInMainMenuScene) 
                return;
            
            // save the objects in this scene if we're going to another one,
            // e.g we're going to an indoor area that's a different scene, then save the objects "outdoors"

            // jsut disable if we're starting an new game, or exiting from loading a save
            suppressStateRemovalOnDisable = true;

            for (int i = 0; i < allActiveLoadedScenes.Count; i++)
                GetObjectStatesInScene ( allActiveLoadedScenes[i], true, GameState.isLoadingSave || GameManager.startingNewGame || GameManager.IsMainMenuScene(targetScene) );

            suppressStateRemovalOnDisable = false;
        }
            
        void OnSceneUnload (string unloadedScene) {

            if (GameManager.isInMainMenuScene) return;
            
            // dont save if we're exiting scene from manual loading another scene
            if (GameState.isLoadingSave || GameManager.startingNewGame) return;

            // save the objects in this scene if we're unloading it,
            // e.g we're out of range of an open world "cell"
            suppressStateRemovalOnDisable = true;
            GetObjectStatesInScene ( unloadedScene, true, false );
            suppressStateRemovalOnDisable = false;
        }


        void OnLoadGame () {
            ClearAll();
            bool hasworldSaved = GameState.gameSaveState.ContainsKey(OBJECT_STATES_KEY);
            
            if (hasworldSaved) {
                scenesAlreadyLoaded = (List<string>)GameState.gameSaveState.Load(LOADED_SCENES_KEY);
                id2ObjectState = (Dictionary<string, DynamicObjectState>)GameState.gameSaveState.Load(OBJECT_STATES_KEY);
                alias2ID = (Dictionary<string, string>)GameState.gameSaveState.Load(ALIASES_KEY);
                
                foreach (var k in alias2ID.Keys) 
                    id2Alias.Add(alias2ID[k], k);
                
                DynamicObjectState savedPlayer = GetStateByAlias(PLAYER_ALIAS);
                GameManager.player.transform.WarpTo(savedPlayer.position, Quaternion.Euler(savedPlayer.rotation));
                GameManager.player.Load(savedPlayer);
            }
            else {
                Debug.LogError("No Game State Saved...");
            }
        }

        void OnSceneLoaded (Scene scene, LoadSceneMode mode)
        {
            string sceneName = scene.name;

            if (GameManager.IsMainMenuScene(sceneName))
                return;


            if (movingPlayer) {
                if (GameManager.playerExists) 
                    MoveDynamicObjectToTransform (GameManager.player, movePlayerTarget.position, movePlayerTarget.rotation, false);
                movingPlayer = false;
            }

            // tODO: objects that are pre placed already alias refed might be copy if 
            // existing state already has loaded object 
            // e.g. (maybe a companion traveled with player back to where it was originally placed in editor)

            // get all the active objects that are default in the current scene
            List<DynamicObject> editorPlacedObjects = FilterObjectsForScene(sceneName, DynamicObject.GetInstancesThatNeedID());
            
            // get reference to all states that were pre defined programatically for this scene... 
            // (before we add the states for the manually placed ones)
            List<DynamicObjectState> states = GetStatesForScene(sceneName);
            
            if (scenesAlreadyLoaded.Contains(sceneName)) {

                // add them to the pool for availablity
                for (int i = 0; i < editorPlacedObjects.Count; i++) {
                    // if it needs a guid it comes with the scene... so this shouldnt matter (maybe...)
                    editorPlacedObjects[i].gameObject.SetActive(false);
                }
            }

            // scene loaded for the first time
            // create object states and id's for default placed objects
            else {
                
                scenesAlreadyLoaded.Add(sceneName);

                // give id's and states for those default objects
                for (int i = 0; i < editorPlacedObjects.Count; i++) {

                    string id = null;
                    if (editorPlacedObjects[i].usesAlias) {
                        id = editorPlacedObjects[i].GetID();
                        if (!string.IsNullOrEmpty(id)) {
                            DynamicObjectState aliasedState = GetStateByID(id);

                            // if our aliased state is untouched then we use this object
                            if (aliasedState.isUntouched) {

                                // dotn load in this scene again, just in case we were trying to
                                if (states.Contains(aliasedState)) {

                                    states.Remove(aliasedState);
                                }
                                
                                editorPlacedObjects[i].AdjustState(aliasedState, sceneName, editorPlacedObjects[i].transform.position, editorPlacedObjects[i].transform.rotation.eulerAngles, false);

                                aliasedState.isUntouched = false;
                            }

                            // we've adjusted the state, disable this object and use the state
                            else {
                                editorPlacedObjects[i].gameObject.SetActive(false);
                            }

                            // if our aliased state scene is this one, 
                            continue;
                        }
                    }

                    id = GetNewGUID ();                    
                    id2ObjectState.Add(id, editorPlacedObjects[i].AdjustState(new DynamicObjectState(id, sceneName, editorPlacedObjects[i].prefabRef, false), sceneName, editorPlacedObjects[i].transform.position, editorPlacedObjects[i].transform.rotation.eulerAngles, false));
                    editorPlacedObjects[i].SetID(id);
                }
            }

            for (int i = 0; i < states.Count; i++) 
                LoadNewDynamicObjectWithState(states[i], !states[i].isUntouched);
            
        }

        List<DynamicObjectState> GetStatesForScene (string scene) {
            List<DynamicObjectState> states = new List<DynamicObjectState>();
            foreach (var state in id2ObjectState.Values) {
                if (state.scene == scene) 
                    states.Add(state);
            }
            return states;
        }



        static Func<string, List<DynamicObject>, List<DynamicObject>> filterObjectsForScene;
        public static void SetObjectsSceneFilter (Func<string, List<DynamicObject>, List<DynamicObject>> filterObjectsForScene) {
            DynamicObjectManager.filterObjectsForScene = filterObjectsForScene;
        }
        static List<DynamicObject> FilterObjectsForScene (string scene, List<DynamicObject> objects) {
            if (filterObjectsForScene == null) 
                return objects;

            return filterObjectsForScene(scene, objects);
        }
    }
}