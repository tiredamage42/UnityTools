using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using UnityEngine.SceneManagement;
          
namespace UnityTools {


    public class SaveLoad 
    {
        static Dictionary<string, object> saveState = new Dictionary<string, object>();
        
        public static void UpdateSaveState (string key, object savedObject) {
            saveState[key] = savedObject;
        }

        public static bool SaveStateContainsKey (string key) {
            return saveState.ContainsKey(key);
        }
        
        public static object LoadSavedObject(string key) {
            object o;
            if (saveState.TryGetValue(key, out o)) {
                return o;
            }
            return null;
        }

        const string savedSceneKey = "SAVELOAD.SAVEDSCENE";
        public static event Action<Scene> onSaveGame;
        public static bool isLoadingSaveSlot;

        
        static string GetSavePath (int slot) {
            return Application.persistentDataPath + "/SaveSate" + slot.ToString() + ".save";
        }

        // deletes save game file, so scene loads dont reload any old info
        public static void OnNewGameStarted (int slot) {
            saveState.Clear();
            string savePath = GetSavePath(slot);
            if (File.Exists(savePath)) 
                File.Delete(savePath);
        }

        public static bool SaveExists (int slot) {
            return File.Exists(GetSavePath(slot));
        }

        public static void SaveGame (int slot) {
            Debug.Log("Saving game");

            // keep track of the scene we were in when saving
            Scene currentScene = SceneManager.GetActiveScene();
            saveState[savedSceneKey] = currentScene.name;
            
            // let everyone know we're saving
            if (onSaveGame != null) onSaveGame(currentScene);

            SystemTools.SaveToFile(saveState, GetSavePath(slot));
        }
        
        public static void LoadGame (int slot) {

            string savePath = GetSavePath(slot);

            if (!File.Exists(savePath)) {
                Debug.LogError("No Save File Found");
                return;
            }

            Debug.Log("Starting Load");
            
            SceneLoading.PrepareForSceneLoad ();

            isLoadingSaveSlot = true;

            // load the actual save state
            saveState = (Dictionary<string, object>)SystemTools.LoadFromFile(savePath);
            
            string sceneFromSave = (string)saveState[savedSceneKey];
            
            SceneLoading.LoadSceneAsync(sceneFromSave, (s) => { isLoadingSaveSlot = false; });            
        }
    }
}
