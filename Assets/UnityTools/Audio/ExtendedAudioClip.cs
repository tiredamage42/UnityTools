using UnityEngine;
using UnityEditor;
using UnityTools.EditorTools;

namespace UnityTools.Audio {

    [System.Serializable] public class ExtendedAudioClipArray : NeatArrayWrapper<ExtendedAudioClip> { }

    [System.Serializable] public class ExtendedAudioClip
    {
        [NeatArray] public PreviewedAudioClipArray clips;
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

        public bool showAdvanced;

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
    [CustomPropertyDrawer(typeof(ExtendedAudioClip))]
    class ExtendedAudioClipDrawer : PropertyDrawer {

        static GUIContent unloopedContent = BuiltInIcons.GetIcon("preAudioLoopOff", "Looped");
        static GUIContent oneShotContent = BuiltInIcons.GetIcon("AudioListener Icon", "One Shot");
        static GUIContent volumeContent = BuiltInIcons.GetIcon("AudioSource Icon", "Volume");
        static GUIContent pitchRangeContent = BuiltInIcons.GetIcon("AudioClip Icon", "Pitch Range");
        static GUIContent linearRolloffContent = BuiltInIcons.GetIcon("DefaultSorting", "Linear Roloff");
        static GUIContent spatialContent = BuiltInIcons.GetIcon("Prefab Icon", "3D Sound");
        static GUIContent minMaxDistanceContent = BuiltInIcons.GetIcon("Transform Icon", "Min Max Distance");
        static GUIContent advancedSettingsContent = BuiltInIcons.GetIcon("_Popup", "Advanced Settings");

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            SerializedProperty clipsProp = prop.FindPropertyRelative("clips");
            SerializedProperty is2D = prop.FindPropertyRelative("is2D");
            SerializedProperty showAdvanced = prop.FindPropertyRelative("showAdvanced");

            pos.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(pos, label, GUITools.boldLabel);
            
            float h = CalculateHeight (clipsProp, is2D, showAdvanced);
            GUITools.Box(new Rect(pos.x, pos.y, pos.width, h + EditorGUIUtility.singleLineHeight * .1f), GUITools.shade);
            
            pos.y += GUITools.singleLineHeight;
            
            pos.x += GUITools.iconButtonWidth;
            pos.width -= GUITools.iconButtonWidth;

            EditorGUI.PropertyField(pos, clipsProp, true);
            pos.y += EditorGUI.GetPropertyHeight(clipsProp, true);
            pos.height = GUITools.singleLineHeight;

            SerializedProperty oneShot = prop.FindPropertyRelative("oneShot");

            float x = pos.x;

            if (!oneShot.boolValue) {
                GUITools.DrawToggleButton (prop.FindPropertyRelative("loop"), unloopedContent, x, pos. y, GUITools.blue, GUITools.white);
                x += GUITools.iconButtonWidth;
            }

            GUITools.DrawToggleButton (oneShot, oneShotContent, x, pos. y, GUITools.blue, GUITools.white);
            x += GUITools.iconButtonWidth;
            
            GUITools.DrawToggleButton (is2D, spatialContent, x, pos. y, GUITools.white, GUITools.blue);
            x += GUITools.iconButtonWidth;
            
            if (!is2D.boolValue) {
                GUITools.DrawToggleButton (prop.FindPropertyRelative("useLinearRolloff"), linearRolloffContent, x, pos.y, GUITools.blue, GUITools.white);
                x += GUITools.iconButtonWidth;
            }
            GUITools.DrawToggleButton (showAdvanced, advancedSettingsContent, x, pos.y, GUITools.blue, GUITools.white);
            pos.y += GUITools.singleLineHeight;

            GUITools.DrawIconPrefixedField ( pos.x, pos.y, pos.width, pos.height, prop.FindPropertyRelative("volume"), volumeContent, GUITools.white );
            pos.y += GUITools.singleLineHeight;

            GUITools.DrawIconPrefixedField ( pos.x, pos.y, pos.width, pos.height, prop.FindPropertyRelative("pitchRange"), pitchRangeContent, GUITools.white );
            pos.y += GUITools.singleLineHeight;

            if (!is2D.boolValue) {
                GUITools.DrawIconPrefixedField (pos.x, pos.y, pos.width, pos.height, prop.FindPropertyRelative("minMaxDistance"), minMaxDistanceContent, GUITools.white );
                pos.y += GUITools.singleLineHeight;
            }

            if (showAdvanced.boolValue) {
                pos.height = EditorGUIUtility.singleLineHeight;
            
                EditorGUI.LabelField(pos, "Advanced Settings:", GUITools.boldLabel);
                pos.y += EditorGUIUtility.singleLineHeight;

                if (!is2D.boolValue) {
                    EditorGUI.PropertyField(pos, prop.FindPropertyRelative("spread"), true);
                    pos.y += EditorGUIUtility.singleLineHeight;
                    
                    EditorGUI.PropertyField(pos, prop.FindPropertyRelative("doppler"), true);
                    pos.y += EditorGUIUtility.singleLineHeight;
                }
                
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("priority"), true);
                pos.y += EditorGUIUtility.singleLineHeight;

                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("stereoPan"), true);
            }
        }


        float CalculateHeight (SerializedProperty clipsProp, SerializedProperty is2D, SerializedProperty showAdvanced) {
            float h = 0;
            h += EditorGUI.GetPropertyHeight(clipsProp, true);
            h += GUITools.singleLineHeight * 4;
            if (!is2D.boolValue) h += GUITools.singleLineHeight;
            if (showAdvanced.boolValue) {
                h += EditorGUIUtility.singleLineHeight * 3;
                if (!is2D.boolValue) h += EditorGUIUtility.singleLineHeight * 2;
            }
            return h;
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return CalculateHeight (prop.FindPropertyRelative("clips"), prop.FindPropertyRelative("is2D"), prop.FindPropertyRelative("showAdvanced")) + EditorGUIUtility.singleLineHeight * .5f;
        }            
    }
    #endif

}
