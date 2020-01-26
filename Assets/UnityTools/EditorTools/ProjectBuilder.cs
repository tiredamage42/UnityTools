#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace UnityTools.EditorTools {

    public class AutoBuildSceneAttribute : AssetSelectionAttribute {
        public override List<Object> OnAssetsFound(List<Object> originals) {
            ProjectBuilder.RemoveAutoIncludedScenes(originals);
            return originals;
        }
        
        public AutoBuildSceneAttribute ( ) : base(typeof(SceneAsset)) { }
    }


    [Serializable] public class BuildTargetDefList : NeatListWrapper<BuildTargetDef> { }
    [Serializable] public class BuildTargetDef {
        public ProjectBuilder.SupportedBuild buildTarget;
        public bool autoRun;
        [NeatArray] public NeatStringArray customDefines;
        public BuildTargetDef (ProjectBuilder.SupportedBuild buildTarget, bool autoRun=false, string[] customDefines=null) {
            this.buildTarget = buildTarget;
            this.autoRun = autoRun;
            this.customDefines = new NeatStringArray( customDefines );
        }
    }

    [CustomPropertyDrawer(typeof(BuildTargetDef))]
    class BuildTargetDefDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            EditorGUI.PropertyField(new Rect(pos.x, pos.y, pos.width - GUITools.iconButtonWidth, EditorGUIUtility.singleLineHeight), prop.FindPropertyRelative("buildTarget"), true);
            GUITools.DrawIconToggle(prop.FindPropertyRelative("autoRun"), new GUIContent("A", "Auto Run"), pos.x + (pos.width - GUITools.iconButtonWidth), pos.y);
            EditorGUI.PropertyField(new Rect(pos.x, pos.y + EditorGUIUtility.singleLineHeight, pos.width, EditorGUIUtility.singleLineHeight), prop.FindPropertyRelative("customDefines"), true);            
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight + EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("customDefines"), true);
        }
    }


    
    public static class ProjectBuilder
    {

        public static void RemoveAutoIncludedScenes(List<Object> scenes) {
            if (onRemoveAutoIncludedScenes != null)
                onRemoveAutoIncludedScenes(scenes);   
        }
        public static event Action<List<Object>> onRemoveAutoIncludedScenes;

        
        static void AddAutoIncludedScenes (List<EditorBuildSettingsScene> scenes, List<SceneAsset> allScenesInProject) {
            if (onAddAutoIncludedScenes != null)
                onAddAutoIncludedScenes(scenes, allScenesInProject);
        }
        public static event Action<List<EditorBuildSettingsScene>, List<SceneAsset>> onAddAutoIncludedScenes;

    
        public enum VersionUpdate { None, Major, Minor, Patch };
        const string versionPrefix = "VS_";
        const string buildsDir = "Builds/";

        static void GetLatestVersion (out Vector3Int version) {
            version = new Vector3Int(-1,-1,-1);
            
            string[] dirs = Directory.GetDirectories(buildsDir, versionPrefix + "*", SearchOption.TopDirectoryOnly);
            foreach (string dir in dirs) {
                string[] v = dir.Split('_');
                int maj = int.Parse(v[1]);
                if (maj < version.x) continue;
                
                int min = int.Parse(v[2]);
                int pat = int.Parse(v[3]);

                // later major patch
                if (maj > version.x) {
                    version.x = maj;
                    version.y = min;
                    version.z = pat;
                }
                // same maj
                else {
                    if (min < version.y) continue;
                    
                    if (min > version.y) {
                        version.y = min;
                        version.z = pat;
                    }
                    // same maj.min
                    else {
                        if (pat < version.z) continue;
                        
                        if (pat > version.z) 
                            version.z = pat;
                        else 
                            Debug.LogWarning("Duplicate version at path: " + dir);
                    }
                }
            }
            if (version.x < 0) version.x = 1;
            if (version.y < 0) version.y = 0;
            if (version.z < 0) version.z = 0;        
        }

        static string GenerateVersionDirectory (string version) {
            string dir = buildsDir + versionPrefix + version.Replace('.', '_') + "/";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            
            return Path.GetFullPath(dir);
        }

        static string GenerateBuildDirectory(string versionDir, BuildTarget target, out bool existsAlready)
        {
            string dir = versionDir + "/" + target.ToString() + "/";
            existsAlready = Directory.Exists(dir);
            
            if (!existsAlready)
                Directory.CreateDirectory(dir);
            
            return Path.GetFullPath(dir);
        }
        
        static void CheckBuildTargetsForDuplicatesAndOnlyOneAutoRun (List<BuildTargetDef> targets) {
            
            // remove duplicates
            HashSet<int> dup = new HashSet<int>();
            for (int i = 0; i < targets.Count; i++) {
                if (dup.Contains(i)) 
                    continue;
                for (int x = i + 1; x < targets.Count; x++) 
                    if (targets[i].buildTarget == targets[x].buildTarget) 
                        dup.Add(x);
            }
            for (int i = targets.Count - 1; i >= 0; i--)
                if (dup.Contains(i)) 
                    targets.RemoveAt(i);

            // make sure only one auto runs
            bool running = false;
            for (int i = 0; i < targets.Count; i++) {
                if (targets[i].autoRun) {
                    if (running)
                        targets[i].autoRun = false;
                    else 
                        running = true;
                }
            }
        }

        public static void PerformBuild (List<EditorBuildSettingsScene> buildScenes, string company, string productName, List<BuildTargetDef> buildTargets, Vector3Int forcedVersion, bool devBuild, bool connectProfiler, bool allowDebugging, bool headless, string bundleID = null) { 
            PerformBuild (buildScenes, company, productName, buildTargets, VersionUpdate.None, true, forcedVersion, devBuild, connectProfiler, allowDebugging, headless, bundleID);
        }
        public static void PerformBuild (List<EditorBuildSettingsScene> buildScenes, string company, string productName, List<BuildTargetDef> buildTargets, VersionUpdate versionUpdate, bool devBuild, bool connectProfiler, bool allowDebugging, bool headless, string bundleID = null) {
            PerformBuild (buildScenes, company, productName, buildTargets, versionUpdate, false, Vector3Int.zero, devBuild, connectProfiler, allowDebugging, headless, bundleID);
        }
        public static void PerformBuild (List<EditorBuildSettingsScene> buildScenes, string company, string productName, List<BuildTargetDef> buildTargets, VersionUpdate versionUpdate, bool forceVersion, Vector3Int forcedVersion, bool devBuild, bool connectProfiler, bool allowDebugging, bool headless, string bundleID)
        {
            
            CheckBuildTargetsForDuplicatesAndOnlyOneAutoRun (buildTargets);

            if (buildTargets.Count == 0)
                return;
            
            AddAutoIncludedScenes(buildScenes, AssetTools.FindAssetsByType<SceneAsset>(log: false, null));
            
            BuildOptions options = BuildOptions.None;
            if (devBuild)           options |= BuildOptions.Development;
            if (allowDebugging)     options |= BuildOptions.AllowDebugging;
            if (headless)           options |= BuildOptions.EnableHeadlessMode;
            if (connectProfiler)    options |= BuildOptions.ConnectWithProfiler;
            
            // Save current player settings, and then set target settings.
            string preBuildCompanyName = PlayerSettings.companyName;
            string preBuildProductName = PlayerSettings.productName;
            string oldVersion = PlayerSettings.bundleVersion;
            
            EditorBuildSettingsScene[] scenes = CheckForNullsAndDuplicateScenes (buildScenes);
            
            Vector3Int version;
            if (forceVersion) {
                version = forcedVersion;
            }
            else {
                GetLatestVersion (out version);
                switch (versionUpdate) {
                    case VersionUpdate.Major: version.x++; break;
                    case VersionUpdate.Minor: version.y++; break;
                    case VersionUpdate.Patch: version.z++; break;
                }
            }

            string versionS = version.x.ToString() + "." + version.y.ToString() + "." + version.z.ToString();

            string versionDir = GenerateVersionDirectory (versionS);

            PlayerSettings.companyName = company;
            PlayerSettings.productName = productName;
            PlayerSettings.bundleVersion = versionS;
            
            int successCount = 0;
            int failCount = 0;

            for (int i = 0; i < buildTargets.Count; i++) {
                bool success = BuildPlayer(scenes, options, versionDir, productName, buildTargets[i].buildTarget, buildTargets[i].autoRun, buildTargets[i].customDefines, bundleID);
        
                if (success)
                    ++successCount;
                else
                    ++failCount;
            }

            //restore old player settings
            PlayerSettings.companyName = preBuildCompanyName;
            PlayerSettings.productName = preBuildProductName;
            PlayerSettings.bundleVersion = oldVersion;

            // Report success/failure.
            Debug.Log("Build Complete. " + string.Format("{0} success. {1} failure.", successCount, failCount));
            
            System.Diagnostics.Process.Start(versionDir);
        }

        static EditorBuildSettingsScene[] CheckForNullsAndDuplicateScenes (List<EditorBuildSettingsScene> scenes) {
            // Verify that all scenes in list still exist.
            for (int i = scenes.Count - 1; i >= 0; i--)
                if (string.IsNullOrEmpty(scenes[i].path) || !File.Exists(scenes[i].path))
                    scenes.RemoveAt(i);
            
            // remove duplicates
            HashSet<int> dup = new HashSet<int>();
            for (int i = 0; i < scenes.Count; i++) {
                if (dup.Contains(i)) 
                    continue;
                for (int x = i + 1; x < scenes.Count; x++) 
                    if (scenes[i].path == scenes[x].path) 
                        dup.Add(x);
            }
            for (int i = scenes.Count - 1; i >= 0; i--)
                if (dup.Contains(i)) 
                    scenes.RemoveAt(i);

            return scenes.ToArray();
        }

        static bool BuildPlayer(EditorBuildSettingsScene[] scenes, BuildOptions options, string versionDir, string productName, SupportedBuild buildTarget, bool autoRun, string[] customDefines, string bundleID)
        {

            BuildTarget target = Supported2Target(buildTarget);
            string dir = GenerateBuildDirectory(versionDir, target, out bool dirExists);
            if (dirExists) {
                if (EditorUtility.DisplayDialog("Overwrite Build", "Overwrite build at path: '" + dir + "' ?", "Yes", "No"))
                    Directory.Delete(dir);
                else 
                    return false;
            }
                
            if (autoRun) 
                options |= BuildOptions.AutoRunPlayer;
            
            string fileName = string.Format(BinaryNameFormat (target), productName);
            
            BuildTargetGroup targetGroup = GetBuildTargetGroup (target);        
            string preBuildDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            string preBuildBundleID = PlayerSettings.GetApplicationIdentifier(targetGroup);

            PlayerSettings.SetApplicationIdentifier(targetGroup, bundleID);

            string defines = preBuildDefines;
            if (customDefines != null) 
                defines += ";" + string.Join(";", customDefines);
            
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defines);
            //Debug.Log("Build Defines: " + defines);
            
            /*
            // APPLY ANDROID VARIANTS
            PlayerSettings.Android.targetArchitectures = (AndroidArchitecture)System.Enum.Parse(typeof(AndroidArchitecture), { "FAT", "ARMv7", "x86" });
            EditorUserBuildSettings.androidBuildSubtarget = (MobileTextureSubtarget)System.Enum.Parse(typeof(MobileTextureSubtarget), { "ETC", "ETC2", "ASTC", "DXT", "PVRTC", "ATC", "Generic" });
            EditorUserBuildSettings.androidBuildSystem = (AndroidBuildSystem)System.Enum.Parse(typeof(AndroidBuildSystem), { "Internal", "Gradle", "ADT (Legacy)" });
            */
            
            string error = "";
            var report = BuildPipeline.BuildPlayer(scenes, Path.Combine(dir, fileName), target, options);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
                error = report.summary.totalErrors + " occurred.";

            bool success = string.IsNullOrEmpty(error);
            if (!success)
                Debug.LogError("Build Failed: " + target.ToString() + "\n" + error);
            
            // Restore pre-build settings.
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, preBuildDefines);
            PlayerSettings.SetApplicationIdentifier(targetGroup, preBuildBundleID);
            
            return success;
        }

        static BuildTargetGroup GetBuildTargetGroup (BuildTarget target) {
            switch (target) {
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;
                case BuildTarget.Lumin:
                    return BuildTargetGroup.Lumin;
                case BuildTarget.PS4:
                    return BuildTargetGroup.PS4;
                case BuildTarget.StandaloneLinux:
                    return BuildTargetGroup.Standalone;;
                case BuildTarget.StandaloneLinux64:
                    return BuildTargetGroup.Standalone;;
                case BuildTarget.StandaloneLinuxUniversal:
                    return BuildTargetGroup.Standalone;;
                case BuildTarget.StandaloneOSX:
                    return BuildTargetGroup.Standalone;;
                case BuildTarget.StandaloneWindows:
                    return BuildTargetGroup.Standalone;;
                case BuildTarget.StandaloneWindows64:
                    return BuildTargetGroup.Standalone;;
                case BuildTarget.Switch:
                    return BuildTargetGroup.Switch;
                case BuildTarget.tvOS:
                    return BuildTargetGroup.tvOS;
                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;
                case BuildTarget.WSAPlayer:
                    return BuildTargetGroup.WSA;
                case BuildTarget.XboxOne:
                    return BuildTargetGroup.XboxOne;
                case BuildTarget.NoTarget:
                    break;
            }
            return BuildTargetGroup.Unknown;
        }

        public enum SupportedBuild { 
            Android, iOS, 
            StandaloneLinux, StandaloneLinux64, StandaloneLinuxUniversal, 
            StandaloneOSX, 
            StandaloneWindows, StandaloneWindows64, 
            WebGL,
        };
            

        static BuildTarget Supported2Target (SupportedBuild supported) {
            switch (supported) {
                case SupportedBuild.Android:
                    return BuildTarget.Android;
                case SupportedBuild.iOS:
                    return BuildTarget.iOS;
                case SupportedBuild.StandaloneLinux:
                    return BuildTarget.StandaloneLinux;
                case SupportedBuild.StandaloneLinux64:
                    return BuildTarget.StandaloneLinux64;
                case SupportedBuild.StandaloneLinuxUniversal:
                    return BuildTarget.StandaloneLinuxUniversal;
                case SupportedBuild.StandaloneOSX:
                    return BuildTarget.StandaloneOSX;
                case SupportedBuild.StandaloneWindows:
                    return BuildTarget.StandaloneWindows;
                case SupportedBuild.StandaloneWindows64:
                    return BuildTarget.StandaloneWindows64;
                case SupportedBuild.WebGL:
                    return BuildTarget.WebGL;
            }   
            return BuildTarget.NoTarget;
        }

        static string BinaryNameFormat (BuildTarget target) {
            switch (target) {
                case BuildTarget.Android:
                    return "{0}.apk";
                case BuildTarget.iOS:
                    return "{0}.apk";
                case BuildTarget.StandaloneLinux:
                    return "{0}.x86";
                case BuildTarget.StandaloneLinux64:
                    return "{0}.x86_64";
                case BuildTarget.StandaloneLinuxUniversal:
                    return "{0}";
                case BuildTarget.StandaloneOSX:
                    return "{0}.app";
                case BuildTarget.StandaloneWindows:
                    return "{0}.exe";
                case BuildTarget.StandaloneWindows64:
                    return "{0}.exe";
                case BuildTarget.WebGL:
                    return "{0}";
                
                case BuildTarget.NoTarget: break;
                case BuildTarget.Lumin: break;
                case BuildTarget.PS4: break;
                case BuildTarget.Switch: break;
                case BuildTarget.tvOS: break;
                case BuildTarget.WSAPlayer: break;
                case BuildTarget.XboxOne: break;
            }
            return null;
        }
        
        /*
        static void BuildAssetBundles_ (string directory, BuildAssetBundleOptions options, BuildTarget buildTarget) {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            BuildPipeline.BuildAssetBundles(directory, options, buildTarget);
        }    
        */

    }

}



#endif
