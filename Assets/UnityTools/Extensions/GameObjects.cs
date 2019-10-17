using System.Collections.Generic;
using UnityEngine;

using System.Reflection;
namespace UnityTools {

    public static class GameObjects 
    {


        public static T GetOrAddComponent<T> (this GameObject g) where T : Component {
            T r = g.GetComponent<T>();
            if (r == null) r = g.AddComponent<T>();
            return r;
        }
        public static T GetOrAddComponent<T> (this GameObject g, ref T variable) where T : Component {
            if (variable == null) variable = g.GetOrAddComponent<T>();
            return variable;
        }
        public static T GetComponentIfNull<T> (this GameObject g, ref T variable) where T : Component {
            if (variable == null) variable = g.GetComponent<T>();
            return variable;
        }    

        static Dictionary<int, Component[]> componentsPerGameObject = new Dictionary<int, Component[]>();
        public static bool CallMethod (this GameObject g, string callMethod, object[] parameters, out float value) {

            int id = g.GetInstanceID();
            Component[] components;
            if (!componentsPerGameObject.TryGetValue(id, out components)) {
                components = g.GetComponents<Component>();
                componentsPerGameObject[id] = components;
            }

            for (int i = 0; i < components.Length; i++)
                if (SystemTools.TryAndCallMethod ( components[i].GetType(), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, callMethod, components[i], parameters, out value, i == components.Length - 1, "Run Target: " + g.name))
                    return true;

            value = 0;
            return false;
        }   
    }
}
