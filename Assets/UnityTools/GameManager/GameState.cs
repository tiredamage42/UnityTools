using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

using UnityTools.Internal;
using UnityTools.DevConsole;
namespace UnityTools {

    public class GameState {

        // main menu is loaded by starting the application, then we initialize the game manager
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnApplicationStartAfterMainMenuLoad()
        {
            LoadSettingsOptions();
            GameManager.onLoadMainMenu += ClearGameSaveState;
            GameManager.onNewGameStart += ClearGameSaveState;
            Application.quitting += SaveSettingsOptions;
        }



        /*
            save and load any gmae settings (video, audio, etc....)
            that are persistent regardless of the game save slot
        */

        
        public static SaveState settingsSaveState = new SaveState();
        static string settingsSavePath { get { return Application.persistentDataPath + "/GameSettingsOptions.save"; } }
        public static event Action onSettingsLoaded, onSettingsSave;

        // call when starting up game
        static void LoadSettingsOptions () {
            string savePath = settingsSavePath;
            if (!File.Exists(savePath)) 
                return;
            settingsSaveState.SetState( (Dictionary<string, object>)IOTools.LoadFromFile(savePath) );
            if (onSettingsLoaded != null) 
                onSettingsLoaded();
        }
        
        // call when we're done editng any settings, or when we're quittin ghte application
        public static void SaveSettingsOptions () {
            // let everyone know we're saving settings
            if (onSettingsSave != null) 
                onSettingsSave();

            IOTools.SaveToFile(settingsSaveState.state, settingsSavePath);
        }

        /*
            save and load actual game states
        */
        public static int maxSaveSlots { get { return GameManagerSettings.instance.maxSaveSlots; } }        
        
        public static SaveState gameSaveState = new SaveState();
        public static event Action<List<string>> onSaveGame;
        public static event Action onGameLoaded;

        public static bool isLoadingSave;

        const string infoExtension = "info";
        const string saveExtension = "save";
        static string GetGameStatePath (int slot, string extension) {
            return Application.persistentDataPath + "/SaveSate" + slot + "." + extension;
        }
        
        public static SaveStateInfo GetSaveDescription (int slot) {
            if (!SaveExists(slot))
                return null;
            return (SaveStateInfo)IOTools.LoadFromFile(GetGameStatePath(slot, infoExtension));
        }

        // call when going to main menu, or starting new game
        static void ClearGameSaveState () {
            gameSaveState.Clear();
        }
        public static bool SaveExists (int slot) {
            return File.Exists(GetGameStatePath(slot, saveExtension));
        }

        [Command("savegame", "Saves a game to the specified slot", "Game", true)]
        public static void SaveGame (int slot) {
            if (GameManager.isInMainMenuScene) {
                Debug.LogWarning("Cant save in main menu scene");
                return;
            }
            Debug.Log("Saving game to slot: " + slot);
            
            Debug.Log("On Save Game Event");
            // let everyone know we're saving
            if (onSaveGame != null) 
                onSaveGame(SceneLoading.currentLoadedScenes);


            Debug.Log("Saving Info To File");
            // keep track of the scene we were in when saving
            // save the description info
            IOTools.SaveToFile(new SaveStateInfo(SceneLoading.playerScene), GetGameStatePath(slot, infoExtension));
            
            Debug.Log("Saving Game State To File");
            // save the actual game state
            IOTools.SaveToFile(gameSaveState.state, GetGameStatePath(slot, saveExtension));
        }

        [Command("loadgame", "Loads a game from the specified slot", "Game", false)]
        public static void LoadGame (int slot, bool overrideConfirm=false) {

            if (overrideConfirm || GameManager.isInMainMenuScene) {
                _LoadGame(slot);
                return;
            }
            string msg = "Are You Sure You Want Load Save Slot " + slot + "?\nAny Unsaved Progress Will Be Lost!";
            UIEvents.ShowConfirmationPopup(msg, () => _LoadGame(slot) );
        }

        static void _LoadGame (int slot) {

            
            string savePath = GetGameStatePath(slot, saveExtension);
            if (!File.Exists(savePath)) {
                Debug.LogError("No Save File Found For Slot " + slot);
                return;
            }

            Debug.Log("Loading game slot " + slot);
            
            isLoadingSave = true;

            // try and load teh scene from the save state:

            // load the actual save state (if the scene starts loading)
            Action<LoadSceneMode> onSceneStartLoad = (m) => {
                
                Debug.Log("Loading State From File");
                gameSaveState.SetState( (Dictionary<string, object>)IOTools.LoadFromFile(savePath) );
                
                Debug.Log("On Load Game Event");
                if (onGameLoaded != null)
                    onGameLoaded();
            };

            // when the scene is done loading, set 'isLoadingSave' to false
            Action<string, LoadSceneMode> onSceneLoaded = (s, m) => isLoadingSave = false;
            
            Debug.Log("Getting Save Descriptor");
            SaveStateInfo descriptor = GetSaveDescription(slot);

            Debug.Log("Loading Saved Scene: " + descriptor.sceneName);
            // if theres a problem and this returns false, set 'isLoadingSave' to false
            if (!SceneLoading.LoadSceneAsync(descriptor.sceneName, onSceneStartLoad, onSceneLoaded, LoadSceneMode.Single, false)) 
                isLoadingSave = false;
        }
    }



    public class SaveState {

        public Dictionary<string, object> state = new Dictionary<string, object>();
        public void SetState (Dictionary<string, object> state) {
            Clear();
            this.state = state;
        }
        public void Clear () {
            state.Clear();
        }
        public void UpdateState (string key, object savedObject) {
            state[key] = savedObject;
        }
        public bool ContainsKey (string key) {
            return state.ContainsKey(key);
        }
        public object Load(string key) {
            if (state.TryGetValue(key, out object o)) 
                return o;
            return null;
        }        
    }
    /*
        saved description for a game save state slot, so we dont have to load the 
        whole state to get which scene was saved, when etc...
    */
    [Serializable] public class SaveStateInfo {
        public string sceneName;
        public string dateTimeSaved;
        // maybe a screenshot ?

        public SaveStateInfo(string sceneName) {
            this.sceneName = sceneName;
            dateTimeSaved = System.DateTime.UtcNow.ToString("HH:mm dd MMMM, yyyy");
        }
        public override string ToString() {
            return sceneName + "\n" + dateTimeSaved;
        }
    }

    /*
        structs that can be serialized to files
    */
    [Serializable] public struct sVector2 {
        public float x, y;
        public sVector2 (Vector2 v) { this.x = v.x; this.y = v.y; }
        public sVector2 (float x, float y) { this.x = x; this.y = y; }
        public static implicit operator Vector2 (sVector2 v) => new Vector2(v.x, v.y);
        public static implicit operator sVector2 (Vector2 v) => new sVector2(v);
    }
    [Serializable] public struct sVector3 {
        public float x, y, z;
        public sVector3 (Vector3 v) { this.x = v.x; this.y = v.y; this.z = v.z; }
        public static implicit operator Vector3 (sVector3 v) => new Vector3(v.x, v.y, v.z);
        public static implicit operator sVector3 (Vector3 v) => new sVector3(v);
    }
    [Serializable] public struct sQuaternion {
        public float x, y, z, w;
        public sQuaternion (Quaternion v) { this.x = v.x; this.y = v.y; this.z = v.z; this.w = v.w; }
        public static implicit operator Quaternion (sQuaternion v) => new Quaternion(v.x, v.y, v.z, v.w);
        public static implicit operator sQuaternion (Quaternion v) => new sQuaternion(v);
    }
}