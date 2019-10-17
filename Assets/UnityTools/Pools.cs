using System.Collections.Generic;
using UnityEngine;

using System;
namespace UnityTools {

    public abstract class Pool<T> where T : Component {
        protected T FindAvailableInstance (List<T> pool) {
            for (int i = 0; i < pool.Count; i++) {
                T c = pool[i];
                
                if (!c.gameObject.activeSelf) {
                    c.transform.SetParent(null);
                    c.gameObject.SetActive(true);
                    return c;
                }
            }
            return null;
        }
        protected T FindAvailableInstance (List<T> pool, Vector3 position, Quaternion rotation) {
            for (int i = 0; i < pool.Count; i++) {
                T c = pool[i];
                if (!c.gameObject.activeSelf) {
                    c.transform.SetParent(null);
                    c.transform.position = position;
                    c.transform.rotation = rotation;
                    c.gameObject.SetActive(true);
                    return c;
                }
            }
            return null;
        }
    }

    public class PrefabPool<T> : Pool<T> where T : Component {
        Dictionary<int, List<T>> pool = new Dictionary<int, List<T>>();

        T CreateInstance (T prefab, List<T> pool, Vector3 position, Quaternion rotation, Action<T> onCreateNew) {
            T newInstance = GameObject.Instantiate(prefab, position, rotation);
            MonoBehaviour.DontDestroyOnLoad(newInstance.gameObject);
            if (onCreateNew != null) onCreateNew(newInstance);
            pool.Add(newInstance);
            return newInstance;
        }

        public T GetAvailable (T prefab, Action<T> onCreateNew) {
            return GetAvailable(prefab, Vector3.zero, Quaternion.identity, onCreateNew);
        }
            
        public T GetAvailable (T prefab, Vector3 position, Quaternion rotation, Action<T> onCreateNew) {
            List<T> prefabPool;
            if (pool.TryGetValue(prefab.GetInstanceID(), out prefabPool)) {
                T r = FindAvailableInstance(prefabPool, position, rotation);
                if (r == null) r = CreateInstance(prefab, prefabPool, position, rotation, onCreateNew);
                return r;
            }
            
            prefabPool = new List<T>();
            pool[prefab.GetInstanceID()] = prefabPool;
            return CreateInstance(prefab, prefabPool, position, rotation, onCreateNew);
        }

        public void AddManualInstance (T prefab, T instance) {
            List<T> prefabPool;
            if (pool.TryGetValue(prefab.GetInstanceID(), out prefabPool)) {
                if (!prefabPool.Contains(instance)) {
                    MonoBehaviour.DontDestroyOnLoad(instance.gameObject);
                    prefabPool.Add(instance);
                }
            }
            else {
                MonoBehaviour.DontDestroyOnLoad(instance.gameObject);
                pool[prefab.GetInstanceID()] = new List<T>() { instance };
            }
        }
    }

    public class ComponentPool<T> : Pool<T> where T : Component {
        List<T> pool = new List<T>();

        T CreateInstance () {
            T newInstance = new GameObject(typeof(T).Name + "_pool_instance").AddComponent<T>();
            MonoBehaviour.DontDestroyOnLoad(newInstance.gameObject);
            pool.Add(newInstance);
            return newInstance;
        }

        public T GetAvailable () {
            T r = FindAvailableInstance(pool);
            if (r == null) r = CreateInstance();
            return r;
        }
    }
}
