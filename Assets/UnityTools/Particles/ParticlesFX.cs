using UnityEngine;
using System;
using UnityTools.EditorTools;
using UnityEditor;
namespace UnityTools.Particles {
    [Serializable] public class ParticlesFX {
        [AssetSelection(typeof(ParticleSystem))] public ParticleSystem particle;
        public float size = 1;
        public float playbackSpeed = 1;
    }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ParticlesFX))] class ParticlesFXDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            
            GUITools.Box(new Rect(pos.x, pos.y, pos.width, EditorGUIUtility.singleLineHeight * 4.1f), GUITools.shade);
            
            pos.height = EditorGUIUtility.singleLineHeight;
            
            EditorGUI.LabelField(pos, label, GUITools.boldLabel);
            pos.y += EditorGUIUtility.singleLineHeight;

            pos.x += GUITools.iconButtonWidth * .5f;
            pos.width -= GUITools.iconButtonWidth * .5f;
            
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("particle"), true);
            pos.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("size"), true);
            pos.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("playbackSpeed"), true);
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return EditorGUIUtility.singleLineHeight * 4.25f;
        }
    }

    #endif
}
