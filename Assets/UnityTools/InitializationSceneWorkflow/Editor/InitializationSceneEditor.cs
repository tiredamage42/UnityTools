

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using UnityTools.EditorTools;

namespace UnityTools.InitializationSceneWorkflow.Internal {

    /*
    lets us load a main menu (or initialization) scene
    when pressing play to keep our workflow "game-like"
    */
    
    [InitializeOnLoad] public class InitializationSceneEditor {
        static InitializationSceneEditor () {
            
            BuildSettings.AddBuildWindowIgnorePattern(InitializationScenes.mainInitializationScene);
            BuildSettings.AddBuildWindowIgnorePattern(InitializationScenes.initializationSceneKey);
            
            UnityToolsEditor.AddProjectChangeListener(RefreshScenesList);

            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }        


        public static void RefreshScenesList () {

            InitializationScenes instance = InitializationScenes.instance;

            // dont update when in play mode or if our game settings object is missing
            if (Application.isPlaying || instance == null) return;

            // update the array of all game settings objects in the project
            SceneAsset[] allScenesInProject = AssetTools.FindAssetsByType<SceneAsset>(logToConsole: false).ToArray();

            SceneAsset mainScene = null;
            List<SceneAsset> initializationScenes = new List<SceneAsset>();


            for (int i = 0; i < allScenesInProject.Length; i++) {
                if (allScenesInProject[i].name == InitializationScenes.mainInitializationScene) {
                    mainScene = allScenesInProject[i];
                    continue;
                }
                if (allScenesInProject[i].name.Contains(InitializationScenes.initializationSceneKey)) {
                    initializationScenes.Add(allScenesInProject[i]);
                }
            }

            if (mainScene == null) {
                Debug.LogError("No Main Initialization scene found named: " + InitializationScenes.mainInitializationScene);
            }
            else {
                instance.mainInitializationScenePath = AssetDatabase.GetAssetPath(mainScene);
            }

            int c = initializationScenes.Count;

            instance.initializationSceneNames = new string[c];
            string[] initializationScenePaths = new string[c];

            for (int i = 0; i < c; i++) {
                string path = AssetDatabase.GetAssetPath(initializationScenes[i]);
                initializationScenePaths[i] = path;
                instance.initializationSceneNames[i] = Path.GetFileNameWithoutExtension(path);
            }

            AddScenesToBuildSettings(instance, initializationScenePaths);

            EditorUtility.SetDirty(instance);
        }  

        static void AddScenesToBuildSettings (InitializationScenes instance, string[] initializationScenePaths) {
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            
            List<EditorBuildSettingsScene> finalScenes = new List<EditorBuildSettingsScene>();
            
            finalScenes.Add(new EditorBuildSettingsScene(instance.mainInitializationScenePath, true));
            
            for (int i = 0; i < initializationScenePaths.Length; i++) 
                finalScenes.Add(new EditorBuildSettingsScene(initializationScenePaths[i], true));
            
            
            // add all non initialization scenes that were already in the build settings...
            for (int i = 0; i < buildScenes.Length; i++) {
                string path = buildScenes[i].path;

                if (string.IsNullOrEmpty(path))
                    continue;
                    
                if (path.Contains(InitializationScenes.mainInitializationScene) || path.Contains(InitializationScenes.initializationSceneKey))
                    continue;
                
                finalScenes.Add(buildScenes[i]);
            }
            
            EditorBuildSettings.scenes = finalScenes.ToArray();
        }


        const char splitKey = '&';
        const string splitKeyS = "&";
        public static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!InitializationScenes.LoadInitialOnPlay)
                return;
            
            if (state == PlayModeStateChange.EnteredEditMode) {
                if (InitializationScenes.bypassInitializationSceneLoad) {
                    InitializationScenes.bypassInitializationSceneLoad = false;
                    return;
                }

                string prevScenesKey = PreviousScene;

                // User pressed stop -- reload previous scene.
                string[] scenes = new string[] { prevScenesKey };
                if (prevScenesKey.Contains(splitKeyS)) 
                    scenes = prevScenesKey.Split(splitKey);

                for (int i = 0; i < scenes.Length; i++) {
                    try {
                        EditorSceneManager.OpenScene(scenes[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
                    }
                    catch {
                        Debug.LogError(string.Format("error: scene not found: {0}", scenes[i]));
                    }
                }
        
            }
            else if (state == PlayModeStateChange.ExitingEditMode) {
                if (InitializationScenes.bypassInitializationSceneLoad)
                    return;
                
                // User pressed play -- autoload initials scene.

                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    string prevScenesKey = "";
                    for (int i = 0; i < EditorSceneManager.sceneCount; i++) {
                        prevScenesKey += EditorSceneManager.GetSceneAt(i).path + (i == EditorSceneManager.sceneCount -1 ? string.Empty : splitKeyS);
                    }

                    PreviousScene = prevScenesKey;
                    
                    try {
                        EditorSceneManager.OpenScene(InitializationScenes.instance.mainInitializationScenePath);
                    }
                    catch {
                        Debug.LogError(string.Format("error: scene not found: {0}", InitializationScenes.instance.mainInitializationScenePath));
                        EditorApplication.isPlaying = false;
                    }
                }
                else {
                    // User cancelled the save operation -- cancel play as well.
                    EditorApplication.isPlaying = false;
                }
            }
        }
    
        const string cEditorPrefPreviousScene = "InitialSceneWorkflow.PreviousScene";
        static string PreviousScene
        {
            get { return EditorPrefs.GetString(cEditorPrefPreviousScene, EditorSceneManager.GetActiveScene().path); }
            set { EditorPrefs.SetString(cEditorPrefPreviousScene, value); }
        }
    
    }
}