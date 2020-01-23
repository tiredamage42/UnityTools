
using UnityEngine;
using UnityTools.GameSettingsSystem;
using UnityTools.EditorTools;
using UnityEditor;
using System;
namespace UnityTools {

    [System.Serializable] public struct PrefabReference {
        // TODO: make prefab select field.....
        [AssetSelection(typeof(PrefabReferenceCollection), true)] 
        public string collection;
        public string name;

        public bool isEmpty { get { return string.IsNullOrEmpty(collection) || string.IsNullOrEmpty(name); } }

        public PrefabReference (string collection, string name) {
            this.collection = collection;
            this.name = name;
        }
    }

    #if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(PrefabReference))]
    public class PrefabReferenceDrawer : PropertyDrawer {

        public static void DrawPrefabReference (PrefabReference reference, GUIContent label, Action<string> onCollectionPicked, Action<string> onPrefabPicked) {
            
            if (!string.IsNullOrEmpty(label.text)) {
                GUITools.Label(label, GUITools.black, GUITools.boldLabel);
            }

            AssetSelector.DrawName(typeof(PrefabReferenceCollection), reference.collection, new GUIContent("Collection"), null, onCollectionPicked );

            if (!string.IsNullOrEmpty(reference.collection)) {
                EditorGUILayout.BeginHorizontal();
                GUITools.Label(new GUIContent("Prefab"), GUITools.black, GUITools.label, GUILayout.Width(EditorGUIUtility.labelWidth));
                
                if (GUITools.Button(new GUIContent(reference.name), GUITools.white, GUITools.popup, GUITools.black)) {
                    
                    PrefabReferenceCollection refObject = GameSettings.GetSettings<PrefabReferenceCollection>(reference.collection);
                    if (refObject != null) {
                        GenericMenu menu = new GenericMenu();
                        for (int i = 0; i < refObject.prefabs.Length; i++) {
                            if (refObject.prefabs[i] != null) {
                                string name = refObject.prefabs[i].name;
                                menu.AddItem (new GUIContent(name), name == reference.name, () => onPrefabPicked(name));
                            }
                        }
                        menu.ShowAsContext();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            // return reference;
        }
       
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {


            pos.height = GUITools.singleLineHeight;            
            
            if (!string.IsNullOrEmpty(label.text)) {
                GUITools.Label(pos, label, GUITools.black, GUITools.boldLabel);
                pos.y += GUITools.singleLineHeight;
            }

            SerializedProperty objProp = prop.FindPropertyRelative("collection");
            EditorGUI.PropertyField(pos, objProp, new GUIContent("Collection"), true);
            pos.y += GUITools.singleLineHeight;
            
            if (!string.IsNullOrEmpty(objProp.stringValue)) {

                SerializedProperty nameProp = prop.FindPropertyRelative("name");

                GUITools.Label(new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, pos.height), new GUIContent("Prefab"), GUITools.black, GUITools.label);
                if (GUITools.Button(pos.x + EditorGUIUtility.labelWidth, pos.y, pos.width - EditorGUIUtility.labelWidth, pos.height, new GUIContent(nameProp.stringValue), GUITools.white, GUITools.popup, GUITools.black)) {
                    
                    PrefabReferenceCollection refObject = GameSettings.GetSettings<PrefabReferenceCollection>(objProp.stringValue);
                    if (refObject != null) {
                        GenericMenu menu = new GenericMenu();
                        
                        for (int i = 0; i < refObject.prefabs.Length; i++) {
                            if (refObject.prefabs[i] != null) {
                                string name = refObject.prefabs[i].name;
                                menu.AddItem (
                                    new GUIContent(name), name == nameProp.stringValue, 
                                    () => {
                                        nameProp.stringValue = name;
                                        nameProp.serializedObject.ApplyModifiedProperties();
                                    }
                                );
                            }
                        }
                        menu.ShowAsContext();
                    }
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {

            float h = GUITools.singleLineHeight;
            
            if (!string.IsNullOrEmpty(label.text)) 
                h += GUITools.singleLineHeight;
            
            if (!string.IsNullOrEmpty(prop.FindPropertyRelative("collection").stringValue)) 
                h += GUITools.singleLineHeight;
            
            return h;
             
        }
    }

    #endif
}