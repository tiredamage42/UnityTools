using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTools.EditorTools
{
    public class ProjectSize : EditorWindow
    {
        public struct FInfo {

            public GUIContent display;
            public double size;
            public FInfo (string path, double size) {
                this.size = size;
                this.display = new GUIContent("   [" + ConvertSize(size, false) + "] " + path);
            }
        }

        public class Category
        {
            public bool show;
            public string extension;
            public double amount;
            public List<FInfo> files = new List<FInfo>();
            public float GetPercent (long total) { return Percent((float)amount, total); }
            
            public Category(string extension) {
                this.extension = extension;
            }
            public void AddFile (string path, double size) {
                files.Add(new FInfo(path, size));
                this.amount += size;
            }
            public void Initialize () {
                files = files.OrderByDescending (i => i.size).ToList();
            }
        }

        List<FInfo> files = new List<FInfo>();    
        List<Category> categories = new List<Category>();
        long total;
        Vector2 scrollPos;
        bool showFiles;

        
        [MenuItem(ProjectTools.defaultMenuItemSection + "Unity Tools/Editor Tools/Project Size", false, ProjectTools.defaultMenuItemPriority)]
        private static void Init()
        {
            ProjectSize window = (ProjectSize)GetWindow(typeof(ProjectSize), true, "Project Size");
            window.Show();
            window.Analyze(false);
        }

        void DrawFilesList (List<FInfo> files, GUIStyle labelStyle) {
            for (int i = 0; i < files.Count; i++) 
                EditorGUILayout.LabelField(files[i].display, labelStyle);
        }
        
        void OnGUI()
        {            
            GUILayout.Space(4);
            GUIStyle label = new GUIStyle(EditorStyles.toolbarButton);// EditorStyles.label);
            label.richText = true;
            label.normal.textColor = Color.white;
            label.alignment = TextAnchor.MiddleLeft;
            
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.richText = true;
            labelStyle.alignment = TextAnchor.MiddleLeft;

            GUI.backgroundColor = GUITools.gray;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Re-Analyze"))
                Analyze(false);
            if (GUILayout.Button("Re-Analyze (Include Meta Files)"))
                Analyze(true);
            GUILayout.EndHorizontal();
            
            GUILayout.Label("<b>Total:</b>\t" + ConvertSize(total), labelStyle);
            GUILayout.EndVertical();
                        
            labelStyle.fontSize = 10;
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUI.backgroundColor = GUITools.gray;            
            if (GUILayout.Button("<b>ALL FILES</b>\t\t[" + files.Count + "]", label)) 
                showFiles = !showFiles;
            GUI.backgroundColor = GUITools.white;
            
            if (showFiles) 
                DrawFilesList (files, labelStyle);
                
            GUITools.Space(2);

            for (int i = 0; i < categories.Count; i++) 
                Bar(categories[i], label, labelStyle);
            
            EditorGUILayout.EndScrollView();
        }

        private void Bar(Category category, GUIStyle style, GUIStyle labelStyle)
        {
            string txt = "<b>" + category.extension + "</b>\t\t[" + category.files.Count + "]\t\t" + ConvertSize(category.amount) + "\t\t<i>" + category.GetPercent(total).ToString("0.00") + "%</i>";            
            GUI.backgroundColor = GUITools.darkGray;
            if (GUILayout.Button(txt, style)) 
                category.show = !category.show;
            GUI.backgroundColor = GUITools.white;
            
            if (category.show)
                DrawFilesList (category.files, labelStyle);
        }
        
        void Analyze(bool includeMetaFiles)
        {
            files.Clear();
            categories.Clear();
            total = 0;

            Dictionary<string, Category> ext2cat = new Dictionary<string, Category>();

            string[] filesFound = Directory.GetFiles(Application.dataPath + "/", "*.*", SearchOption.AllDirectories);

            foreach (string file in filesFound)
            {
                FileInfo info = new FileInfo(file);
                if (!includeMetaFiles && info.Extension.Contains(".meta")) 
                    continue;

                if (!ext2cat.TryGetValue(info.Extension, out Category category))
                    ext2cat[info.Extension] = category = new Category(info.Extension);
                
                string path = file.Substring(Application.dataPath.Length + 1);
                category.AddFile(path, info.Length);
                files.Add(new FInfo( path, info.Length ));
                // update the total
                total += info.Length;
            }
            
            categories = ext2cat.Values.OrderByDescending (i => i.amount).ToList();
            foreach (var category in categories) 
                category.Initialize();
            files = files.OrderByDescending (i => i.size).ToList();
        }

        static float Percent(float a, float b)
        {
            return (a / b) * 100;
        }


        static string ConvertSize(double sizeInBytes, bool useColor=true)
        {
            double size = SystemTools.ConvertSize(sizeInBytes, out FileSize fSize);
            if (useColor) {
                switch (fSize) {
                    case FileSize.B:    return size + " <color=#FCF960>" + fSize.ToString() + "</color>";
                    case FileSize.KB:   return size.ToString("0.00") + " <color=#FCF960>" + fSize.ToString() + "</color>";
                    case FileSize.MB:   return size.ToString("0.00") + " <color=#FFAD3B>" + fSize.ToString() + "</color>";
                    case FileSize.GB:   return size.ToString("0.00") + " <color=#C93038>" + fSize.ToString() + "</color>";
                }
            }
            else {
                switch (fSize) {
                    case FileSize.B:    return size + " " + fSize.ToString();
                    case FileSize.KB:   return size.ToString("0.00") + " " + fSize.ToString();
                    case FileSize.MB:   return size.ToString("0.00") + " " + fSize.ToString();
                    case FileSize.GB:   return size.ToString("0.00") + " " + fSize.ToString();
                }
            }
            return null;
        }
    }
}