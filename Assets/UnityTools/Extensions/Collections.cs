using System.Collections.Generic;
using UnityEngine;

namespace UnityTools {
    public static class Collections 
    {
        public static T GetRandom<T> (this List<T> a) where T : class {
            return a.GetRandom<T>(null);
        }
        public static T GetRandom<T> (this T[] a) where T : class {
            return a.GetRandom<T>(null);
        }

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

        public static T[] MakeCopy <T> (this T[] s) {
            return (T[])s.Clone();
        }
        public static List<T> MakeCopy <T> (this List<T> s) {
            return new List<T>(s);
        }
        public static T Last <T> (this List<T> s, T defaultValue) {
            int c = s.Count;
            if (c == 0)
                return defaultValue;
            return s[c - 1];
        }
        public static T Last <T> (this List<T> s) where T : class {
            return s.Last<T>(null);
        }
        public static T AddNew <T> (this List<T> s, T item) {
            s.Add(item);
            return item;
        }

    }
}
