using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

using UnityTools.Spawning.Internal;
namespace UnityTools.Spawning {

    /*
        create and maintain a dictionary of all spawn points in each scene, 

        so we can reference them without loading scenes

        dictionary is saved to a file in the streaming assets directory

        structure:
            Dictionary<Scene Name, Dictionary<SpawnPoint Name, SpawnPoint> 

        currently dictionary is updated every time a scene is saved
    */

    [InitializeOnLoad] public class SpawnPointEditor {
        
        static SpawnPointEditor () {
            EditorSceneManager.sceneSaving += RefreshSpawnPoints;
        }

        static List<SceneSpawnPoint> GetAllSpawnPointTemplates (Scene scene) {
            List<SceneSpawnPoint> templates = new List<SceneSpawnPoint>();
            
            GameObject[] rootObjs = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjs.Length; i++)
                templates.AddRange(rootObjs[i].GetComponentsInChildren<SceneSpawnPoint>());
            
            return templates;
        }

        static void AddSpawns (List<SceneSpawnPoint> templates, string sceneName, List<SpawnPoint> allSpawns, Func<SceneSpawnPoint, bool> predicate, List<SpawnPoint> pointSpawns) {
            List<SceneSpawnPoint> templatesToUse = templates.Where( predicate ).ToList();
            for (int i = 0; i < templatesToUse.Count; i++) {
                AddSpawnPoints (templatesToUse[i], sceneName, pointSpawns, allSpawns);
            }
        }

        static void AddNonAreaSpawnPoints (List<SceneSpawnPoint> templates, string sceneName, List<SpawnPoint> allSpawns) {
            AddSpawns (templates, sceneName, allSpawns, s => s.type != SpawnPointType.Area, null);
        }
        static void AddAreaSpawnPoints (List<SceneSpawnPoint> templates, string sceneName, List<SpawnPoint> allSpawns) {
            // get a list of all the "Point" type spawn points, so area spawn point can reference point spawns within its bounds
            List<SpawnPoint> pointSpawns = allSpawns.MakeCopy().Where( s => s.type == (int)SpawnPointType.Point ).ToList();
            AddSpawns (templates, sceneName, allSpawns, s => s.type == SpawnPointType.Area, pointSpawns);
        }

        static void RefreshSpawnPoints (Scene scene, string path) {
            string sceneName = scene.name;

            List<SceneSpawnPoint> templates = GetAllSpawnPointTemplates(scene);
            if (templates.Count == 0)
                return;

            Debug.Log("Refreshing Spawn Points List for scene " + sceneName);
            
            List<SpawnPoint> allSpawns = new List<SpawnPoint>();
            
            AddNonAreaSpawnPoints (templates, sceneName, allSpawns);
            AddAreaSpawnPoints (templates, sceneName, allSpawns);
            
            if (allSpawns.Count == 0)
                return;

            Dictionary<string, SpawnPoint> spawnDict = new Dictionary<string, SpawnPoint>();
            for (int i = 0; i < allSpawns.Count; i++) {
                spawnDict.Add(allSpawns[i].name, allSpawns[i]);
            }

            string directory = SpawnPoint.GetSpawnPointsObjectDirectory();
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            string filePath = SpawnPoint.GetSpawnPointsObjectPath();
            Dictionary<string, Dictionary<string, SpawnPoint>> existing;
            if (File.Exists(filePath)) {
                Debug.Log("Updating Spawns file: " + filePath);
                existing = (Dictionary<string, Dictionary<string, SpawnPoint>>)SystemTools.LoadFromFile(filePath);
                existing[sceneName] = spawnDict;
            }
            else {
                Debug.Log("Creating Spawns file: " + filePath);
                existing = new Dictionary<string, Dictionary<string, SpawnPoint>> () { { sceneName, spawnDict } };
            }
            SystemTools.SaveToFile(existing, filePath);
        }

        static string GetNameWParent (Transform t, string name) {
            if (t.parent == null)
                return name;
            return GetNameWParent(t.parent, t.parent.name + "." + name);
        }

        static void AddSpawnPoints (SceneSpawnPoint spawn, string scene, List<SpawnPoint> pointSpawns, List<SpawnPoint> allSpawns) {
            Transform t = spawn.transform;
            string baseName = GetNameWParent(t, t.name);
            
            switch (spawn.type) {
                case SpawnPointType.Point:
                    allSpawns.Add(new SpawnPoint(scene, baseName, t));
                    break;
                case SpawnPointType.Group:
                    if (spawn.subSpawns.Length != 0) {
                        List<SpawnPoint> subSpawns = new List<SpawnPoint>();
                        List<string> groupNames = new List<string>();
                        for (int i = 0; i < spawn.subSpawns.Length; i++) 
                            groupNames.Add(subSpawns.AddNew(new SpawnPoint(scene, baseName + "." + spawn.subSpawns[i].name, spawn.subSpawns[i].position, spawn.subSpawns[i].rotation)).name);
                        
                        allSpawns.Add(new SpawnPoint(scene, baseName, groupNames));
                        allSpawns.AddRange(subSpawns);
                    }
                    break;
                case SpawnPointType.Area:
                    allSpawns.Add(new SpawnPoint(scene, baseName, t.position, spawn.size, GetSpawnsInArea(spawn, t.position, pointSpawns)));
                    break;
            }
        }
        
        static List<string> GetSpawnsInArea(SceneSpawnPoint spawn, Vector3 pos, List<SpawnPoint> pointSpawns) {
            Bounds bounds = new Bounds(pos, spawn.size);
            List<string> spawnsInArea = new List<string>();
            for (int i = 0; i < pointSpawns.Count; i++) {
                if (bounds.Contains(pointSpawns[i].pos)) {
                    spawnsInArea.Add(pointSpawns[i].name);
                }
            }
            return spawnsInArea;
        }
    }
}