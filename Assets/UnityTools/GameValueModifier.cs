using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEditor;
using UnityTools.EditorTools;
namespace UnityTools {

    
    [System.Serializable] public class GameValueModifierArray : NeatArrayWrapper<GameValueModifier> { }
    [System.Serializable] public class GameValueModifierArray2D { 
        public bool displayed;
        [NeatArray] public GameValueModifierArray[] list; 
    }
    
    
    [System.Serializable] public class GameValueModifier {
        public bool isPermanent {
            get {
                return modifyValueComponent == GameValue.GameValueComponent.BaseValue
                    || modifyValueComponent == GameValue.GameValueComponent.BaseMinValue
                    || modifyValueComponent == GameValue.GameValueComponent.BaseMaxValue;
            }
        }

        [NeatArray] public Conditions conditions;
        
        public GameValueModifier (GameValue.GameValueComponent modifyValueComponent, ModifyBehavior modifyBehavior, float modifyValue) {
            this.modifyValueComponent = modifyValueComponent;
            this.modifyBehavior = modifyBehavior;
            this.modifyValue = modifyValue;
        }

        public GameValueModifier () {
            count = 1;
        }
        
        public GameValueModifier (GameValueModifier template, int count, int key, string description) {
            this.key = key;
            this.count = count;

            gameValueName = template.gameValueName;
            modifyValueComponent = template.modifyValueComponent;
            modifyBehavior = template.modifyBehavior;
            modifyValue = template.modifyValue;

            this.description = description;
        }


        public string description;
            
        [HideInInspector] public int key;
        [HideInInspector] public int count = 1;
        int getCount { get { return isStackable ? count : 1; } }
        public bool isStackable;
        public string gameValueName = "Game Value Name";
        public GameValue.GameValueComponent modifyValueComponent;
        
        public enum ModifyBehavior { Add, Multiply, Set };
        public ModifyBehavior modifyBehavior;
        public float modifyValue = 0;

        public float Modify(float baseValue) {
            if (modifyBehavior == ModifyBehavior.Set)
                return modifyValue;
            else if (modifyBehavior == ModifyBehavior.Add)
                return baseValue + (modifyValue * getCount);
            else if (modifyBehavior == ModifyBehavior.Multiply)
                return baseValue * (modifyValue * getCount);
            return baseValue;
        }

        // public string modifyBehaviorString {
        //     get {
        //         if (modifyBehavior == ModifyBehavior.Set)
        //             return "=";
        //         else if (modifyBehavior == ModifyBehavior.Add)
        //             return "+";
        //         else if (modifyBehavior == ModifyBehavior.Multiply) 
        //             return "x";
        //         return "";
        //     }
        // }
        // // public string gameMessageToShow {
        //     get {
        //         return gameValueName + " " + modifyBehaviorString + modifyValue.ToString(); 
        //     }
        // }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GameValueModifier))] public class GameValueModifierDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginProperty(position, label, property);

            int i = 0;
            float[] widths = new float[] { 60, 100, 90, 80, 60, 15, };

            float x = position.x;
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], singleLineHeight), property.FindPropertyRelative("modifyBehavior"), GUITools.noContent);
            x += widths[i++];
            
            GUITools.StringFieldWithDefault ( x, position. y, widths[i], singleLineHeight, property.FindPropertyRelative("gameValueName"), "Value Name");
            x+= widths[i++];

            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], singleLineHeight), property.FindPropertyRelative("modifyValueComponent"), GUITools.noContent);
            x+= widths[i++];
            
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], singleLineHeight), property.FindPropertyRelative("modifyValue"), GUITools.noContent);
            x+= widths[i++];
            
            EditorGUI.LabelField(new Rect(x, position.y, widths[i], singleLineHeight), "Stackable:");
            x+= widths[i++];
            
            EditorGUI.PropertyField(new Rect(x, position.y, widths[i], singleLineHeight), property.FindPropertyRelative("isStackable"), GUITools.noContent);
            
            // EditorGUI.indentLevel = oldIndent + 1;
            SerializedProperty conditionsProp = property.FindPropertyRelative("conditions");
            EditorGUI.PropertyField(new Rect(position.x, position.y + singleLineHeight, position.width, (EditorGUI.GetPropertyHeight(conditionsProp, true))), conditionsProp, new GUIContent("Conditions"));
            // EditorGUI.indentLevel = oldIndent;
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + (EditorGUI.GetPropertyHeight(property.FindPropertyRelative("conditions"), true));
        }
    }
#endif
}
