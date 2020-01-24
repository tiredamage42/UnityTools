using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace UnityTools.EditorTools {
    public class EditorCoroutine : IDisposable {
        public static EditorCoroutine Start(IEnumerator routine) {
            return new EditorCoroutine(routine).Start();
        }
        static readonly FieldInfo WaitForSeconds_m_Seconds = typeof(UnityEngine.WaitForSeconds).GetField("m_Seconds", BindingFlags.Instance | BindingFlags.NonPublic);
        static IEnumerator WaitForSeconds(object subroutine) {
            var waitForSeconds = subroutine as UnityEngine.WaitForSeconds;
            if (waitForSeconds == null)
                return null;

            var seconds =(float)WaitForSeconds_m_Seconds.GetValue(waitForSeconds);
            return WaitForSeconds(seconds);
        }

        static IEnumerator WaitForSeconds(float seconds) {
            var end = DateTime.Now + TimeSpan.FromSeconds(seconds);
            do yield return null; while (DateTime.Now < end);
        }

        readonly Stack<IEnumerator> _stack = new Stack<IEnumerator>();

        public EditorCoroutine(IEnumerator routine) {
            Push(routine);
        }
        void Push(object subroutine) {
            Push(subroutine as IEnumerator ?? WaitForSeconds(subroutine));
        }

        void Push(IEnumerator subroutine) {
            if (subroutine != null) {
                // Debug.Log("Push To Stack");
                _stack.Push(subroutine);
            }
            else {
                // Debug.Log("Subroutine null");
            }
        }
        
        void IDisposable.Dispose() {
            Stop();
        }

        public EditorCoroutine Start() {
            if (_stack.Count > 0) {
                EditorApplication.update -= Update;
                EditorApplication.update += Update;
            }
            return this;
        }

        public void Stop() {
            EditorApplication.update -= Update;
        }

        void Update() {
            try { 
                MoveNext(); 
            }
            catch (Exception ex) { 
                Stop(); 
                Debug.LogError(ex); 
            }
        }
        void MoveNext() {
            // Debug.Log("Move Next");
            var routine = _stack.Peek();
            if (routine.MoveNext()) {
                // Debug.Log("Push In Move Next");
                Push(routine.Current);
            }
            else
                Pop();
        }
        void Pop() {
            _stack.Pop();
            if (_stack.Count == 0) 
                Stop();
        }
    }
}