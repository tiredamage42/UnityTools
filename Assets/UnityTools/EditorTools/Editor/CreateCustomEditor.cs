using UnityEngine;
using UnityEditor;
using System.IO;

namespace UnityTools.EditorTools {
    public static class CreateCustomEditor
    {
        [MenuItem("Assets/Create/Custom Inspector", priority = 81)]
        static void CreateInsptorEditorClass()
        {
            foreach (var script in Selection.objects)
                BuildEditorFile(script);
            AssetDatabase.Refresh();
        }
        [MenuItem("Assets/Create/Custom Inspector", priority = 81, validate = true)]
        static bool ValidateCreateInsptorEditorClass()
        {
            foreach (var script in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(script);
                if (script.GetType() != typeof(MonoScript))
                    return false;
                if (!path.EndsWith(".cs"))
                    return false;
            }
            return true;
        }
        static void BuildEditorFile(Object obj)
        {
            MonoScript monoScript = obj as MonoScript;
            string assetPath = AssetDatabase.GetAssetPath(obj);
            var filename = Path.GetFileNameWithoutExtension(assetPath);
            string script = "";

            string scriptNamespace = monoScript.GetClass().Namespace;
            if (scriptNamespace == null)
                script = string.Format(template, filename);
            else
                script = string.Format(namespaceTemplate, filename, scriptNamespace);
            
            // make sure a editor folder exists for us to put this script into...       
            var editorFolder = Path.GetDirectoryName(assetPath) + "/Editor";
            if (!Directory.Exists(editorFolder))
                Directory.CreateDirectory(editorFolder);
            else {
                if (File.Exists(editorFolder + "/" + filename + "Editor.cs"))
                {
                    Debug.Log("ERROR: " +filename + "Editor.cs already exists.");
                    return;
                }
            }
            // finally write out the new editor~
            File.WriteAllText(editorFolder + "/" + filename + "Editor.cs", script);
        }
        
        #region Templates
        static readonly string template = @"
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof({0}))]
public class {0}Inspector : Editor
{{
    new {0} target;
    void OnEnable()
    {{
        target = base.target as {0};
    }}
    public override void OnInspectorGUI()
    {{
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }}
}}
";
            static readonly string namespaceTemplate = @"
using UnityEngine;
using UnityEditor;
namespace {1}
{{
    [CustomEditor(typeof({0}))]
    public class {0}Inspector : Editor
    {{
        new {0} target;
        void OnEnable()
        {{
            target = base.target as {0};
        }}
        public override void OnInspectorGUI()
        {{
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }}
    }}
}}
";
        #endregion
    }

}