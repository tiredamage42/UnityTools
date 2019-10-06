using UnityEngine;
using UnityEditor;

using UnityTools.EditorTools;
using UnityTools.Audio.Internal;
using UnityTools.Audio.Editor;

namespace UnityTools.Audio.Internal {
    [System.Serializable] public class PreviewedAudioClipArrayElement : NeatArrayElement { 
        [PreviewedAudio(false)] public AudioClip element; 
        public static implicit operator AudioClip (PreviewedAudioClipArrayElement c) { return c.element; }
    }
    
    [System.Serializable] public class PreviewedAudioAssetArrayElement : NeatArrayElement { 
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

    

    [System.Serializable] public class PreviewedAudioClipArray : NeatArrayWrapper<PreviewedAudioClipArrayElement> { }
    [System.Serializable] public class PreviewedAudioAssetArray : NeatArrayWrapper<PreviewedAudioAssetArrayElement> { }


    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(PreviewedAudioAttribute))] 
    class PreviewedAudioDrawer : PropertyDrawer {
        const float optionsToolbarSize = GUITools.iconButtonWidth * 2;
        const float fullToolbarSize = optionsToolbarSize + GUITools.toolbarDividerSize;

        static GUIContent playGUI = BuiltInIcons.GetIcon("preAudioPlayOff", "Play Clip");
        static GUIContent playGUILooped = BuiltInIcons.GetIcon("preAudioLoopOff", "Play Clip Looped");
        
        static bool DrawIconButton (float x, float y, bool state, GUIContent onGUI) {
            return GUITools.IconButton(x, y, onGUI, state ? GUITools.blue : GUITools.white);
        }

        static void DrawPlayStopButton (AudioClip newClip, float x, float y, bool isPlaying, bool sourceState, GUIContent onGUI, bool looped, Object targetObject) {
        
            bool wasEnabled = GUI.enabled;
            GUI.enabled = wasEnabled && (!isPlaying || sourceState);
            if (DrawIconButton ( x, y, sourceState, onGUI) ) {
            
                if (isPlaying) {
                    EditorAudioTools.StopClip(newClip);
                }
                else {
                    EditorAudioTools.PlayClip(newClip, looped, targetObject);
                }
            }
            GUI.enabled = wasEnabled;
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            EditorGUI.BeginProperty(pos, label, prop);
            
            Object oldClip = prop.objectReferenceValue;

            float toolbarStart = pos.width - fullToolbarSize;
            
            Rect rect = new Rect(pos.x, pos.y, toolbarStart, EditorGUIUtility.singleLineHeight);
            if ((attribute as PreviewedAudioAttribute).useAssetSelector) {
                AssetSelector.Draw(typeof(AudioClip), rect, prop, label, null);
            }
            else {
                EditorGUI.PropertyField(rect, prop, label);
            }

            if (oldClip != prop.objectReferenceValue) EditorAudioTools.StopClip(oldClip as AudioClip);

            GUITools.DrawToolbarDivider(pos.x + toolbarStart, pos.y);
            GUI.enabled = prop.objectReferenceValue != null;
            
            pos = new Rect(pos.x + toolbarStart + GUITools.toolbarDividerSize, pos.y, optionsToolbarSize, EditorGUIUtility.singleLineHeight);
            AudioClip newClip = prop.objectReferenceValue as AudioClip;
            AudioSource source;
            bool isPlaying = EditorAudioTools.IsClipPlaying(newClip, out source);
            DrawPlayStopButton (newClip, pos.x, pos.y, isPlaying, isPlaying && !source.loop, playGUI, false, prop.serializedObject.targetObject);
            DrawPlayStopButton (newClip, pos.x + GUITools.iconButtonWidth, pos.y, isPlaying, isPlaying && source.loop, playGUILooped, true, prop.serializedObject.targetObject);

            GUI.enabled = true;   
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight;
        }
    }
    #endif
}
