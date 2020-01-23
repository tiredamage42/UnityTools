

using UnityEngine;
using UnityEditor;
using UnityTools.EditorTools;

namespace UnityTools.Rendering {
    [CreateAssetMenu(menuName="Unity Tools/Game Effects/Image Space Modifier Effect", fileName="ImageSpaceModifierEffect")]
    public class ImageSpaceModifierEffect : GameEffect {
        protected override bool PlayerOnly() { return true; }
        protected override bool CreateCopy() { return false; }
        public bool keepInEffectsList;
        public ImageSpaceMod mod;
        protected override bool AddToEffectsList() {
            return keepInEffectsList;
        }
        public override string GetDescription(float magnitude, float duration) { return null; }
        public override void OnEffectRemove (DynamicObject caster, DynamicObject target, float magnitude, float duration) {
            ImageSpaceModifier.RemoveImageSpaceModifier(mod.profile, mod.fadeOut);
        }
        protected override void OnEffectUpdate(DynamicObject caster, DynamicObject target, float deltaTime, float timeAdded, float currentTime, float magnitude, float duration, out bool removeEffect) {
            removeEffect = false;
        }
        protected override bool EffectValid (DynamicObject caster, DynamicObject target) {
            return true;
        }
        protected override bool OnEffectStart (DynamicObject caster, DynamicObject target, float magnitude, float duration) {
            // if we're keeping in the effects list, let the game effect handle duration....
            return ImageSpaceModifier.AddImageSpaceModifier(mod.profile, mod.fadeIn, keepInEffectsList ? 0 : mod.duration, mod.fadeOut, mod.anim, mod.animCycle);
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(ImageSpaceModifierEffect))] class ImageSpaceModifierEffectEditor : Editor {
        public override void OnInspectorGUI() {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mod"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keepInEffectsList"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conditions"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keywords"), true);
            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}

