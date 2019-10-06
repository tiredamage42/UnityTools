// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools {
    public static class Collections 
    {
        
        public static T GetRandom<T> (this List<T> a, T defaultValue) {
            int c = a.Count;
            if (c == 0) return defaultValue;
            if (c == 1) return a[0];
            return a[Random.Range(0, c)];
        }
        public static T GetRandom<T> (this T[] a, T defaultValue) {
            int c = a.Length;
            if (c == 0) return defaultValue;
            if (c == 1) return a[0];
            return a[Random.Range(0, c)];
        }
    }
}
