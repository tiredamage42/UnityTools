
using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using UnityTools.EditorTools;

namespace UnityTools.Internal {
    
    [Serializable] public class LocationKey {
        [SceneName] public string scene;
        public string name;
    }

    #if UNITY_EDITOR
    
    [CustomPropertyDrawer(typeof(LocationKey))] class LocationKeyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            
            SerializedProperty sceneProp = prop.FindPropertyRelative("scene");
            SerializedProperty locationProp = prop.FindPropertyRelative("name");
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(pos, sceneProp, true);
            bool changedScene = EditorGUI.EndChangeCheck();

            string scene = sceneProp.stringValue;
            if (!string.IsNullOrEmpty(locationProp.stringValue)) {
                
                if (string.IsNullOrEmpty(scene)) {
                    locationProp.stringValue = string.Empty;
                }
                else {
                    if (changedScene) {
                        Dictionary<string, Dictionary<string, LocationDefenition>> locations = Locations.LoadAllLocations();
                        if (locations == null || !locations.ContainsKey(scene) || !locations[scene].ContainsKey(locationProp.stringValue)) {
                            locationProp.stringValue = string.Empty;
                        }
                    }
                }
            }
            
            pos.y += GUITools.singleLineHeight;
            DrawLocationSelect(pos, locationProp, scene);

            if (GUITools.IconButton(pos.x + (pos.width - GUITools.iconButtonWidth), pos.y, new GUIContent("G", "Go To Scene / Location"), GUITools.white)) 
                FastTravelToLocation(sceneProp.stringValue, locationProp.stringValue);
            
        }

        static void DrawLocationSelect (Rect pos, SerializedProperty prop, string scene) {
            
            EditorGUI.LabelField(new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, pos.height), new GUIContent(prop.displayName));
            
            if (GUITools.Button(pos.x + EditorGUIUtility.labelWidth, pos.y, pos.width - (EditorGUIUtility.labelWidth + GUITools.iconButtonWidth), pos.height, new GUIContent(!string.IsNullOrEmpty(prop.stringValue) ? prop.stringValue : "[ Null ]"), GUITools.popup)) {
                GenericMenu menu = new GenericMenu();
                string msg;
                List<string> locationNames = BuildLocationNames(scene, out msg);

                if (locationNames == null) {
                    menu.AddItem ( new GUIContent(msg), false, () => { } );
                }
                else {
                    for (int i =0 ; i < locationNames.Count; i++) {
                        string e = locationNames[i];
                        menu.AddItem (new GUIContent(e), e == prop.stringValue, 
                            () => {
                                prop.stringValue = e;
                                prop.serializedObject.ApplyModifiedProperties();
                            }
                        );
                    }
                }
                menu.ShowAsContext();  
            }
        }
        static List<string> BuildLocationNames(string scene, out string msg) {
            msg = null;
            Dictionary<string, Dictionary<string, LocationDefenition>> scene2Locations = Locations.LoadAllLocations();
            if (scene2Locations != null) {    
                if (scene2Locations.ContainsKey(scene)) {
                    List<string> r = new List<string>() { null };
                    r.AddRange(scene2Locations[scene].Keys);
                    return r;
                }
                else msg = "No Locations Specified For: '" + scene + "'";
            }
            else msg = "Locations File Doesnt Exist....";
            return null;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight * 2;//3f;
        }


        public static void FastTravelToLocation (string chosenScene, string chosenLocation) {
            if (Application.isPlaying) {
                DynamicObjectManager.MovePlayer(chosenScene, chosenLocation, false);
            }
            else {
                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                string scenePath = null;
                for (int i = 0; i < scenes.Length; i++) {
                    if (Path.GetFileNameWithoutExtension(scenes[i].path) == chosenScene) {
                        scenePath = scenes[i].path;
                    }
                }
                if (scenePath != null) {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                        if (Locations.GetLocationBounds(chosenScene, chosenLocation, out Bounds bounds)) 
                            SceneView.lastActiveSceneView.Frame(bounds, false);
                    }
                }
                else 
                    Debug.LogWarning("No Scene Found: " + chosenScene);
            }
        }
    }

#endif
}