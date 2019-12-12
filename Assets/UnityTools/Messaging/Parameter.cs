
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityTools.EditorTools;

namespace UnityTools.Internal {
    [System.Serializable] public class Parameters : NeatArrayWrapper<Parameter> { }

    [System.Serializable] public class Parameter {
        
        public enum NativeType { Float, Int, Bool, String, Object };
        public float fValue;
        public int iValue;
        public bool bValue;
        public string sValue;
        public Object oValue;
        public NativeType type;

        public object GetParamObject() {
            switch (type) {
                case NativeType.Float: return fValue;
                case NativeType.Int: return iValue;
                case NativeType.Bool: return bValue;
                case NativeType.String: return sValue;
                case NativeType.Object: return oValue;
            }
            return null;
        }
    }


    #if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(Parameters))] 
    public class ParametersDrawer : NeatArrayAttributeDrawer
    {
        static float paramsLabelW;
        
        static GUIContent _paramsLabel;
        static GUIContent paramsLabel {
            get {
                if (_paramsLabel == null) {
                    _paramsLabel = new GUIContent("P: ", "Parameters");
                    paramsLabelW = GUITools.boldLabel.CalcSize(_paramsLabel).x;
                }
                return _paramsLabel;
            }
        }

        const float booleanParameterWidth = 20;
        
        public static void DrawFlat (Rect pos, SerializedProperty prop) {
            // the property we want to draw is the list child
            prop = prop.FindPropertyRelative(listName);

            float paramsSpace = pos.width - paramsLabelW;

            pos.width = paramsLabelW;
            GUITools.Label(pos, paramsLabel, GUITools.black, GUITools.boldLabel);
            pos.x += pos.width;

            int arraySize = prop.arraySize;

            int booleanParameters = 0;
            for (int i = 0; i < arraySize; i++) {
                if (ParamIsBool(prop.GetArrayElementAtIndex(i))) {
                    booleanParameters++;
                }
            }
            int nonBooleanParameters = arraySize - booleanParameters;

            paramsSpace = paramsSpace - (booleanParameterWidth * booleanParameters);

            float propW = paramsSpace * (1f/nonBooleanParameters);

            for (int i = 0; i < arraySize; i++) {
                SerializedProperty param = prop.GetArrayElementAtIndex(i);
                pos.width = ParamIsBool(param) ? booleanParameterWidth : propW;
                DrawParamGUIFlat(pos, param);
                pos.x += pos.width;
            }
        }


        static bool ParamIsBool (SerializedProperty paramProp) {
            return ((Parameter.NativeType)paramProp.FindPropertyRelative("type").enumValueIndex) == Parameter.NativeType.Bool;
        }


        static void DrawParamGUIFlat(Rect pos, SerializedProperty paramProp) {
            string valueName = null;
            switch ((Parameter.NativeType)paramProp.FindPropertyRelative("type").enumValueIndex) {
                case Parameter.NativeType.Float:  valueName = "fValue"; break;
                case Parameter.NativeType.Int:    valueName = "iValue"; break;
                case Parameter.NativeType.Bool:   valueName = "bValue"; break;
                case Parameter.NativeType.String: valueName = "sValue"; break;
                case Parameter.NativeType.Object: valueName = "oValue"; break;
            }
            EditorGUI.PropertyField(pos, paramProp.FindPropertyRelative(valueName), GUITools.noContent, true);

        }

        static void DrawParamGUI(Rect pos, SerializedProperty paramProp) {
            
            float typeWidth = 50;
            float hWidth = pos.width - typeWidth;

            pos.width = typeWidth;
            EditorGUI.PropertyField(pos, paramProp.FindPropertyRelative("type"), GUITools.noContent, true);
            pos.x += pos.width;

            pos.width = hWidth;
            DrawParamGUIFlat (pos, paramProp);                
        }

        
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {

            float indent1, indent2, indent2Width;
            bool displayedValue;
            StartArrayDraw ( pos, ref prop, ref label, out indent1, out indent2, out indent2Width, out displayedValue );

            DrawAddElement ( pos, prop, indent1, displayedValue );
            
            float xOffset = indent2 + GUITools.toolbarDividerSize;
            DrawArrayTitle ( pos, prop, label, xOffset );
            
            if (displayedValue) {
                Object baseObject = prop.serializedObject.targetObject;
                
                
                pos.x = xOffset;
                pos.y = pos.y + GUITools.singleLineHeight;
                pos.width = (indent2Width) - GUITools.toolbarDividerSize;

                int indexToDelete = -1;

                for (int i = 0; i < prop.arraySize; i++) {

                    if (GUITools.IconButton(indent1, pos.y, deleteContent, GUITools.red))
                        indexToDelete = i;
                    
                    DrawParamGUI(pos, prop.GetArrayElementAtIndex(i));
                    
                    pos.y += GUITools.singleLineHeight;
                }

                if (indexToDelete != -1) {
                    prop.DeleteArrayElementAtIndex(indexToDelete);                    
                }
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(Parameter))] 
    class ParameterDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight;
        }
    }
    
    #endif
}

