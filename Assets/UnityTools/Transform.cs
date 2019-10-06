// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools {

    /*
        for use in editor
    */
    [System.Serializable] public class MiniTransform {
        public Vector3 position, rotation, scale;
    }

    public static class TransformUtils 
    {

        public static void SetParent (this Transform transform, Transform parent, Vector3 localPosition) {
            if (transform.parent != parent) transform.SetParent(parent);
            transform.localPosition = localPosition;
        }
        public static void SetParent (this Transform transform, Transform parent, Vector3 localPosition, Quaternion localRotation) {
            transform.SetParent(parent, localPosition);
            transform.localRotation = localRotation;
        }
        public static void SetParent (this Transform transform, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale) {
            transform.SetParent(parent, localPosition, localRotation);
            transform.localScale = localScale;
        }


        public static void SetTransform (this Transform transform, MiniTransform settings, TransformBehavior behavior) {
            if (settings == null) return;
            if (behavior == null || behavior.position) transform.position = settings.position;
            if (behavior == null || behavior.rotation) transform.rotation = Quaternion.Euler(settings.rotation);
            if (behavior == null || behavior.scale) transform.localScale = settings.scale;
        }
        public static void SetTransform (this Transform transform, Transform parent, MiniTransform settings, TransformBehavior behavior) {
            if (settings == null) return;
            if (transform.parent != parent) transform.SetParent(parent);
            if (behavior == null || behavior.position) transform.localPosition = settings.position;
            if (behavior == null || behavior.rotation) transform.localRotation = Quaternion.Euler(settings.rotation);
            if (behavior == null || behavior.scale) transform.localScale = settings.scale;
        }

        public static void SetTransform (this Transform transform, TransformBehavior behavior, int index) {
            transform.SetTransform(GetTransform(behavior, index), behavior);
        }
        public static void SetTransform (this Transform transform, Transform parent, TransformBehavior behavior, int index) {
            transform.SetTransform(parent, GetTransform(behavior, index), behavior);
        }
        public static void SetTransform (this Transform transform, TransformBehavior behavior, string name) {
            transform.SetTransform(GetTransform(behavior, name), behavior);
        }
        public static void SetTransform (this Transform transform, Transform parent, TransformBehavior behavior, string name) {
            transform.SetTransform(parent, GetTransform(behavior, name), behavior);
        }

        static Dictionary<int, Dictionary<string, MiniTransform>> transformDictionaries = new Dictionary<int, Dictionary<string, MiniTransform>>();
        static MiniTransform GetTransform (TransformBehavior behavior, string name) {
            Dictionary<string, MiniTransform> dictionary;
            if (!transformDictionaries.TryGetValue(behavior.GetInstanceID(), out dictionary)) {

                dictionary = new Dictionary<string, MiniTransform>();

                for (int i = 0; i < behavior.transforms.Length; i++) 
                    dictionary.Add(behavior.transforms[i].name, behavior.transforms[i].transform);
                
                transformDictionaries[behavior.GetInstanceID()] = dictionary;
            }
            
            MiniTransform transform;
            if ( dictionary.TryGetValue(name, out transform) ) 
                return transform;
            
            Debug.LogWarning("Transform name " + name + " not found on Transform Behavior: " + behavior.name);
            return null;
        }
        static MiniTransform GetTransform (TransformBehavior behavior, int index) {
            if (index < 0 || index >= behavior.transforms.Length) {
                Debug.LogWarning("Index " + index + " is out of range on Transform Behavior: " + behavior.name);
                return null;
            }
            return behavior.transforms[index].transform;
            
        }
    }
}
