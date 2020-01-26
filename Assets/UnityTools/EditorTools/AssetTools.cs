#if UNITY_EDITOR

using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

using System;
using Object = UnityEngine.Object;

namespace UnityTools.EditorTools {

    public static class AssetTools
    {

        public static T CreateScriptableObject <T> (string path, bool refreshAndSave=true) where T : ScriptableObject {
            T r = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(r, path + ".asset");
            
            if (refreshAndSave) 
                ProjectTools.RefreshAndSave();
            
            return r;
        }

        public static List<T> FindAssetsByType<T> (bool log, Func<List<Object>, List<Object>> filter) where T : Object {
            return FindAssetsByType(typeof(T), log, filter).Cast<T>().ToList();
        }

        public static List<Object> FindAssetsByType (Type type, bool log, Func<List<Object>, List<Object>> filter)
        {
            if (log) 
                Debug.Log("Searching project for assets of type: " + type.FullName);
            
            List<Object> assets = new List<Object>();
            
            string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", type.FullName));
            if (guids.Length == 0) {
                if (log) 
                    Debug.Log("None Found, searching project for assets of type: " + type.Name);
                guids = AssetDatabase.FindAssets(string.Format("t:{0}", type.Name));
            }
            
            for( int i = 0; i < guids.Length; i++ ) {
                Object asset = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath( guids[i] ), type );
                if ( asset != null ) 
                    assets.Add(asset);
            }

            Object[] found = Resources.FindObjectsOfTypeAll(type);

            for (int i = 0; i < found.Length; i++) {
                if (!assets.Contains(found[i])) {
                    assets.Add(found[i]);

                    // if resources found it, but guids didnt, reimport the asset
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(found[i]));
                }
            }

            if (filter != null)
                assets = filter(assets);

            if (log) 
                Debug.Log("Found " + assets.Count + " assets in project");
            
            return assets;
        }
    }
}

#endif
