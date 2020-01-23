#if UNITY_EDITOR
using UnityEngine;
using System;

namespace UnityTools.EditorTools {

    public class UnityToolsEditor
    {
        public static bool prohibitProjectChangeCallback;
        static event Action projectChange;

        public static void AddProjectChangeListener (Action listener) {
            listener();
            projectChange += listener;
        }

        public static void OnProjectChange () {

            if (prohibitProjectChangeCallback)
                return;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (projectChange != null)
                projectChange();
            watch.Stop();
            Debug.Log("Calling Project Change Callback: " + watch.ElapsedMilliseconds + "ms");
        }
    }
}
#endif
