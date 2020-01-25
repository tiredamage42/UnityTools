using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

using UnityTools.Internal;
namespace UnityTools {

    /*
        create and maintain a dictionary of all locations in each scene, 

        so we can reference them without loading scenes

        dictionary is saved to a file in the streaming assets directory

        structure:
            Dictionary<Scene Name, Dictionary<Location Name, Location> 

        currently dictionary is updated every time a scene is saved
    */

    [InitializeOnLoad] public class LocationEditor {
        
        static LocationEditor () {
            EditorSceneManager.sceneSaving += RefreshLocations;
        }

        static List<LocationTemplate> GetAllLocationTemplates (Scene scene) {
            List<LocationTemplate> templates = new List<LocationTemplate>();
            
            GameObject[] rootObjs = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjs.Length; i++)
                templates.AddRange(rootObjs[i].GetComponentsInChildren<LocationTemplate>());
            
            return templates;
        }

        static void AddLocations (List<LocationTemplate> templates, string sceneName, List<LocationDefenition> allLocations, Func<LocationTemplate, bool> predicate, List<LocationDefenition> pointLocations) {
            List<LocationTemplate> templatesToUse = templates.Where( predicate ).ToList();
            for (int i = 0; i < templatesToUse.Count; i++) {
                AddLocations (templatesToUse[i], sceneName, pointLocations, allLocations);
            }
        }

        static void AddNonAreaLocations (List<LocationTemplate> templates, string sceneName, List<LocationDefenition> allLocations) {
            AddLocations (templates, sceneName, allLocations, s => s.type != LocationType.Area, null);
        }
        static void AddAreaLocations (List<LocationTemplate> templates, string sceneName, List<LocationDefenition> allLocations) {
            // get a list of all the "Point" type locations, so area locations can reference point locations within its bounds
            List<LocationDefenition> pointLocations = allLocations.MakeCopy().Where( s => s.type == (int)LocationType.Point ).ToList();
            AddLocations (templates, sceneName, allLocations, s => s.type == LocationType.Area, pointLocations);
        }

        static void RefreshLocations (Scene scene, string path) {
            string sceneName = scene.name;

            List<LocationTemplate> templates = GetAllLocationTemplates(scene);
            if (templates.Count == 0)
                return;

            Debug.Log("Refreshing Locations List for scene " + sceneName);
            
            List<LocationDefenition> allLocations = new List<LocationDefenition>();
            
            AddNonAreaLocations (templates, sceneName, allLocations);
            AddAreaLocations (templates, sceneName, allLocations);
            
            if (allLocations.Count == 0)
                return;

            Dictionary<string, LocationDefenition> locationDict = new Dictionary<string, LocationDefenition>();
            for (int i = 0; i < allLocations.Count; i++) {
                locationDict.Add(allLocations[i].name, allLocations[i]);
            }

            string directory = Locations.GetLocationsObjectDirectory();
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            string filePath = Locations.GetLocationsObjectPath();
            Dictionary<string, Dictionary<string, LocationDefenition>> existing;
            if (File.Exists(filePath)) {
                Debug.Log("Updating Locations File: " + filePath);
                existing = (Dictionary<string, Dictionary<string, LocationDefenition>>)IOTools.LoadFromFile(filePath);
                existing[sceneName] = locationDict;
            }
            else {
                Debug.Log("Creating Locations File: " + filePath);
                existing = new Dictionary<string, Dictionary<string, LocationDefenition>> () { { sceneName, locationDict } };
            }
            IOTools.SaveToFile(existing, filePath);
        }

        static string GetNameWParent (Transform t, string name) {
            if (t.parent == null)
                return name;
            return GetNameWParent(t.parent, t.parent.name + "." + name);
        }

        static void AddLocations (LocationTemplate template, string scene, List<LocationDefenition> pointLocations, List<LocationDefenition> allLocations) {
            Transform t = template.transform;
            string baseName = GetNameWParent(t, t.name);
            
            switch (template.type) {
                case LocationType.Point:
                    allLocations.Add(new LocationDefenition(scene, baseName, t));
                    break;
                case LocationType.Group:
                    if (template.subLocations.Length != 0) {
                        List<LocationDefenition> subLocations = new List<LocationDefenition>();
                        List<string> groupNames = new List<string>();
                        for (int i = 0; i < template.subLocations.Length; i++) 
                            groupNames.Add(subLocations.AddNew(new LocationDefenition(scene, baseName + "." + template.subLocations[i].name, template.subLocations[i].position, template.subLocations[i].rotation)).name);
                        
                        allLocations.Add(new LocationDefenition(scene, baseName, groupNames));
                        allLocations.AddRange(subLocations);
                    }
                    break;
                case LocationType.Area:
                    allLocations.Add(new LocationDefenition(scene, baseName, t.position, template.size, GetLocationsInArea(template, t.position, pointLocations)));
                    break;
            }
        }
        
        static List<string> GetLocationsInArea(LocationTemplate area, Vector3 pos, List<LocationDefenition> pointLocations) {
            Bounds bounds = new Bounds(pos, area.size);
            List<string> locationsInArea = new List<string>();
            for (int i = 0; i < pointLocations.Count; i++) {
                if (bounds.Contains(pointLocations[i].pos)) {
                    locationsInArea.Add(pointLocations[i].name);
                }
            }
            return locationsInArea;
        }
    }
}