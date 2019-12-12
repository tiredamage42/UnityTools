using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityTools.Spawning;
using UnityEditor;

/*
    framework for saving the state of specified components on a per scene basis
    when loading or unloading scenes

    each object has two states, loaded and unloaded

    loaded is the actual gameObject in game representation (the npc model walking around)

    unloaded is a serializable class representation that can be saved and kept track of, without
    the need for having an instance in the scene (if it's out of range or in a different level)
    this unloaded version should contain enough information within it to rebuild the loaded representation
    from a newly instantiated instance.
    (the system uses pooling, to get an available copy to "load" into)


    this way we can keep track of objects even when they're not loaded or instantiated
    examples:

    - a spawned npc from a quest that lives in an unloaded level scene, 
    - the location and amount of physics items or npcs in a building we've left, 
        so that when we go back, it's as we left it


    if any attached components to the object that can be loaded/unloaded or need to save their state, they should
    implement the 
        IObjectAttachment interface

    e.g. and inventory component on npcs

    then when loading or unloading the base object (npc component), it calls the load or unload functionality 
    for all IObjectAttachment components attached to the object

    this way inventory loads dont try to instantiate copies that are already instantiated by npc loading...

*/

using UnityTools.Internal;
    
namespace UnityTools {

    
    /*
        serializable representation of the object script, includign all the states
        of the attachment scripts
    */
    [Serializable] public class DynamicObjectState : ObjectState {

        public PrefabReference prefabRef;
        public sVector3 position;
        public sQuaternion rotation;
        public string trackKey;
        public bool isTracked { get { return !string.IsNullOrEmpty(trackKey); } }
        public SpawnOptions spawnOptions;

    }
        
    public class DynamicObject : CustomGameObject {

        public void AddAvailabilityForLoadCheck (Func<bool> check) {
            availableForLoadChecks.Add(check);
        }
        public void RemoveAvailabilityForLoadCheck (Func<bool> check) {
            availableForLoadChecks.Remove(check);
        }
        List<Func<bool>> availableForLoadChecks = new List<Func<bool>>();
        public bool IsAvailableForLoad () {
            for (int i = availableForLoadChecks.Count - 1; i>=0; i--) {
                if (availableForLoadChecks[i] == null) {
                    availableForLoadChecks.RemoveAt(i);
                    continue;
                }
                if (!availableForLoadChecks[i]())
                    return false;    
            }

            Debug.Log(name + " is vaialbale for load");
            return true;
        }

        // handled by editor automation.....
        [HideInInspector] public PrefabReference prefabRef;

        protected override void OnEnable () {
            base.OnEnable();

            if (!isPlayer) 
                activeInstances.Add(this);
        }
        void OnDisable () {

            SetTracked(null);

            if (!isPlayer)
                activeInstances.Remove(this);
                
            if (isPlayer) {
                if (playerObject != null && playerObject == this) {
                    playerObject = null;
                }
            }
        }

        public static DynamicObject playerObject;
        bool isPlayer;

        public Vector3 GetPosition () {
            if (getPosition != null)
                return getPosition();
            return transform.position;
        }
        public Vector3 GetForward () {
            if (getForward != null)
                return getForward();
            return transform.forward;
        }
        Func<Vector3> getPosition, getForward;
        public void SetTransformGetters (Func<Vector3> getPosition, Func<Vector3> getForward) {
            this.getPosition = getPosition;
            this.getForward = getForward;
        }

        void Awake () {
            isPlayer = gameObject.CompareTag(GameManager.playerTag);

            if (isPlayer) {
                if (playerObject != null && playerObject != this) {
                    Debug.LogWarning("Copy of player playerObject in scene, deleting: " + name);
                    Destroy(gameObject);
                    return;
                }
                playerObject = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public bool MatchesState (DynamicObjectState state) {
            return state.spawnOptions == null
                && state.prefabRef.collection == prefabRef.collection 
                && state.prefabRef.name == prefabRef.name 
                && state.position == transform.position 
                && state.rotation == transform.rotation 
                && state.attachedStates.Count == attachments.Length
            ;
        }

        // if delayedInstantiate, we dont have anything to actually load or unload to state
        // serves as a delayed 'Instantiate' call
        // since we cant serialize prefab object references :/
        public void Load(DynamicObjectState state) 
        {
            _trackKey = state.trackKey;

            state.loadedVersion = this;

            if (state.spawnOptions == null) {
                LoadAttachedStates(state);
            }
        }

        
        public DynamicObjectState GetState ()
        {
            DynamicObjectState state = new DynamicObjectState();
            
            state.prefabRef = prefabRef;
            
            state.position = transform.position;
            state.rotation = transform.rotation;

            GetAttachedStates (state);
            return state;
        }

        public DynamicObjectState GetState (SpawnOptions spawnOptions, Vector3 position, Quaternion rotation)
        {
            // when delayed instantiate happens, we supply a custom position and rotation...
            DynamicObjectState state = new DynamicObjectState();
            state.prefabRef = prefabRef;
            state.position = position;
            state.rotation = rotation;

            state.spawnOptions = spawnOptions;
            return state;
        }   

        
        public void SetTracked (string trackKey) {
            _trackKey = trackKey;
        }

        string _trackKey;
        bool isInPool;

        public static readonly Action<DynamicObject> onPoolCreateAction = (o) => o.isInPool = true;

        public string trackKey { get { return _trackKey; } }
        public bool isTracked { get { return !string.IsNullOrEmpty(trackKey); } }
        

        static PrefabPool<DynamicObject> pool = new PrefabPool<DynamicObject>();
        static List<DynamicObject> activeInstances = new List<DynamicObject>();

        public static List<DynamicObject> GetInstancesNotInPool () {
            return activeInstances.Where(i => !i.isInPool).ToList();
        }
        public static List<DynamicObject> GetInstancesAvailableForLoad () {
            return activeInstances.Where(i => i.IsAvailableForLoad()).ToList();
        }


        public void AddInstanceToPool (bool disable) {
            if (isInPool) {
                Debug.LogWarning(name + " ( DynamicObject ) is already in pool...");
                return;
            }

            Debug.Log("Adding to pool: " + name);
            
            pool.AddManualInstance(PrefabReferenceCollection.GetPrefabReference<DynamicObject>(prefabRef), this, onPoolCreateAction);
            if (gameObject.activeSelf) {
                if (disable) {
                    gameObject.SetActive(false);
                }
            }
        }

        public static DynamicObject GetAvailableInstance (PrefabReference prefabRef, Vector3 position, Quaternion rotation) {
            return GetAvailableInstance(PrefabReferenceCollection.GetPrefabReference<DynamicObject>(prefabRef), position, rotation);
        }
        public static T GetAvailableInstance<T> (PrefabReference prefabRef, Vector3 position, Quaternion rotation) where T : Component{
            DynamicObject prefab = PrefabReferenceCollection.GetPrefabReference<DynamicObject>(prefabRef);
            if (prefab == null)
                return null;
            if (prefab.GetComponent<T>() == null)
                return null;
            return GetAvailableInstance(prefab, position, rotation).GetComponent<T>();
        }
        public static DynamicObject GetAvailableInstance (DynamicObjectState state) {
            return GetAvailableInstance(state.prefabRef, state.position, state.rotation);
        }
        public static DynamicObject GetAvailableInstance (DynamicObject prefab, Vector3 position, Quaternion rotation) {
            return pool.GetAvailable(prefab, null, true, position, rotation, onPoolCreateAction, null);
        }
        
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(DynamicObject))] class DynamicObjectEditor : Editor {
        public override void OnInspectorGUI() {
            // base.OnInspectorGUI();
        }
    }
    #endif
}
