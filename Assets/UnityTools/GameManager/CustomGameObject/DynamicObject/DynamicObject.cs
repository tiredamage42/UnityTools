

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
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

    
namespace UnityTools {

    /*
        serializable representation of the object script, includign all the states
        of the attachment scripts
    */
    [Serializable] public class DynamicObjectState : ObjectState {

        public PrefabReference prefabRef;
        public sVector3 position, rotation;
        public string scene, id;
        public bool permanent, delayedInstantiate, isUntouched;

        public DynamicObjectState (string id, string scene, PrefabReference prefabRef, bool permanent) {
            this.id = id;
            this.scene = scene;
            this.prefabRef = prefabRef;
            this.permanent = permanent;
        }
    }

        
    public class DynamicObject : CustomGameObject {

        Dictionary<Type, Component> cachedScripts = new Dictionary<Type, Component>();
        public T GetObjectScript<T> () where T : Component {
            Type t = typeof(T);

            Component script;
            if (cachedScripts.TryGetValue(t, out script)) {
                return script as T;
            }

            T i = GetComponent<T>();
            cachedScripts[t] = i;
            return i;
        }

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

            // Debug.Log(name + " is vaialbale for load");
            return true;
        }

        // handled by editor automation.....
        [HideInInspector] public PrefabReference prefabRef;

        protected override void OnEnable () {
            base.OnEnable();

            if (!_isPlayer) {
                activeInstances.Add(this);
            }
        }

        [HideInInspector] public Renderer[] renderers;
        [HideInInspector] public Collider[] colliders;
        void GetComponents() {
            renderers = GetComponentsInChildren<Renderer>();
            colliders = GetComponentsInChildren<Collider>();
        }
        

        void OnDisable () {

            // dont disable loaded version (since it's happening anyways)
            DynamicObjectManager.RemoveObjectByGUID(id, false, false); 
            
            id = null;

            if (!_isPlayer)
                activeInstances.Remove(this);
                
            if (_isPlayer) {
                if (playerObject != null && playerObject == this) {
                    playerObject = null;
                }
            }
        }

        public void SetID (string id) {        
            this.id = id;
        }
        public string GetID () {
            return id;
        }
        string id = null;

        // for editor placed alias models
        public bool usesAlias;
        public string aliasUsed;


        // should only return true when a non aliased obj is loaded with scene
        // if we instntiate objects we should set guids manually
        bool NeedsID () {
            bool idIsEmpty = string.IsNullOrEmpty(id);    
            if (usesAlias && idIsEmpty) 
                id = DynamicObjectManager.Alias2ID (aliasUsed);
            return idIsEmpty;
        }


        public static DynamicObject playerObject;
        bool _isPlayer;
        public bool isPlayer { get { return _isPlayer; } }

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
            _isPlayer = gameObject.CompareTag(GameManager.playerTag);

            if (_isPlayer) {
                if (playerObject != null && playerObject != this) {
                    Debug.LogWarning("Copy of player playerObject in scene, deleting: " + name);
                    Destroy(gameObject);
                    return;
                }
                playerObject = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                GetComponents ();
            }
        }

        // if delayedInstantiate, we dont have anything to actually load or unload to state
        // serves as a delayed 'Instantiate' call since we cant serialize prefab object references :/
        public void Load(DynamicObjectState state) 
        {

            SetID(state.id);
            
            SetLoadedVersion (state);

            if (!state.delayedInstantiate)
                LoadAttachedStates(state);

            state.delayedInstantiate = false;
        }

        
        public DynamicObjectState AdjustState (DynamicObjectState state, string scene, Vector3 position, Vector3 rotation, bool delayedInstantiate)
        {    
            state.isUntouched = false;

            state.prefabRef = prefabRef;
            state.scene = scene;

            state.position = position;
            state.rotation = rotation;

            state.delayedInstantiate = delayedInstantiate;
            
            if (!delayedInstantiate) {
                SetLoadedVersion (state);
                GetAttachedStates (state);
            }

            return state;
        }

        
        bool isInPool;

        void Start () {
            if (_isPlayer)
                return;

            if (isInPool) 
                return;
            
            Debug.Log("Adding to pool: " + name);
            DynamicObjectManager.pool.AddManualInstance(PrefabReferenceCollection.GetPrefabReference<DynamicObject>(prefabRef), this, onPoolCreateAction);
        }

        public static readonly Action<DynamicObject> onPoolCreateAction = (o) => o.isInPool = true;
        static List<DynamicObject> activeInstances = new List<DynamicObject>();

        public static List<DynamicObject> GetInstancesThatNeedID () {
            return activeInstances.Where(i => i.NeedsID()).ToList();
        }

        public static List<DynamicObject> GetInstancesAvailableForLoad () {
            return activeInstances.Where(i => i.IsAvailableForLoad()).ToList();
        }        
    }

    

    #if UNITY_EDITOR
    [CustomEditor(typeof(DynamicObject))] class DynamicObjectEditor : Editor {
        public override void OnInspectorGUI() {
            // base.OnInspectorGUI();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("usesAlias"), true);

            //TODO: use alias Picker
            if (serializedObject.FindProperty("usesAlias").boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("aliasUsed"), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}
