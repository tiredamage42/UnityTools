
using UnityEngine;
using System;

namespace UnityTools {


    public class DynamicObjectScript<T> : MonoBehaviour where T : DynamicObjectScript<T> {
        DynamicObject _dynamicObject;
        public DynamicObject dynamicObject { get { return this.GetComponentIfNull<DynamicObject>(ref _dynamicObject, false); } }

        public PrefabReference GetMyPrefabInformation () {
            return dynamicObject.prefabRef;
        }
        public static T GetAvailableInstance (T prefab, Vector3 position, Quaternion rotation) {
            return DynamicObject.GetAvailableInstance(prefab.dynamicObject, position, rotation).GetComponent<T> ();           
        }
        public static T GetAvailableInstance (PrefabReference prefabRef, Vector3 position, Quaternion rotation) {
            return DynamicObject.GetAvailableInstance<T>(prefabRef, position, rotation);           
        }
    }
}