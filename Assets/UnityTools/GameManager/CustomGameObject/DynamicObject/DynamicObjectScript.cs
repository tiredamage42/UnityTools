
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
            return DynamicObjectManager.GetAvailableInstance<T>(prefab.dynamicObject, position, rotation, null, true, true, false);           
        }
        public static T GetAvailableInstance (PrefabReference prefabRef, Vector3 position, Quaternion rotation) {
            return DynamicObjectManager.GetAvailableInstance<T>(prefabRef, position, rotation, null, true, true, false);           
        }
    }
}