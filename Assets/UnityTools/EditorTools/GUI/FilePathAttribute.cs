using System.IO;
using UnityEngine;
using UnityEditor;

namespace UnityTools.EditorTools
{
    public class FilePathAttribute : PropertyAttribute {
        public bool folder, allowManualEdit;
        public string defaultName = "";
        public FilePathAttribute(bool folder = true, bool allowManualEdit = true, string defaultName = "") {
            this.folder = folder;
            this.allowManualEdit = allowManualEdit;
            this.defaultName = defaultName;
        }
    }

#if UNITY_EDITOR    

    [CustomPropertyDrawer(typeof(FilePathAttribute))] public class FilePathDrawer : PropertyDrawer {
        static GUIStyle _style;
        static GUIStyle style {
            get {
                if (_style == null) {
                    _style = new GUIStyle(GUI.skin.button);
                    _style.alignment = TextAnchor.MiddleCenter;
                    _style.fontStyle = FontStyle.Normal;
                    _style.margin = new RectOffset(0, 5, 0, 0);
                    _style.fixedWidth = 30;
                }
                return _style;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                base.OnGUI(position, property, label);

            EditorGUI.BeginProperty(position, label, property);

            FilePathAttribute att = attribute as FilePathAttribute;

            EditorGUILayout.BeginHorizontal();

            if (att.allowManualEdit)
                property.stringValue = EditorGUILayout.TextField(label, property.stringValue);
            else
                EditorGUILayout.TextField(label, property.stringValue);

            if (GUILayout.Button("...", style))
                SetPath(property, att);
            
            EditorGUILayout.EndHorizontal();

            EditorGUI.EndProperty();
        }

        void SetPath(SerializedProperty property, FilePathAttribute att)
        {
            string projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..")) + Path.DirectorySeparatorChar;
        
            string directory;

            if (att.folder)
                directory = EditorUtility.OpenFolderPanel("Choose Folder", projectPath, att.defaultName);
            else
                directory = EditorUtility.OpenFilePanel("Choose File", projectPath, att.defaultName);

            // Canceled.
            if (string.IsNullOrEmpty(directory))
                return;

            // Normalize path separators.
            directory = Path.GetFullPath(directory);

            // If relative to project path, reduce the filepath to just what we need.
            if (directory.Contains(projectPath))
                directory = directory.Replace(projectPath, "");

            // Save setting.
            property.stringValue = directory;
        }
    }
#endif
}

