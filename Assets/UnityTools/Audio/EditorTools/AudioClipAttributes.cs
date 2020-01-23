using System;

using UnityEngine;
using UnityEditor;

using UnityTools.EditorTools;
using UnityTools.Audio.Internal;
using UnityTools.Audio.Editor;

namespace UnityTools.Audio.Internal {
    [Serializable] public class PreviewedAudioClipArrayElement : NeatArrayElement { 
        [PreviewedAudio(false)] public AudioClip element; 
        public static implicit operator AudioClip (PreviewedAudioClipArrayElement c) { return c.element; }
    }
    
    [Serializable] public class PreviewedAudioAssetArrayElement : NeatArrayElement { 
        [PreviewedAudio(true)] public AudioClip element; 
        public static implicit operator AudioClip (PreviewedAudioAssetArrayElement c) { return c.element; }
    }
}

namespace UnityTools.Audio {
    
    public class PreviewedAudioAttribute : PropertyAttribute { 
        public bool useAssetSelector;
        public PreviewedAudioAttribute (bool useAssetSelector) {
            this.useAssetSelector = useAssetSelector;
        }
    }

    [Serializable] public class PreviewedAudioClipArray : NeatArrayWrapper<PreviewedAudioClipArrayElement> { }
    [Serializable] public class PreviewedAudioAssetArray : NeatArrayWrapper<PreviewedAudioAssetArrayElement> { }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PreviewedAudioAttribute))] class PreviewedAudioDrawer : PropertyDrawer {
        const float optionsToolbarSize = GUITools.iconButtonWidth * 2;
        const float fullToolbarSize = optionsToolbarSize + GUITools.toolbarDividerSize;

        static void DrawPlayStopButton (AudioClip newClip, float x, float y, bool isPlaying, bool sourceState, GUIContent onGUI, bool looped, UnityEngine.Object targetObject) {
            bool wasEnabled = GUI.enabled;
            GUI.enabled = wasEnabled && (!isPlaying || sourceState);

            if (GUITools.DrawIconToggle(sourceState, onGUI, x, y)) {
                if (isPlaying) 
                    EditorAudioTools.StopClip(newClip);
                else 
                    EditorAudioTools.PlayClip(newClip, looped, targetObject);
            }
            GUI.enabled = wasEnabled;
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            
            UnityEngine.Object oldClip = prop.objectReferenceValue;

            Rect rect = new Rect(pos.x, pos.y, pos.width - fullToolbarSize, EditorGUIUtility.singleLineHeight);
            
            if ((attribute as PreviewedAudioAttribute).useAssetSelector) 
                AssetSelector.Draw(typeof(AudioClip), rect, prop, label, null);
            else 
                EditorGUI.PropertyField(rect, prop, label);
            
            float toolbarStart = pos.x + pos.width - fullToolbarSize;

            if (oldClip != prop.objectReferenceValue) EditorAudioTools.StopClip(oldClip as AudioClip);

            GUITools.DrawToolbarDivider(toolbarStart, pos.y);
            GUI.enabled = prop.objectReferenceValue != null;
            
            AudioClip newClip = prop.objectReferenceValue as AudioClip;
            
            AudioSource source;
            bool isPlaying = EditorAudioTools.IsClipPlaying(newClip, out source);
            
            float xStart = toolbarStart + GUITools.toolbarDividerSize;
            UnityEngine.Object targetObject = prop.serializedObject.targetObject;
            DrawPlayStopButton (newClip, xStart + GUITools.iconButtonWidth * 0, pos.y, isPlaying, isPlaying && !source.loop, BuiltInIcons.GetIcon("preAudioPlayOff", "Play Clip"), false, targetObject);
            DrawPlayStopButton (newClip, xStart + GUITools.iconButtonWidth * 1, pos.y, isPlaying, isPlaying && source.loop, BuiltInIcons.GetIcon("preAudioLoopOff", "Play Clip Looped"), true, targetObject);

            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight;
        }
    }
    #endif
}
