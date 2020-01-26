using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

namespace UnityTools.EditorTools
{
    [InitializeOnLoad] public static class ProjectBuilderEditor
    {        
        static ProjectBuilderEditor () {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }        
        
        static void OnToolbarGUI()
		{
            if (Application.isPlaying)
                return;

            if(GUILayout.Button(BuiltInIcons.GetIcon("BuildSettings.SelectedIcon", "Open Build Options"), ToolbarExtender.commandButtonStyle))
                ProjectBuilderWindow.OpenWindow();
		}
    }

    // custom class so it can be drawn as asset selector iwithin array
    [System.Serializable] public class SceneAssetArrayElement : NeatArrayElement { 
        [AutoBuildScene] public SceneAsset element; 
        public SceneAssetArrayElement (SceneAsset element) {
            this.element = element;
        }
    }

    [System.Serializable] public class SceneAssetArray : NeatArrayWrapper<SceneAssetArrayElement> { 
        public SceneAssetArray() : base() { } public SceneAssetArray(SceneAssetArrayElement[] list) : base(list) { }
    }

    [System.Serializable] public class ProjectBuilderWindow : EditorWindow
    {

        public static void OpenWindow () {
            EditorWindowTools.CenterWindow( EditorWindow.GetWindow<ProjectBuilderWindow>("Build", true) ).Init();
		}


        void Init () {
            company = PlayerSettings.companyName;
            productName = PlayerSettings.productName;

            List<SceneAsset> sceneAssets = AssetTools.FindAssetsByType<SceneAsset>(false, new AutoBuildSceneAttribute().OnAssetsFound);
            SceneAssetArrayElement[] arrayElements = new SceneAssetArrayElement[sceneAssets.Count];
            for (int i = 0; i < sceneAssets.Count; i++) {
                arrayElements[i] = new SceneAssetArrayElement(sceneAssets[i]);
            }
            scenes = new SceneAssetArray(arrayElements);
        }

        List<EditorBuildSettingsScene> BuildFinalScenes () {

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

            SerializedProperty scenes = scenesList;
            for (int i = 0; i < scenes.arraySize; i++) {
                SerializedProperty scene = scenes.GetArrayElementAtIndex(i).FindPropertyRelative(NeatArray.elementName);
                if (scene.objectReferenceValue != null) {
                    buildScenes.Add( new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(scene.objectReferenceValue), true) );
                }
            }

            return buildScenes;
        }

        [NeatArray] public SceneAssetArray scenes;
        SerializedObject windowSO;
        SerializedProperty scenesList { get { return windowSO.FindProperty("scenes").FindPropertyRelative("list"); } }
        Vector2 scrollPos;

        [NeatArray] [SerializeField] BuildTargetDefList buildTargets;
        [SerializeField] string company = null, productName = null;
        [SerializeField] ProjectBuilder.VersionUpdate versionUpdate = ProjectBuilder.VersionUpdate.None;
        [SerializeField] bool forceVersion = false;
        [SerializeField] Vector3Int forcedVersion = new Vector3Int(1,0,0);
        [SerializeField] bool devBuild = true;
        [SerializeField] bool connectProfiler = false;
        [SerializeField] bool allowDebugging = true;
        [SerializeField] bool headless = false;
        [SerializeField] string bundleID = null;


        void OnGUI () {

            if (windowSO == null) windowSO = new SerializedObject(this);
            
            GUITools.Space(2);
            if (GUILayout.Button("Start Build")) 
                ProjectBuilder.PerformBuild (BuildFinalScenes(), company, productName, buildTargets, versionUpdate, forceVersion, forcedVersion, devBuild, connectProfiler, allowDebugging, headless, bundleID);
            
            
            windowSO.Update();

            EditorGUILayout.PropertyField(windowSO.FindProperty("company"), true);
            EditorGUILayout.PropertyField(windowSO.FindProperty("productName"), true);
            EditorGUILayout.PropertyField(windowSO.FindProperty("buildTargets"), true);

            EditorGUILayout.PropertyField(windowSO.FindProperty("forceVersion"), true);
            if (windowSO.FindProperty("forceVersion").boolValue) {
                EditorGUILayout.PropertyField(windowSO.FindProperty("forcedVersion"), true);
            }
            else {
                EditorGUILayout.PropertyField(windowSO.FindProperty("versionUpdate"), true);
            }

            EditorGUILayout.PropertyField(windowSO.FindProperty("devBuild"), true);
            
            if (windowSO.FindProperty("devBuild").boolValue) {
                EditorGUILayout.PropertyField(windowSO.FindProperty("allowDebugging"), true);
                EditorGUILayout.PropertyField(windowSO.FindProperty("connectProfiler"), true);
            }
            else {
                EditorGUILayout.PropertyField(windowSO.FindProperty("headless"), true);
            }

            EditorGUILayout.PropertyField(windowSO.FindProperty("bundleID"), true);
            
            EditorGUILayout.LabelField("Scenes to build:", GUITools.boldLabel);
            
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.PropertyField(windowSO.FindProperty("scenes"), true);
            GUILayout.EndScrollView();
            
            windowSO.ApplyModifiedProperties();
        }
    }    
}
