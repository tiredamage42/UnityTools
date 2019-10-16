using System.Collections;
using System.Collections.Generic;
using UnityEngine;





using UnityEditor;
using UnityTools.EditorTools;

using ONamepsac;
namespace ANamespace {
    [System.Serializable] public class ClassA : BaseClassTest {
        public float classAFloat;

        public override void DrawGUI(Rect position)
        {
            classAFloat = EditorGUI.FloatField(position, "Class A Float", classAFloat);
            
        }
        // public override float GetPropertyHeight()
        // {
        //     return GUITools.singleLineHeight;
        // }
    }
// [CustomPropertyDrawer(typeof(ClassA), true)] class ClassADrawer : PropertyDrawer {
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         float singleLineHeight = EditorGUIUtility.singleLineHeight;
//         EditorGUI.BeginProperty(position, label, property);
//         // float classSelectorWidth = BaseClassTestDrawer.DrawClassSelector(new Rect(position.x, position.y, 0, singleLineHeight), property);
//         // EditorGUI.PropertyField(new Rect(position.x + classSelectorWidth, position.y, position.width - classSelectorWidth, singleLineHeight), property.FindPropertyRelative("classAFloat"));        
//         EditorGUI.PropertyField(position, property.FindPropertyRelative("classAFloat"));        
        
//         EditorGUI.EndProperty();
//     }
//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//     {
//         return GUITools.singleLineHeight;
//     }
// }
}
