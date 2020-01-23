using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

using UnityTools.Internal;

/*
    be able to reference predefined locations between scenes
    (useful for spawn points, or naming locations, or fast travelling)
*/

    
namespace UnityTools {

    public enum LocationType { Point, Area, Group }
    
    public static class Locations {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnApplicationStart()
        {
            allLocations = LoadAllLocations ();
        }

        static Dictionary<string, Dictionary<string, LocationDefenition>> allLocations;

        static LocationDefenition GetLocation (Dictionary<string, Dictionary<string, LocationDefenition>> scene2locations, string scene, string name) {
            if (scene2locations != null) {
                if (scene2locations.TryGetValue(scene, out Dictionary<string, LocationDefenition> locations)) {
                    if (locations.TryGetValue(name, out LocationDefenition location)) 
                        return location;
                }
            }
            Debug.LogError("Location: No Location Named: " + name + " specified for scene: " + scene);
            return null;
        }

        public static bool GetLocationTransform (LocationKey key, out MiniTransform transform) {
            if (key == null) {
                transform = new MiniTransform(Vector3.zero);
                return false;
            }
            return GetLocationTransform(key.scene, key.name, out transform);
        }
        
        public static bool GetLocationTransform (string scene, string name, out MiniTransform transform) {
            LocationDefenition location = GetLocation(scene, name);
            if (location == null) {
                transform = new MiniTransform(Vector3.zero);
                return false;
            } 
            transform = location.GetTransform();
            return true;
        }
        public static bool GetLocationBounds (string scene, string name, out Bounds bounds) {
            LocationDefenition location = GetLocation(scene, name);
            if (location == null) {
                bounds = new Bounds();
                return false;
            } 
            bounds = location.GetBounds();
            return true;
        }

        public static LocationDefenition GetLocation (LocationKey key) {
            if (key != null) 
                return GetLocation(key.scene, key.name);
            return null;
        }
        
        public static LocationDefenition GetLocation (string scene, string name) {
            if (string.IsNullOrEmpty(scene)) {
                Debug.LogError("Location: GetLocation, scene is Null");
                return null;
            }
            if (string.IsNullOrEmpty(name)) {
                Debug.LogError("Location: GetLocation, location name is Null");
                return null;
            }

            if (!Application.isPlaying) 
                return GetLocation(LoadAllLocations(), scene, name);
            return GetLocation(allLocations, scene, name);
        }

        public static Dictionary<string, Dictionary<string, LocationDefenition>> LoadAllLocations () {
            string path = GetLocationsObjectPath();
            if (File.Exists(path))
                return (Dictionary<string, Dictionary<string, LocationDefenition>>)SystemTools.LoadFromFile(path);
            return null;
        }
        public static string GetLocationsObjectDirectory () {
            return Application.streamingAssetsPath + "/";
        }
        public static string GetLocationsObjectPath () {
            return GetLocationsObjectDirectory() + "Locations.lcs";    
        }
    }
    
    [Serializable] public class LocationDefenition
    {
        public string name, scene;
        public sVector3 pos, size;
        public float rot;
        public int type;
        public List<string> containedLocations = new List<string>();
        
        LocationDefenition GetRandomLocationPointContained () {
            return Locations.GetLocation(scene, containedLocations.GetRandom());
        }

        public Bounds GetBounds () {
            if ((LocationType)type == LocationType.Group)
                return GetRandomLocationPointContained().GetBounds();
            return new Bounds (pos, size);
        }

        MiniTransform GetAreaTransform () {
            if (containedLocations.Count > 0)
                return GetRandomLocationPointContained().GetTransform();
            return new MiniTransform(GetBounds().RandomPoint(), Vector3.zero);
        }

        public MiniTransform GetTransform () {
            switch ((LocationType)type) {
                case LocationType.Area:
                    return GetAreaTransform();

                // group isnt saved unless it has at least one sup location...
                case LocationType.Group:
                    return GetRandomLocationPointContained().GetTransform();
            }

            return new MiniTransform(pos, new Vector3(0, rot, 0));
        }

        void InitLocation (string scene, string name, Vector3 pos, float rot, LocationType type, Vector3 size, List<string> containedLocations) {
            this.scene = scene;
            this.name = name;
            this.pos = pos;
            this.type = (int)type;
            this.size = size;
            this.containedLocations = containedLocations;
            this.rot = rot;
        }

        public LocationDefenition (string scene, string name, Vector3 pos, Vector3 size, List<string> containedLocations) {
            InitLocation (scene, name, pos, 0, LocationType.Area, size, containedLocations);
        }
        public LocationDefenition (string scene, string name, List<string> containedLocations) {
            InitLocation (scene, name, Vector3.zero, 0, LocationType.Group, Vector3.one * 3, containedLocations);
        }
        public LocationDefenition (string scene, string name, Vector3 pos, float rot) {
            InitLocation (scene, name, pos, rot, LocationType.Point, Vector3.one * 3, null);
        }
        public LocationDefenition (string scene, string name, Transform transform) {
            InitLocation (scene, name, transform.position, transform.rotation.eulerAngles.y, LocationType.Point, Vector3.one * 3, null);
        }
    }
}
