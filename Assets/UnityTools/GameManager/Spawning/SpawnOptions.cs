
using UnityEngine;
using UnityTools.EditorTools;
using UnityEditor;
using System;
namespace UnityTools.Spawning {

    [Serializable] public class SpawnOptions {
        public bool ground, navigate, uncollide;
        public SpawnOptions (bool ground, bool navigate, bool uncollide) {
            this.ground = ground;
            this.navigate = navigate;
            this.uncollide = uncollide;
        }
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SpawnOptions))] class SpawnOptionsDrawer : PropertyDrawer
    {
        void DrawProp (ref float x, float y, float width, SerializedProperty prop, string propName, GUIContent gui) {
            GUITools.DrawToggleButton(prop.FindPropertyRelative(propName), gui, x, y, width, GUITools.singleLineHeight, GUITools.toolbarButton);
            x += width;
        }            
        static readonly GUIContent[] guis = new GUIContent[] {
            new GUIContent("Ground", "Raycast To Find Environment Ground"),
            new GUIContent("Navigate", "Make Sure Position Is On NavMesh If Possible"),
            new GUIContent("Uncollide", "Move The Spawned Object So Its Colliders Arent Intersecting Any Other Colliders"),
        };
        static readonly string[] optionNames = new string[] { "ground", "navigate", "uncollide" };
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            float w = pos.width * .333f;
            float x = pos.x;
            for (int i = 0; i < 3; i++) DrawProp (ref x, pos.y, w, prop, optionNames[i], guis[i]);
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight;
        }
    }
#endif
}