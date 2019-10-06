// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using System;
using UnityTools.EditorTools;
using UnityEditor;
namespace UnityTools.Particles {
    [Serializable] public class ParticlesFX 
    {
        // [AssetSelection(typeof(ParticleSystem))] public ParticleSystem particle;
        [AssetSelection] public ParticleSystem particle;

        public float size = 1;
        public float playbackSpeed = 1;

    }


    
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ParticlesFX))]
    class ParticlesFXDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            EditorGUI.BeginProperty(pos, label, prop);
            
            GUITools.Box(new Rect(pos.x, pos.y, pos.width, EditorGUIUtility.singleLineHeight * 3 + GUITools.singleLineHeight), GUITools.shade);
            pos.height = EditorGUIUtility.singleLineHeight;
            
            EditorGUI.LabelField(pos, label, GUITools.boldLabel);
            pos.y += EditorGUIUtility.singleLineHeight;

            pos.x += GUITools.iconButtonWidth;
            pos.width -= GUITools.iconButtonWidth;
            
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("particle"), true);
            pos.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("size"), true);
            pos.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("playbackSpeed"), true);
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight * 4;
        }
    }

    #endif
}
