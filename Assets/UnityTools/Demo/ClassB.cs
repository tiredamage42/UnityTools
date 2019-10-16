using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityEditor;
using UnityTools.EditorTools;
using ONamepsac;
namespace BNamespace {
[System.Serializable] public class ClassB : BaseClassTest {
    public bool classBBool;
// }


// [CustomPropertyDrawer(typeof(ClassB))] class ClassBDrawer : PropertyDrawer {
    public override void DrawGUI(Rect position)
    {

        classBBool = EditorGUI.Toggle(position, "Class b bool", classBBool);
        
    }
    // public override float GetPropertyHeight()
    // {
    //     return GUITools.singleLineHeight;
    // }
}
}
