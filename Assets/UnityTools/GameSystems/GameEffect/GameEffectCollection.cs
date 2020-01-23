using System;
using UnityEngine;
using UnityTools.EditorTools;
using UnityEditor;

namespace UnityTools {
    [Serializable] public class GameEffectList : NeatArrayWrapper<GameEffectItem> { }
    [Serializable] public class GameEffectItem {
        [AssetSelection(typeof(GameEffect))] public GameEffect effect;
        public float magnitude, duration;
        [NeatArray("Subjects: 'Caster', 'AffectedObject'")] public Conditions conditions;
    }

    [Serializable] public class EffectContext {
        public float radius;
        public bool checkLOSOnRadiusCast;
        public LayerMask mask;
        [TextArea] public string message;
        [NeatArray] public GameEffectList effects;
    }
        
    [CreateAssetMenu(menuName="Unity Tools/Game Effects/Game Effects Collection", fileName="GameEffectsCollection")]
    public class GameEffectCollection : ScriptableObject {
        public EffectContext cast, dispell;
        [TextArea] public string description;
        [NeatArray] public NeatStringList keywords;
        public bool HasKeyword (string keyWord) {
            return keywords.list.Contains(keyWord);
        }
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GameEffectItem))] class GameEffectItemDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            float w2 = 64;
            float w1 = pos.width - (w2 * 2);
            GUITools.Label(new Rect(pos.x, pos.y, w1, pos.height), new GUIContent("Effect"), GUITools.black, GUITools.boldLabel);
            GUITools.Label(new Rect(pos.x + w1, pos.y, w2, pos.height), new GUIContent("Magnitude"), GUITools.black, GUITools.boldLabel);
            GUITools.Label(new Rect(pos.x + w1 + w2, pos.y, w2, pos.height), new GUIContent("Duration"), GUITools.black, GUITools.boldLabel);
            EditorGUI.PropertyField(new Rect(pos.x, pos.y + GUITools.singleLineHeight, w1, pos.height), prop.FindPropertyRelative("effect"), GUITools.noContent, true);
            EditorGUI.PropertyField(new Rect(pos.x + w1, pos.y + GUITools.singleLineHeight, w2, pos.height), prop.FindPropertyRelative("magnitude"), GUITools.noContent, true);
            EditorGUI.PropertyField(new Rect(pos.x + w1 + w2, pos.y + GUITools.singleLineHeight, w2, pos.height), prop.FindPropertyRelative("duration"), GUITools.noContent, true);
            EditorGUI.PropertyField(new Rect(pos.x, pos.y + GUITools.singleLineHeight * 2, pos.width, pos.height), prop.FindPropertyRelative("conditions"), true);   
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight * 2 + EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("conditions"), true);
        }
    }

    [CustomPropertyDrawer(typeof(EffectContext))] class EffectContextDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            pos.y = GUITools.PropertyFieldAndHeightChange(pos, prop, "radius");
            if (prop.FindPropertyRelative("radius").floatValue > 0) {
                pos.y = GUITools.PropertyFieldAndHeightChange(pos, prop, "checkLOSOnRadiusCast");
                pos.y = GUITools.PropertyFieldAndHeightChange(pos, prop, "mask");
            }
            pos.y = GUITools.PropertyFieldAndHeightChange(pos, prop, "message");
            pos.y = GUITools.PropertyFieldAndHeightChange(pos, prop, "effects");
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            float h = 0;
            SerializedProperty rProp = prop.FindPropertyRelative("radius");
            h += EditorGUIUtility.singleLineHeight * (rProp.floatValue > 0 ? 3 : 1);
            h += EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("message"));
            h += EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("effects"));
            return h;
        }
    }
    #endif
}