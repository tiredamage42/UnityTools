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


        static void PrepareArrayForCopy<T> (ref T[] a, int count) {
            if (a == null) 
                a = new T[count];
            else {
                if (a.Length != count)
                    System.Array.Resize(ref a, count);
            }
        }
        static List<T> PrepareListForCopy<T> (ref List<T> a) {
            if (a == null) 
                a = new List<T>();
            else 
                a.Clear();
            return a;
        }


        public static void MakeCopy <T> (this T[] s, ref T[] c) {

            int l = s.Length;
            PrepareArrayForCopy(ref c, l);
            for (int i = 0; i < l; i++)
                c[i] = s[i];
        }
        public static void MakeCopy <T> (this List<T> s, ref T[] c) {

            int l = s.Count;
            PrepareArrayForCopy(ref c, l);
            for (int i = 0; i < l; i++)
                c[i] = s[i];
        }

        public static void MakeCopy<T, K> (this Dictionary<K, T> s, ref T[] c) {
             int l = s.Count;
            PrepareArrayForCopy(ref c, l);
            
            int i = 0;
            foreach (var k in s.Keys) {
                c[i] = s[k];
                i++;
            }
        }

        public static void MakeCopy <T> (this T[] s, ref List<T> c) {
            PrepareListForCopy(ref c).AddRange(s);
        }
        public static void MakeCopy <T> (this List<T> s, ref List<T> c) {
            PrepareListForCopy(ref c).AddRange(s);
        }
        public static void MakeCopy<T, K> (this Dictionary<K, T> s, ref List<T> c) {
            PrepareListForCopy(ref c);            
            foreach (var k in s.Keys) 
                c.Add(s[k]);
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
