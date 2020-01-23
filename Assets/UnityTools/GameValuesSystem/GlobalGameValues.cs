using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;

using System;
namespace UnityTools {

    [CreateAssetMenu(menuName="Unity Tools/Global Game Values Object", fileName="GlobalValuesObject")]
    public class GlobalGameValues : GameSettingsObject
    {
        static Dictionary<string, GameValue> globalValuesDict;

        public static void AddGlobalValue (GameValue gameValue) {
            InitializeDictionaryIfNull();
            globalValuesDict[gameValue.name] = new GameValue(gameValue);
        }

        static void InitializeDictionaryIfNull () {
            if (globalValuesDict == null) {
                globalValuesDict = new Dictionary<string, GameValue>();
                List<GlobalGameValues> allValuesObjs = GameSettings.GetSettingsOfType<GlobalGameValues>();

                for (int i = 0; i < allValuesObjs.Count; i++) {
                    for (int x = 0; x < allValuesObjs[i].gameValues.Length; x++) {
                        GameValue gv = new GameValue(allValuesObjs[i].gameValues[x]);
                        globalValuesDict[gv.name] = gv;
                    }
                }
            }
        }

        public static bool ValueExists (string name, out GameValue value) {
            InitializeDictionaryIfNull();
            return globalValuesDict.TryGetValue(name, out value);
        }
        public static bool ValueExists (string name) {
            return ValueExists(name, out _);
        }

        public static GameValue GetGameValue (string name) {
            InitializeDictionaryIfNull();
            
            GameValue value;
            if (ValueExists(name, out value))
                return value;
            
            Debug.LogWarning("Global Value: '" + name + "' does not exist");
            return null;
        }


        public static float GetGlobalBaseValue (string name) {
            GameValue gv = GetGameValue(name);
            if (gv == null)
                return 0;
            return gv.GetBaseValue();
        }
        
        public static float GetGlobalValue (string name) {
            GameValue gv = GetGameValue(name);
            if (gv == null)
                return 0;
            return gv.GetValue();
        }
        
        
        [NeatArray] public GameValueArray gameValues;


        #if UNITY_EDITOR
        public static void DrawGlobalValueSelector (Rect pos, SerializedProperty prop) {
            GlobalGameValueSelector.DrawName(pos, prop, GUITools.noContent);
        }
        #endif
    }

    #if UNITY_EDITOR
        
    

    public class GlobalGameValueSelector {

        public static void DrawName (SerializedProperty prop, GUIContent gui) {
            DrawName(prop.stringValue, gui, 
                (picked) => {
                    prop.stringValue = picked;
                    prop.serializedObject.ApplyModifiedProperties();    
                }
            );
        }

        public static void DrawName (Rect pos, SerializedProperty prop, GUIContent gui) {
            DrawName(pos, prop.stringValue, gui, 
                (picked) => {
                    prop.stringValue = picked;
                    prop.serializedObject.ApplyModifiedProperties();
                } 
            );
        }
        public static void DrawName (Rect pos, string current, GUIContent gui, Action<string> onPicked) {
            float x = pos.x;
            DrawLabel (ref x, pos.y, gui);
            if (DrawButton (x, pos, GetGUI(current)))
                BuildMenu (current, onPicked);
        }
        public static void DrawName (string current, GUIContent gui, Action<string> onPicked) {

            EditorGUILayout.BeginHorizontal();
            DrawLabel(gui);
            if (GUITools.Button(GetGUI(current), GUITools.popup))
                BuildMenu (current, onPicked);
            
            EditorGUILayout.EndHorizontal();
        }

        static GUIContent GetGUI (string current) { return new GUIContent(current != null ? current : nullString); }

        const string nullString = "[ Null ]";
        
        static List<string> BuildElements() {
            List<string> r = new List<string>();
            List<GlobalGameValues> allValuesObjs = GameSettings.GetSettingsOfType<GlobalGameValues>();
            for (int i = 0; i < allValuesObjs.Count; i++) {
                for (int x = 0; x < allValuesObjs[i].gameValues.Length; x++) {
                    r.Add(allValuesObjs[i].gameValues[x].name);
                }
            }
            return r;
        }


        static void BuildMenu (string current, Action<string> onPicked) {
            List<string> elements = BuildElements();
            GenericMenu menu = new GenericMenu();
            for (int i =0 ; i < elements.Count; i++) {
                string e = elements[i];
                menu.AddItem (new GUIContent(elements[i]), e == current, () => onPicked(e));
            }
            menu.ShowAsContext();
        }
        static void DrawLabel (ref float x, float y, GUIContent gui) {
            if (!string.IsNullOrEmpty(gui.text)) {
                EditorGUI.LabelField(new Rect(x, y, EditorGUIUtility.labelWidth, GUITools.singleLineHeight), gui);
                x += EditorGUIUtility.labelWidth;
            }
        }
        static void DrawLabel (GUIContent gui) {
            if (!string.IsNullOrEmpty(gui.text)) 
                EditorGUILayout.LabelField(gui, GUILayout.Width(EditorGUIUtility.labelWidth));
        }
        
        static bool DrawButton (float x, Rect pos, GUIContent gui) {
            return GUITools.Button(x, pos.y, pos.width - ((x - pos.x) + GUITools.iconButtonWidth), GUITools.singleLineHeight, gui, GUITools.popup);
        }   
    }
    #endif
}
