using System.Collections.Generic;
using UnityEngine;

using System;
namespace UnityTools {


    public abstract class Pool<T> where T : Component {
        protected T FindAvailableInstance (List<T> pool, Transform parentToSet, bool setActive, Vector3 position, Quaternion rotation) {
            for (int i = 0; i < pool.Count; i++) {
                T c = pool[i];
                if (!c.gameObject.activeSelf) {
                    Transform t = c.transform;

                    if (t.parent != parentToSet)
                        t.SetParent(parentToSet);
                    
                    if (parentToSet == null) {
                        t.position = position;
                        t.rotation = rotation;
                    }
                    else {
                        t.localPosition = position;
                        t.localRotation = rotation;
                    }

                    if (setActive)
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
        public T GetAvailable (T prefab, Transform parentToSet, bool setActive, Action<T> onCreateNew, Action<T, T> onRetrieve) {
            return GetAvailable(prefab, parentToSet, setActive, Vector3.zero, Quaternion.identity, onCreateNew, onRetrieve);
        }
        public void AddManualInstance (T prefab, T instance, Action<T> onCreateNew) {
            List<T> prefabPool;
            if (pool.TryGetValue(prefab.GetInstanceID(), out prefabPool)) {
                if (!prefabPool.Contains(instance)) {

                    MonoBehaviour.DontDestroyOnLoad(instance.gameObject);
                    prefabPool.Add(instance);
                    if (onCreateNew != null) {
                        onCreateNew(instance);
                    }
                }
            }
            else {
                MonoBehaviour.DontDestroyOnLoad(instance.gameObject);
                pool[prefab.GetInstanceID()] = new List<T>() { instance };
                if (onCreateNew != null) {        
                    onCreateNew(instance);
                }
            }
        }
        public virtual T GetAvailable (T prefab, Transform parentToSet, bool setActive, Vector3 position, Quaternion rotation, Action<T> onCreateNew, Action<T, T> onRetrieve) {
            List<T> prefabPool;
            T r = null;
            if (pool.TryGetValue(prefab.GetInstanceID(), out prefabPool)) {
                r = FindAvailableInstance(prefabPool, parentToSet, setActive, position, rotation);
                if (r == null) r = CreateInstance(prefab, prefabPool, position, rotation, onCreateNew);
                
                if (onRetrieve != null) {
                    onRetrieve(prefab, r);
                }
                return r;
            }
            
            prefabPool = new List<T>();
            pool[prefab.GetInstanceID()] = prefabPool;
            r = CreateInstance(prefab, prefabPool, position, rotation, onCreateNew);
           
            if (onRetrieve != null) {
                onRetrieve(prefab, r);
            }
                
            return r;
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

        public T GetAvailable (Transform parentToSet, bool setActive) {
            return GetAvailable(parentToSet, setActive, Vector3.zero, Quaternion.identity);
        }
        public T GetAvailable (Transform parentToSet, bool setActive, Vector3 position, Quaternion rotation) {
            T r = FindAvailableInstance(pool, parentToSet, setActive, position, rotation);
            if (r == null) r = CreateInstance();
            return r;
        }
    }
}
