using UnityEngine;
using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;

namespace UnityTools {
    /*
        used for referencing prefabs by name, for serialization purposes
    */
    [CreateAssetMenu(menuName="Unity Tools/Prefabs/Prefab Reference Collection")]
    public class PrefabReferenceCollection : GameSettingsObject
    {
        [NeatArray] public NeatGameObjectArray prefabs;
        
        public T GetPrefabReference<T> (string name) where T : Component {
            GameObject g = GetPrefabReference(name);
            if (g != null) {
                T r = g.GetComponent<T>();
                if (r == null) {
                    Debug.LogError("Prefab named: " + name + " doesnt have component typeof: " + typeof(T).Name);
                }
                return r;
            }
            return null;
        }

        public GameObject GetPrefabReference (string name) {
            for (int i = 0; i < prefabs.Length; i++) {
                if (prefabs[i].name == name) {
                    return prefabs[i];
                }
            }
            Debug.LogError("Cant find prefab name: " + name);
            return null;
        }        

        public static T GetPrefabReference<T> (PrefabReference refInfo) where T : Component {
            PrefabReferenceCollection obj = GameSettings.GetSettings<PrefabReferenceCollection>(refInfo.collection);
            if (obj != null) return obj.GetPrefabReference<T>( refInfo.name );
            return null;
        }
        public static GameObject GetPrefabReference (PrefabReference refInfo) {
            PrefabReferenceCollection obj = GameSettings.GetSettings<PrefabReferenceCollection>(refInfo.collection);
            if (obj != null) return obj.GetPrefabReference( refInfo.name );
            return null;
        }
    }
}
