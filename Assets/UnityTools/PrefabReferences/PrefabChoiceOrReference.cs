
using UnityEngine;
using UnityEditor;
using UnityTools.EditorTools;
namespace UnityTools {
    [System.Serializable] public class PrefabChoiceOrReference {
        public RandomPrefabChoice prefabChoice;
        public PrefabReference prefab;
        public PrefabReference GetPrefab (GameObject subject, GameObject target) {
            if (prefabChoice != null)
                return prefabChoice.GetRandomPrefabChoice(subject, target);
            return prefab;
        }
    }


    
    #if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(PrefabChoiceOrReference))]
    class PrefabChoiceOrReferenceDrawer : PropertyDrawer {
       
        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label) {

            pos.height = GUITools.singleLineHeight;            
            EditorGUI.LabelField(pos, property.displayName, GUITools.boldLabel);
            pos.y += GUITools.singleLineHeight;

            // pos.x += GUITools.iconButtonWidth;
            // pos.width -= GUITools.iconButtonWidth;

            SerializedProperty choiceProp = property.FindPropertyRelative("prefabChoice");
            EditorGUI.PropertyField(pos, choiceProp, GUITools.noContent, true);
            pos.y += GUITools.singleLineHeight;

            if (choiceProp.objectReferenceValue == null) {
                SerializedProperty prefabProp = property.FindPropertyRelative("prefab");
                EditorGUI.PropertyField(pos, prefabProp, GUITools.noContent, true);
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float h = GUITools.singleLineHeight * 2;
            SerializedProperty choiceProp = property.FindPropertyRelative("prefabChoice");
            if (choiceProp.objectReferenceValue == null) {
                h += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("prefab"), GUITools.noContent, true);
            }
            return h;
        }
    }

    #endif

}
