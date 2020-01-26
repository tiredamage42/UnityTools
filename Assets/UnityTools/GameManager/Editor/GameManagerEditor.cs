
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using System.IO;
using System.Collections.Generic;

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

            ProjectBuilder.onRemoveAutoIncludedScenes += RemoveAutoIncludedScenes;
            ProjectBuilder.onAddAutoIncludedScenes += AddAutoIncludedScenes;

            UnityToolsEditor.AddProjectChangeListener(RefreshScenesList);

            EditorApplication.playModeStateChanged += OnPlayModeChanged;
			ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }        
        
        static void OnToolbarGUI()
		{
            if (Application.isPlaying)
                return;

			GUILayout.FlexibleSpace();

            if(GUILayout.Button(BuiltInIcons.GetIcon("PlayButtonProfile", "Play From Main Menu"), ToolbarExtender.commandButtonStyle))
			{
                loadMainMenuOnEditorPlay = true;
                EditorApplication.isPlaying = true;
			}
		}
        const string loadMenuOnPlayKey = "GameManager.LoadMainMenuOnPlay";
        public static bool loadMainMenuOnEditorPlay {
            get { return EditorPrefs.GetBool(loadMenuOnPlayKey, false); }
            set { EditorPrefs.SetBool(loadMenuOnPlayKey, value); }
        }

        static void RemoveAutoIncludedScenes (List<Object> scenes) {
            for (int i = scenes.Count - 1; i >= 0; i--) {
                if (scenes[i].name == GameManager.mainMenuScene) {
                    scenes.RemoveAt(i);
                    continue;
                }
                if (scenes[i].name.StartsWith(initSceneKey)) {
                    scenes.RemoveAt(i);
                    continue;
                }
                if (scenes[i].name.StartsWith(mmSceneKey)) {
                    scenes.RemoveAt(i);
                    continue;
                }
            }
        }
        static void AddAutoIncludedScenes (List<EditorBuildSettingsScene> scenes, List<SceneAsset> allScenes) {
            for (int i = 0; i < allScenes.Count; i++) {
                if (allScenes[i].name == GameManager.mainMenuScene) {
                    scenes.Insert(0, new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(allScenes[i]), true));
                    continue;
                }
                if (allScenes[i].name.StartsWith(initSceneKey)) {
                    scenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(allScenes[i]), true));
                    continue;
                }
                if (allScenes[i].name.StartsWith(mmSceneKey)) {
                    scenes.Add(new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(allScenes[i]), true));
                    continue;
                }
            }
        }

        public static void RefreshScenesList () {

            GameManagerSettings settings = GameManagerSettings.instance;

            // dont update when in play mode or if our game settings object is missing
            if (Application.isPlaying || settings == null) return;

            // update the array of all game settings objects in the project
            SceneAsset[] allScenesInProject = AssetTools.FindAssetsByType<SceneAsset>(log: false, null).ToArray();

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
            
            StoreSceneNames(initScenes, ref settings.initSceneNames);
            StoreSceneNames(mmScenes, ref settings.mmSceneNames);
            EditorUtility.SetDirty(settings);
        }  

        static void StoreSceneNames (List<SceneAsset> scenes, ref string[] sceneNames) {
            sceneNames = new string[scenes.Count];
            for (int i = 0; i < scenes.Count; i++) 
                sceneNames[i] = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(scenes[i]));
        }

        /*
            load main menu scene when pressing play in editor
        */
        const char splitKey = '&';
        const string splitKeyS = "&";
        public static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!loadMainMenuOnEditorPlay)
                return;
            
            
            if (state == PlayModeStateChange.ExitingEditMode) {
                
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
                        loadMainMenuOnEditorPlay = false;
                        EditorApplication.isPlaying = false;
                    }
                }
                else {
                    // User cancelled the save operation -- cancel play as well.
                    loadMainMenuOnEditorPlay = false;
                    EditorApplication.isPlaying = false;
                }
            }
            // User pressed stop -- reload previous scene.
            else if (state == PlayModeStateChange.EnteredEditMode) {
                loadMainMenuOnEditorPlay = false;
                    
                string[] scenes = PreviousScene.Split(splitKey);
                for (int i = 0; i < scenes.Length; i++) {
                    try     { EditorSceneManager.OpenScene(scenes[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive); }
                    catch   { Debug.LogError("Error: scene not found: " + scenes[i]); }
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
