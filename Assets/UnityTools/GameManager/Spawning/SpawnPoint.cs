using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
namespace UnityTools.Spawning {
    public enum SpawnPointType { Point, Area, Group }
    [Serializable] public class SpawnPoint
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void _LoadSpawns()
        {
            Debug.Log("SpawnPoint: Loading Spawns...");
            allSpawnPoints = LoadAllSpawnPoints ();
        }
        static Dictionary<string, Dictionary<string, SpawnPoint>> allSpawnPoints;

        static SpawnPoint GetSpawnPoint (Dictionary<string, Dictionary<string, SpawnPoint>> spawns, string scene, string name) {
            if (spawns != null) {
                Dictionary<string, SpawnPoint> points;
                if (spawns.TryGetValue(scene, out points)) {
                    SpawnPoint point;
                    if (points.TryGetValue(name, out point)) {
                        return point;
                    }
                }
            }

            Debug.LogError("SpawnPoint: No Spawn Point Named: " + name + " specified for scene: " + scene);
            return null;
        }

        public static SpawnPoint GetSpawnPoint (string scene, string name) {
            if (string.IsNullOrEmpty(scene)) {
                Debug.LogError("SpawnPoint: Get Spawn Point, scene is Null");
                return null;
            }
            if (string.IsNullOrEmpty(name)) {
                Debug.LogError("SpawnPoint: Get Spawn Point, spawn name is Null");
                return null;
            }

            if (!Application.isPlaying) 
                return GetSpawnPoint(LoadAllSpawnPoints(), scene, name);
            return GetSpawnPoint(allSpawnPoints, scene, name);
        }

        public static Dictionary<string, Dictionary<string, SpawnPoint>> LoadAllSpawnPoints () {
            string path = GetSpawnPointsObjectPath();
            if (File.Exists(path))
                return (Dictionary<string, Dictionary<string, SpawnPoint>>)SystemTools.LoadFromFile(path);
            return null;
        }

        public static string GetSpawnPointsObjectDirectory () {
            return Application.streamingAssetsPath + "/";
        }
        public static string GetSpawnPointsObjectPath () {
            return GetSpawnPointsObjectDirectory() + "SpawnPoints.spn";    
        }

        public string name, scene;
        public sVector3 pos, size;
        public float rot;
        public int type;
        public List<string> pointsInArea = new List<string>();
        
        SpawnPoint GetRandomSpawnPointContained () {
            return GetSpawnPoint(scene, pointsInArea.GetRandom());
        }

        public Bounds GetBounds () {
            if ((SpawnPointType)type == SpawnPointType.Group)
                return GetRandomSpawnPointContained().GetBounds();
            
            return new Bounds (pos, size);
        }

        MiniTransform GetAreaTransform () {
            if (pointsInArea.Count > 0)
                return GetRandomSpawnPointContained().GetTransform();
            return new MiniTransform(GetBounds().RandomPoint(), Vector3.zero);
        }

        public MiniTransform GetTransform () {
            switch ((SpawnPointType)type) {
                case SpawnPointType.Area:
                    return GetAreaTransform();

                // spawn group isnt saved unless it has at least one sup spawn...
                case SpawnPointType.Group:
                    return GetRandomSpawnPointContained().GetTransform();
            }

            return new MiniTransform(pos, new Vector3(0, rot, 0));
        }

        void InitializeSpawnPoint (string scene, string name, Vector3 pos, float rot, SpawnPointType type, Vector3 size, List<string> pointsInArea) {
            this.scene = scene;
            this.name = name;
            this.pos = pos;
            this.type = (int)type;
            this.size = size;
            this.pointsInArea = pointsInArea;
            this.rot = rot;
        }

        public SpawnPoint (string scene, string name, Vector3 pos, Vector3 size, List<string> pointsInArea) {
            InitializeSpawnPoint (scene, name, pos, 0, SpawnPointType.Area, size, pointsInArea);
        }
        public SpawnPoint (string scene, string name, List<string> pointsInArea) {
            InitializeSpawnPoint (scene, name, Vector3.zero, 0, SpawnPointType.Group, Vector3.one * 3, pointsInArea);
        }
        public SpawnPoint (string scene, string name, Vector3 pos, float rot) {
            InitializeSpawnPoint (scene, name, pos, rot, SpawnPointType.Point, Vector3.one * 3, null);
        }
        public SpawnPoint (string scene, string name, Transform transform) {
            InitializeSpawnPoint (scene, name, transform.position, transform.rotation.eulerAngles.y, SpawnPointType.Point, Vector3.one * 3, null);
        }
    }
}
