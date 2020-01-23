using UnityEngine;
using UnityEditor;
using UnityTools.EditorTools;

namespace UnityTools.Audio {

    [System.Serializable] public class ExtendedAudioClipArray : NeatArrayWrapper<ExtendedAudioClip> { }

    [System.Serializable] public class ExtendedAudioClip
    {
        [NeatArray] public PreviewedAudioAssetArray clips;
        public bool loop;
        public bool oneShot = true;
        [Range(0,1)] public float volume = 1;
        public Vector2 pitchRange = new Vector2(.9f, 1.1f);
        public bool is2D;
        public Vector2 minMaxDistance = new Vector2(0, 25);
        public bool useLinearRolloff;
        [Tooltip("Default: 128")] [Range(0,256)] public int priority = 128;
        [Tooltip("L - R")] [Range(-1,1)] public float stereoPan = 0;
        [Tooltip("How much 3D position affects panning")] [Range(0,360)] public float spread = 0;
        [Tooltip("Default: 1.0")][Range(0,5)] public float doppler = 1.0f;

        public ExtendedAudioClip () {
            oneShot = true;
            volume = 1;
            pitchRange = new Vector2(.9f, 1.1f);
            minMaxDistance = new Vector2(0, 25);
            priority = 128;
            doppler = 1.0f;
        }
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ExtendedAudioClip))] class ExtendedAudioClipDrawer : PropertyDrawer {

        void DrawToolbar (float x, float y, SerializedProperty prop, SerializedProperty oneShot, SerializedProperty is2D) {
            GUITools.DrawIconToggle (oneShot, BuiltInIcons.GetIcon("AudioListener Icon", "One Shot"), x, y);
            x += GUITools.iconButtonWidth;
            
            GUI.enabled = !oneShot.boolValue;
            GUITools.DrawIconToggle (prop.FindPropertyRelative("loop"), BuiltInIcons.GetIcon("preAudioLoopOff", "Looped"), x, y);
            x += GUITools.iconButtonWidth;
            GUI.enabled = true;

            GUITools.DrawIconToggle (is2D, BuiltInIcons.GetIcon("Prefab Icon", "3D Sound"), x, y, GUITools.white, GUITools.blue);
            x += GUITools.iconButtonWidth;
            
            GUI.enabled = !is2D.boolValue;
            GUITools.DrawIconToggle (prop.FindPropertyRelative("useLinearRolloff"), BuiltInIcons.GetIcon("DefaultSorting", "Linear Roloff"), x, y);
            x += GUITools.iconButtonWidth;
            GUI.enabled = true;

            prop.isExpanded = GUITools.DrawIconToggle (prop.isExpanded, BuiltInIcons.GetIcon("_Popup", "Advanced Settings"), x, y);
        }

        void DrawProp (SerializedProperty prop, ref Rect pos, string propName) {
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative(propName), true);
            pos.y += EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            SerializedProperty clipsProp = prop.FindPropertyRelative("clips");
            
            SerializedProperty is2D = prop.FindPropertyRelative("is2D");
            SerializedProperty oneShot = prop.FindPropertyRelative("oneShot");

            if (string.IsNullOrEmpty(label.text)) 
                label = new GUIContent("Clip");
            
            pos.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(pos, label, GUITools.boldLabel);

            DrawToolbar (pos.x + pos.width - GUITools.iconButtonWidth * 5, pos.y, prop, oneShot, is2D);

            GUITools.Box(new Rect(pos.x, pos.y, pos.width, CalculateHeight (clipsProp, is2D, prop.isExpanded) - EditorGUIUtility.singleLineHeight * .25f), GUITools.shade);
            
            pos.y += GUITools.singleLineHeight;
            pos.x += GUITools.iconButtonWidth * .5f;
            pos.width -= GUITools.iconButtonWidth * .5f;

            EditorGUI.PropertyField(pos, clipsProp, true);
            pos.y += EditorGUI.GetPropertyHeight(clipsProp, true);
            
            DrawProp (prop, ref pos, "volume");
            DrawProp (prop, ref pos, "pitchRange");
            
            if (!is2D.boolValue) 
                DrawProp (prop, ref pos, "minMaxDistance");
            
            if (prop.isExpanded) {
                if (!is2D.boolValue) {
                    DrawProp (prop, ref pos, "spread");
                    DrawProp (prop, ref pos, "doppler");
                }
                DrawProp (prop, ref pos, "priority");
                DrawProp (prop, ref pos, "stereoPan");
            }
        }

        float CalculateHeight (SerializedProperty clipsProp, SerializedProperty is2D, bool showAdvanced) {
            float h = GUITools.singleLineHeight * 3;
            h += EditorGUI.GetPropertyHeight(clipsProp, true);
            
            if (!is2D.boolValue) 
                h += GUITools.singleLineHeight;
            if (showAdvanced) {
                h += EditorGUIUtility.singleLineHeight * 2;
                if (!is2D.boolValue) 
                    h += EditorGUIUtility.singleLineHeight * 2;
            }
            return h;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return CalculateHeight (prop.FindPropertyRelative("clips"), prop.FindPropertyRelative("is2D"), prop.isExpanded);
        }            
    }
    #endif

}
