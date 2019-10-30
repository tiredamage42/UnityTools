﻿using UnityEngine;
namespace UnityTools {
    public class Singleton {
        public static T GetOrBuildInstance<T> (ref T variable) where T : MonoBehaviour {
            if (variable == null) {
                variable = GameObject.FindObjectOfType<T>();
                if (variable == null)   
                    variable = new GameObject(typeof(T).Name + "_singleton").AddComponent<T>();
            }
            return variable;
        }
        public static T GetInstance <T> (ref T variable) where T : MonoBehaviour {
            if (variable == null) variable = GameObject.FindObjectOfType<T>();
            if (variable == null) Debug.LogWarning("No instance of " + typeof(T).FullName + " in the scene");
            return variable;
        }
    }
    public class Singleton <T> : MonoBehaviour where T : MonoBehaviour {
        static T _instance;
        public static T instance { get { return Singleton.GetOrBuildInstance<T>(ref _instance); } }
        protected static void BuildInstanceIfNull () { T i = instance; }
        protected bool thisInstanceErrored;
        protected virtual void Awake () {
            if (_instance != null && _instance != this) {
                thisInstanceErrored = true;
                Debug.Log("Multiple instances of " + typeof(T).Name + " singleton in the scene. Destroying instance: " + gameObject.name);
                Destroy (gameObject);
            }
            else {
                //populate the instance variable...
                T i = instance;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}