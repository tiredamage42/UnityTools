

using UnityEngine;
using UnityEditor;
using UnityTools.EditorTools;

namespace UnityTools {
    [CreateAssetMenu(menuName="Unity Tools/Game Effects/Dispell Effects By Keyword Effect", fileName="DispellEffectsByKeywordEffect")]
    public class DispellEffectsByKeywordEffect : GameEffect {
        protected override bool PlayerOnly() { return false; }
        protected override bool CreateCopy() { return false; }
        protected override bool AddToEffectsList() { return false; }
        public override string GetDescription(float magnitude, float duration) { return null; }
        public override void OnEffectRemove (DynamicObject caster, DynamicObject obj, float magnitude, float duration) { }
        protected override void OnEffectUpdate(DynamicObject caster, DynamicObject obj, float deltaTime, float timeAdded, float currentTime, float magnitude, float duration, out bool removeEffect) { removeEffect = false; }
        protected override bool EffectValid (DynamicObject caster, DynamicObject obj) {
            return obj.GetObjectScript<GameEffectsHandler>() != null;
        }
        protected override bool OnEffectStart (DynamicObject caster, DynamicObject obj, float magnitude, float duration) {
            obj.GetObjectScript<GameEffectsHandler>().RemoveEffects(keywords);
            return true;
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(DispellEffectsByKeywordEffect))]
    public class DispellEffectsByKeywordEffectEditor : Editor {
        public override void OnInspectorGUI() {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conditions"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keywords"), true);
            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}