using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

/*
    framework for saving the state of specified components on a per scene basis

    e.g. where npcs were when going to a different scene
    or where certain objects where dropped / if they were picked up


    the system should use pooling, which instantiates whatever copies it needs,
    so if any attached components need to save their state, they should
    implement the 
        ISaveableAttachment interface

    e.g. and inventory component on npcs

    then when loading or saving the base object (npc component), it calls the load or save functionality 
    for all ISaveableAttachment components

    this way inventory loads dont try to instantiate copies that are already 
    instantiated by npc loading...
*/
    
namespace UnityTools {
    
    /*
        base for the saved representation of the objects to save
    */
    [Serializable] public abstract class SceneObjectState {

        // the saved state of all attached components that needs to be saved/loaded
        public AttachmentState[] attachmentStates;

        public sVector3 position;
        public sQuaternion rotation;

        public SceneObjectState(Component c) {
            
            // get all saveable information from attached scripts, 
            // maybe a scene item has an inventory or otehr ocmponents thhat need to save their states
            ISaveableAttachment[] saveables = c.GetComponents<ISaveableAttachment>();
            
            attachmentStates = new AttachmentState[saveables.Length];
            for (int x = 0; x < saveables.Length; x++) {
                attachmentStates[x] = new AttachmentState(saveables[x].AttachmentType(), saveables[x].OnSaved());
            }
        

            this.position = c.transform.position;
            this.rotation = c.transform.rotation;
        }
    }

    [Serializable] public class AttachmentState {
        public Type type;
        public object state;
        public AttachmentState (Type type, object state) {
            this.type = type;
            this.state = state;
        }
    }


    public interface ISaveableAttachment
    {
        Type AttachmentType ();
        object OnSaved ();
        void OnLoaded (object savedAttachmentInfo);
    }

    public interface ISaveableObject<S> where S : SceneObjectState
    {
        void LoadFromSavedObject(S savedObject);
        void WarpTo (Vector3 position, Quaternion rotation);
    }
        

    /*
        handle saving of all objects in a scene of type
    */
    // C = component targeted
    // S = saved component type
    // T = parent type (for singleton)
    public abstract class SaveLoadObjectSceneHandler<C, S, T> : Singleton<T> 
        where C : MonoBehaviour
        where S : SceneObjectState
        where T : MonoBehaviour 
    {

        protected void OnObjectLoad (C loadedObject, ISaveableObject<S> loadedObjectAsSaveableObject, S savedObject) {

            loadedObjectAsSaveableObject.WarpTo(savedObject.position, savedObject.rotation);
            loadedObjectAsSaveableObject.LoadFromSavedObject(savedObject);

            // load all saveable information to attached scripts, 
            // maybe a scene item has an inventory or otehr ocmponents thhat need to save their states
            ISaveableAttachment[] saveables = loadedObject.GetComponents<ISaveableAttachment>();
            for (int i = 0; i < saveables.Length; i++) {
                Type saveableType = saveables[i].AttachmentType();
                for (int x = 0; x < savedObject.attachmentStates.Length; x++) {
                    if (saveableType == savedObject.attachmentStates[x].type) {
                        saveables[i].OnLoaded(savedObject.attachmentStates[x].state);
                        break;
                    }
                }
            }
        }

        protected abstract void SaveObjects (C[] activeObjects, List<S> savedObjects, bool manualSave);
        protected abstract void LoadObjects (C[] activeObjects, List<S> savedObjects, bool loadingSaveSlot);
        
        void OnEnable () 
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneLoading.onSceneExit += OnSceneExit;
            
            SaveLoad.onSaveGame += OnSaveGame;
        }
     
        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneLoading.onSceneExit -= OnSceneExit;

            SaveLoad.onSaveGame -= OnSaveGame;
        }

        void SaveObjectsInScene (Scene scene, bool manualSave) {

            string objectKey = typeof(C).FullName;

            Debug.Log(objectKey + ": " + (manualSave ? "Manual Save" : "Unloaded Scene") + ": " + scene.name + "... updating running state state");

            // get all the active objects in the current scene
            C[] activeObjects = GameObject.FindObjectsOfType<C>();

            // teh list of saved objects to populate
            List<S> savedObjects = new List<S>();
            SaveObjects ( activeObjects, savedObjects, manualSave );

            // save the state by scene
            SaveLoad.UpdateSaveState (SceneKey(scene, objectKey), savedObjects);
        }


        void OnSaveGame (Scene scene) {
            SaveObjectsInScene ( scene, true );
        }

        void OnSceneExit (Scene scene) {

            if (GameManager.SceneIsNonSaveable(scene.name))
                return;

            // dont save if we're exiting scene from manual loading another scene
            if (SaveLoad.isLoadingSaveSlot)
                return;

            // save the objects in this scene if we're going to another one,

            // e.g we're going to an indoor area that's a different scene, then save the objects "outdoors"
            SaveObjectsInScene ( scene, false );
        }

        static string SceneKey (Scene scene, string suffix) {
            return scene.name + "." + suffix;
        }

        void OnSceneLoaded (Scene scene, LoadSceneMode mode)
        {
            string objectKey = typeof(C).FullName;
            
            string sceneKey = SceneKey(scene, objectKey);

            // if this scene has saved info for the objects, then load the objects
            if (SaveLoad.SaveStateContainsKey(sceneKey)) {
                
                // laod the scene objects states that were saved for this scene
                List<S> savedObjects = (List<S>)SaveLoad.LoadSavedObject(sceneKey);

                // get all the active objects that are default in the current scene
                C[] activeObjects = GameObject.FindObjectsOfType<C>();

                LoadObjects( activeObjects, savedObjects, SaveLoad.isLoadingSaveSlot );   
            }
        }
    }
}