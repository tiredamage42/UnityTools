using UnityEngine;
using UnityTools;
using UnityTools.RandomEncounters;
namespace UnityToolsDemo {

    public class ScriptedEncounter : MonoBehaviour
    {
        public string moveObjectKey = "Spawn";
        RandomEncounter encounter;

        void Awake () {
            encounter = GetComponent<RandomEncounter>();
        }

        Vector3 originalPosition;
        Transform moveObject;
        
        void Start()
        {
            moveObject = null;

            object obj;
            if (encounter.GetSpawnedObject(moveObjectKey, out obj) == ObjectLoadState.Loaded) {
                moveObject = ((DynamicObject)obj).transform;
                originalPosition = moveObject.position;
            }

            // DynamicObject obj;
            // TrackedObjectState state = encounter.GetSpawnedObject(moveObjectKey, out obj, out _);
            // if (state == TrackedObjectState.Loaded) {
            //     moveObject = obj.transform;
            //     originalPosition = moveObject.position;
            // }
        }

        void Update()
        {
            if (moveObject != null)
                moveObject.position = originalPosition + new Vector3(Mathf.Sin(Time.time) * 1, 0, 0);   
        }
    }
}
