
using System.Collections.Generic;
using UnityEngine;
using UnityTools.EditorTools;
using UnityEditor;
using System;
namespace UnityTools.Spawning {
    
    [Serializable] public class SpawnLocation {
        [SceneName] public string scene;
        public string spawnName;
    }

    #if UNITY_EDITOR
    
    [CustomPropertyDrawer(typeof(SpawnLocation))] class SpawnLocationDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            // GUITools.Box(new Rect(pos.x, pos.y, pos.width, GUITools.singleLineHeight * 3), new Color32(0, 0, 0, 32));

            // pos.height = GUITools.singleLineHeight;
            // GUITools.Label(pos, label, GUITools.black, GUITools.boldLabel);
            // pos.y += GUITools.singleLineHeight;
            
            SerializedProperty sceneProp = prop.FindPropertyRelative("scene");
            SerializedProperty spawnProp = prop.FindPropertyRelative("spawnName");
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(pos, sceneProp, true);
            bool changedScene = EditorGUI.EndChangeCheck();

            string scene = sceneProp.stringValue;
            if (!string.IsNullOrEmpty(spawnProp.stringValue)) {
                
                if (string.IsNullOrEmpty(scene)) {
                    spawnProp.stringValue = string.Empty;
                }
                else {
                    if (changedScene) {
                        Dictionary<string, Dictionary<string, SpawnPoint>> spawns = SpawnPoint.LoadAllSpawnPoints();
                        if (spawns == null || !spawns.ContainsKey(scene) || !spawns[scene].ContainsKey(spawnProp.stringValue)) {
                            spawnProp.stringValue = string.Empty;
                        }
                    }
                }
            }
            
            pos.y += GUITools.singleLineHeight;
            DrawSpawnSelect(pos, spawnProp, scene);
        }

        static void DrawSpawnSelect (Rect pos, SerializedProperty prop, string scene) {
            
            EditorGUI.LabelField(new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, pos.height), new GUIContent(prop.displayName));
            if (GUITools.Button(pos.x + EditorGUIUtility.labelWidth, pos.y, pos.width - (GUITools.iconButtonWidth + EditorGUIUtility.labelWidth), pos.height, new GUIContent(prop.stringValue != null ? prop.stringValue : "[ Null ]"), GUITools.popup)) {
                GenericMenu menu = new GenericMenu();
                string msg;
                List<string> spawnNames = BuildSpawnNames(scene, out msg);
                if (spawnNames == null) {
                    menu.AddItem ( new GUIContent(msg), false, () => { } );
                }
                else {
                    for (int i =0 ; i < spawnNames.Count; i++) {
                        string e = spawnNames[i];
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
        static List<string> BuildSpawnNames(string scene, out string msg) {
            msg = null;
            Dictionary<string, Dictionary<string, SpawnPoint>> scene2Spawns = SpawnPoint.LoadAllSpawnPoints();
            if (scene2Spawns != null) {    
                if (scene2Spawns.ContainsKey(scene)) {
                    List<string> r = new List<string>() { null };
                    r.AddRange(scene2Spawns[scene].Keys);
                    return r;
                }
                else msg = "No Spawn Points Specified For: '" + scene + "'";
            }
            else msg = "Spawn Point File Doesnt Exist....";
            return null;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight * 2;//3f;
        }
    }

#endif
}