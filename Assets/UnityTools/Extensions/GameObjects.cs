// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;


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
    }
}
