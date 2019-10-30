using UnityEngine;
using UnityEditor;
    
using UnityEditor.SceneManagement;
namespace UnityTools.EditorTools {

    // custom class so it can be drawn as asset selector iwithin array
    // [System.Serializable] public class SceneAssetArrayElement : NeatArrayElement { [AssetSelection(typeof(SceneAsset))] public SceneAsset element; }
    [System.Serializable] public class SceneAssetArrayElement : NeatArrayElement { [AssetSelection(typeof(SceneAsset))] public SceneAsset element; }
    
    [System.Serializable] public class SceneAssetArray : NeatArrayWrapper<SceneAssetArrayElement> {  }

    [System.Serializable] public class BuildWindow : EditorWindow
    {
        //% (ctrl on Windows, cmd on macOS), # (shift), & (alt).

        [MenuItem(ProjectTools.defaultMenuItemSection + "Unity Tools/Editor Tools/Build Window", false, ProjectTools.defaultMenuItemPriority)]
		static void OpenWindow () {
            EditorWindowTools.OpenWindowNextToInspector<BuildWindow>("Build");
		}

        [NeatArray] public SceneAssetArray scenes;
        SerializedObject windowSO;
        int topSpaces = 5;

        SerializedProperty scenesList { get { return windowSO.FindProperty("scenes").FindPropertyRelative("list"); } }
        void UpdateToReflectSettings() {

            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            int l = buildScenes.Length;

            SerializedProperty scenes = scenesList;
            for (int i = 0; i < l; i++) {
                if (i >= scenes.arraySize) 
                    scenes.InsertArrayElementAtIndex(scenes.arraySize);

                SerializedProperty scene = scenes.GetArrayElementAtIndex(i).FindPropertyRelative(NeatArray.elementName);
                scene.objectReferenceValue = AssetDatabase.LoadAssetAtPath(buildScenes[i].path, typeof(SceneAsset));
            }
        }

        void UpdateSettings() {
            SerializedProperty scenes = scenesList;
            
            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[scenes.arraySize];
            
            for (int i = 0; i < scenes.arraySize; i++) {
                SerializedProperty scene = scenes.GetArrayElementAtIndex(i).FindPropertyRelative(NeatArray.elementName);
                buildScenes[i] = new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(scene.objectReferenceValue), true);
            }

            EditorBuildSettings.scenes = buildScenes;
        }
        // public static bool LoadMasterOnPlay
        // {
        //     get { return EditorPrefs.GetBool(cEditorPrefLoadMasterOnPlay, false); }
        //     set { EditorPrefs.SetBool(cEditorPrefLoadMasterOnPlay, value); }
        // }
        // const string cEditorPrefLoadMasterOnPlay = "SceneAutoLoader.LoadMasterOnPlay";
        
        void OnGUI () {

            if (windowSO == null) windowSO = new SerializedObject(this);
            
            GUITools.Space(topSpaces);

            bool playInitialSceneFirst = InitialSceneWorkflow.LoadInitialOnPlay;

            GUI.backgroundColor = playInitialSceneFirst ? GUITools.blue : GUITools.white;
            if (GUILayout.Button("Play Initial Scene First In Editor")) {
                InitialSceneWorkflow.LoadInitialOnPlay = !playInitialSceneFirst;
            }
            GUI.backgroundColor = GUITools.white;

            if (GUILayout.Button("Open Master Scene In Editor")) {
                EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
            }

            string skipToScene = InitialSceneWorkflow.SkipToScene;

            string skipToScene2 = EditorGUILayout.TextField("Skip To Scene On Play", skipToScene);

            if (skipToScene2 != skipToScene) InitialSceneWorkflow.SkipToScene = skipToScene2;
            
            GUITools.Space();

            EditorGUILayout.LabelField("Scenes to build:", GUITools.boldLabel);

            UpdateToReflectSettings ();
            windowSO.ApplyModifiedProperties();

            EditorGUI.BeginChangeCheck();
            
            windowSO.Update();
            
            EditorGUILayout.PropertyField(windowSO.FindProperty("scenes"), true);
            
            if (EditorGUI.EndChangeCheck()) {
                UpdateSettings();
            }

            windowSO.ApplyModifiedProperties();
        }
    }    
}