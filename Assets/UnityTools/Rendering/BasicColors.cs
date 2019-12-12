using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityTools.EditorTools;


namespace UnityTools {
    public enum BasicColor {
        Black, Gray, White, Red, Green, Blue, Cyan, Magenta, Yellow, Orange, Purple
    };

    [Serializable] public class BasicColorDefs {
        public Color32 GetColor (BasicColor color) {
            return colors[(int)color];
        }
        public const int colorsCount = 11;
        public Color32[] colors = new Color32[] {
            Color.black, 
            Color.gray, 
            Color.white,

            Color.red, 
            Color.green, 
            Color.blue, 
            Color.cyan, 
            Color.magenta, 

            new Color32 (255,255,0,255),
            new Color32 (255,127,0,255),
            new Color32 (127,0,255,255),
        };
    }

    
    #if UNITY_EDITOR
    
    [CustomPropertyDrawer(typeof(BasicColorDefs))] 
    public class BasicColorDefsDrawer : PropertyDrawer
    {

        static GUIContent _isShownContent;
        protected static GUIContent isShownContent { get { 
            if (_isShownContent == null) _isShownContent = BuiltInIcons.GetIcon("animationvisibilitytoggleon", "Hide"); 
            return _isShownContent;
        } }
        static GUIContent _hiddenContent;
        protected static GUIContent hiddenContent { get { 
            if (_hiddenContent == null) _hiddenContent = BuiltInIcons.GetIcon("animationvisibilitytoggleoff", "Show"); 
            return _hiddenContent;
        } }

        protected const string listName = "colors";

        void MakeSureSizeIsOK (SerializedProperty prop) {
            
            if (prop.arraySize != BasicColorDefs.colorsCount) {
                if (prop.arraySize > BasicColorDefs.colorsCount) prop.ClearArray();
                int c = BasicColorDefs.colorsCount - prop.arraySize;
                for (int i = 0; i < c; i++) prop.InsertArrayElementAtIndex(prop.arraySize);
            }
        }

        protected bool DrawDisplayedToggle (Rect pos, SerializedProperty prop) {
            if (GUITools.IconButton(pos.x, pos.y, prop.isExpanded ? isShownContent : hiddenContent, GUITools.white)){
                prop.isExpanded = !prop.isExpanded;
            }
            return prop.isExpanded;
        }

        
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            int origIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            EditorGUI.BeginProperty(pos, label, prop);

            bool displayedValue = DrawDisplayedToggle ( pos, prop );
            
            // the property we want to draw is the list child
            prop = prop.FindPropertyRelative(listName);

            MakeSureSizeIsOK(prop);
            
            float xOffset = (pos.x + GUITools.iconButtonWidth) + GUITools.toolbarDividerSize;
            pos.x = xOffset;

            
            GUITools.Label(pos, label, GUITools.black, GUITools.boldLabel);
            
            if (displayedValue) {
                pos.y += GUITools.singleLineHeight;
                pos.width = pos.width - (GUITools.iconButtonWidth + GUITools.toolbarDividerSize) * 2;
                pos.height = EditorGUIUtility.singleLineHeight;

                for (int i = 0; i < BasicColorDefs.colorsCount; i++) {
                    EditorGUI.PropertyField(pos, prop.GetArrayElementAtIndex(i), new GUIContent(((BasicColor)i).ToString()), true);
                    pos.y += GUITools.singleLineHeight;
                }
            }
            EditorGUI.indentLevel = origIndentLevel;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return (GUITools.singleLineHeight * (prop.isExpanded ? BasicColorDefs.colorsCount + 1 : 1))  + GUITools.singleLineHeight * .25f;
        }
    }
    #endif

}


