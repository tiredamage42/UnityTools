using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using UnityTools.EditorTools;
using UnityTools.Internal;

namespace UnityTools {
    /*
    lets us load a main menu (or initialization) scene
    when pressing play to keep our workflow "game-like"
    */
    
    [InitializeOnLoad] public class GameManagerEditor {

        public const string initSceneKey = "@@";
        public const string mmSceneKey = "##";
        
        static GameManagerEditor () {
            
            BuildSettings.AddBuildWindowIgnorePattern(GameManager.mainMenuScene);
            BuildSettings.AddBuildWindowIgnorePattern(initSceneKey);
            BuildSettings.AddBuildWindowIgnorePattern(mmSceneKey);

            
            UnityToolsEditor.AddProjectChangeListener(RefreshScenesList);

            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }        


        public static void RefreshScenesList () {

            GameManagerSettings settings = GameManagerSettings.instance;

            // dont update when in play mode or if our game settings object is missing
            if (Application.isPlaying || settings == null) return;

            // update the array of all game settings objects in the project
            SceneAsset[] allScenesInProject = AssetTools.FindAssetsByType<SceneAsset>(logToConsole: false).ToArray();

            SceneAsset mainScene = null;
            List<SceneAsset> initScenes = new List<SceneAsset>();
            List<SceneAsset> mmScenes = new List<SceneAsset>();

            for (int i = 0; i < allScenesInProject.Length; i++) {
                if (allScenesInProject[i].name == GameManager.mainMenuScene) {
                    mainScene = allScenesInProject[i];
                    continue;
                }
                if (allScenesInProject[i].name.StartsWith(initSceneKey)) {
                    initScenes.Add(allScenesInProject[i]);
                    continue;
                }
                if (allScenesInProject[i].name.StartsWith(mmSceneKey)) {
                    mmScenes.Add(allScenesInProject[i]);
                    continue;
                }
            }

            if (mainScene == null) {
                Debug.LogError("No Main Menu scene found named: " + GameManager.mainMenuScene);
            }
            else {
                settings.mainMenuScenePath = AssetDatabase.GetAssetPath(mainScene);
            }

            AddScenesToBuildSettings(settings.mainMenuScenePath, StoreSceneNames(initScenes, ref settings.initSceneNames), StoreSceneNames(mmScenes, ref settings.mmSceneNames));            
            EditorUtility.SetDirty(settings);
        }  

        static string[] StoreSceneNames (List<SceneAsset> scenes, ref string[] sceneNames) {
            int c = scenes.Count;
            sceneNames = new string[c];
            string[] paths = new string[c];
            for (int i = 0; i < c; i++) {
                string path = AssetDatabase.GetAssetPath(scenes[i]);
                paths[i] = path;
                sceneNames[i] = Path.GetFileNameWithoutExtension(path);
            }
            return paths;
        }

        static void AddScenesToBuildSettings (string mainMenuScenePath, string[] initPaths, string[] mmPaths) {
            
            List<EditorBuildSettingsScene> finalScenes = new List<EditorBuildSettingsScene>();
            finalScenes.Add(new EditorBuildSettingsScene(mainMenuScenePath, true));
            for (int i = 0; i < initPaths.Length; i++) finalScenes.Add(new EditorBuildSettingsScene(initPaths[i], true));
            for (int i = 0; i < mmPaths.Length; i++) finalScenes.Add(new EditorBuildSettingsScene(mmPaths[i], true));
            
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            // add all non initialization scenes that were already in the build settings...
            for (int i = 0; i < buildScenes.Length; i++) {
                string path = buildScenes[i].path;

                if (string.IsNullOrEmpty(path) || path.Contains(GameManager.mainMenuScene) || path.Contains(initSceneKey) || path.Contains(mmSceneKey))
                    continue;
                
                finalScenes.Add(buildScenes[i]);
            }
            
            EditorBuildSettings.scenes = finalScenes.ToArray();
        }

        /*
            load main menu scene when pressing play in editor
        */
        const char splitKey = '&';
        const string splitKeyS = "&";
        public static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!GameManagerSettings.loadMainMenuOnEditorPlay)
                return;
            
            // User pressed stop -- reload previous scene.
            if (state == PlayModeStateChange.EnteredEditMode) {
                if (GameManagerSettings.bypassMenuLoadOnEditorPlay) {
                    GameManagerSettings.bypassMenuLoadOnEditorPlay = false;
                    return;
                }

                string[] scenes = PreviousScene.Split(splitKey);
                for (int i = 0; i < scenes.Length; i++) {
                    try     { EditorSceneManager.OpenScene(scenes[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive); }
                    catch   { Debug.LogError("Error: scene not found: " + scenes[i]); }
                }
        
            }
            else if (state == PlayModeStateChange.ExitingEditMode) {
                if (GameManagerSettings.bypassMenuLoadOnEditorPlay)
                    return;
                
                // User pressed play -- autoload initials scene.
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    List<string> paths = new List<string>();
                    for (int i = 0; i < EditorSceneManager.sceneCount; i++) 
                        paths.Add(EditorSceneManager.GetSceneAt(i).path);

                    PreviousScene = string.Join(splitKeyS, paths);
                    
                    string mainMenuScenePath = GameManagerSettings.instance.mainMenuScenePath;
                    
                    try {
                        EditorSceneManager.OpenScene(mainMenuScenePath);
                    }
                    catch {
                        Debug.LogError("Error: scene not found: " + mainMenuScenePath);
                        EditorApplication.isPlaying = false;
                    }
                }
                else {
                    // User cancelled the save operation -- cancel play as well.
                    EditorApplication.isPlaying = false;
                }
            }
        }
    
        const string prevSceneKey = "GameManager.PreviousScenes";
        static string PreviousScene
        {
            get { return EditorPrefs.GetString(prevSceneKey, EditorSceneManager.GetActiveScene().path); }
            set { EditorPrefs.SetString(prevSceneKey, value); }
        }
    }

}
