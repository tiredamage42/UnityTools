﻿using UnityEngine;

namespace UnityTools {
    public class Singleton {

        public static T GetInstance<T> (ref T variable) where T : MonoBehaviour {
            if (variable == null) {
                variable = GameObject.FindObjectOfType<T>();
                if (variable == null) variable = new GameObject(typeof(T).Name + "_singleton").AddComponent<T>();
            }
            return variable;
        }
    }

    public class Singleton <T> : MonoBehaviour where T : MonoBehaviour {
        static T _instance;
        public static T instance { get { return Singleton.GetInstance<T>(ref _instance); } }

        protected virtual void Awake () {
            if (_instance != null && _instance != this) {
                Debug.Log("Multiple instances of " + typeof(T).Name + " singleton in the scene");
                Destroy (this.gameObject);
            }
        }
    }
}
