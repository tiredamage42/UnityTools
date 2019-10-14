using System.Collections.Generic;
using UnityEngine;

using System;
namespace UnityTools {

    public abstract class Pool<T> where T : Component {
        protected T FindAvailableInstance (List<T> pool) {
            for (int i = 0; i < pool.Count; i++) {
                T component = pool[i];
                
                if (!component.gameObject.activeSelf) {
                    component.transform.SetParent(null);
                    component.gameObject.SetActive(true);
                    return component;
                }
            }
            return null;
        }
        protected T FindAvailableInstance (List<T> pool, Vector3 position, Quaternion rotation) {
            for (int i = 0; i < pool.Count; i++) {
                T component = pool[i];
                if (!component.gameObject.activeSelf) {
                    component.transform.SetParent(null);
                    component.transform.position = position;
                    component.transform.rotation = rotation;
                    component.gameObject.SetActive(true);
                    return component;
                }
            }
            return null;
        }
    }

    // public interface IPoolable<T> where T : Component
    // {
    //     T basePrefab { get; set; }
    // }

    public class PrefabPool<T> : Pool<T> where T : Component {
        Dictionary<int, List<T>> pool = new Dictionary<int, List<T>>();

        // void InitializeNewInstance (T prefab, T instance) {
        //     // IPoolable<T> asPoolable = instance as IPoolable<T>;
        //     // if (asPoolable != null) asPoolable.basePrefab = prefab;
        //     MonoBehaviour.DontDestroyOnLoad(instance.gameObject);
        // }

        T CreateInstance (T prefab, List<T> pool, Vector3 position, Quaternion rotation, Action<T> onCreateNew) {
            T newInstance = GameObject.Instantiate(prefab, position, rotation);
            // InitializeNewInstance(prefab, newInstance);
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
