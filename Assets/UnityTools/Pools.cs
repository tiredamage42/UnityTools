using System.Collections.Generic;
using UnityEngine;

using System;
namespace UnityTools {

    public abstract class Pool<T> where T : Component {
        protected T FindAvailableInstance (List<T> pool) {
            for (int i = 0; i < pool.Count; i++) {
                if (!pool[i].gameObject.activeSelf) {
                    pool[i].transform.SetParent(null);
                    pool[i].gameObject.SetActive(true);
                    return pool[i];
                }
            }
            return null;
        }
    }

    public class PrefabPool<T> : Pool<T> where T : Component {
        Dictionary<int, List<T>> pool = new Dictionary<int, List<T>>();

        T CreateInstance (T prefab, List<T> pool, Action<T> onCreateNew) {
            T newInstance = GameObject.Instantiate(prefab);
            if (onCreateNew != null) onCreateNew(newInstance);
            pool.Add(newInstance);
            return newInstance;
        }

        public T GetAvailable (T prefab, Action<T> onCreateNew) {
            List<T> prefabPool;
            if (pool.TryGetValue(prefab.GetInstanceID(), out prefabPool)) {
                T r = FindAvailableInstance(prefabPool);
                if (r == null) r = CreateInstance(prefab, prefabPool, onCreateNew);
                return r;
            }
            
            prefabPool = new List<T>();
            pool[prefab.GetInstanceID()] = prefabPool;
            return CreateInstance(prefab, prefabPool, onCreateNew);
        }
    }

    public class ComponentPool<T> : Pool<T> where T : Component {
        List<T> pool = new List<T>();

        T CreateInstance () {
            T newInstance = new GameObject(typeof(T).Name + "_pool_instance").AddComponent<T>();
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
