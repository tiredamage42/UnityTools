using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using System;

namespace UnityTools {

    [Serializable] public abstract class ObjectAttachmentState { }
    public interface IObjectAttachment {
        void Strip ();
        void InitializeDefault ();
        int InitializationPhase ();
        void LoadState(ObjectAttachmentState state); 
        ObjectAttachmentState GetState ();
    }


    
    /*
        serializable representation of the object script, includign all the states
        of the attachment scripts
    */
    [Serializable] public class ObjectState {

        // the saved state of all attached components that needs to be saved/loaded
        public Dictionary<string, ObjectAttachmentState> attachedStates;
        [NonSerialized] public CustomGameObject loadedVersion = null;
        // public bool isLoaded { get { return loadedVersion != null; } }
        public bool isLoaded { get { return loadedVersion != null && loadedVersion.gameObject.activeSelf; } }

        public ObjectAttachmentState GetComponent (Type type) {

            foreach (var k in attachedStates.Values) {
                if (k.GetType() == type) {
                    return k;
                }
            }
            Debug.Log("Object STate Doesnt Contain Component of Type: " + type.Name);
            return null;
        }
    }

    public abstract class CustomGameObject : MonoBehaviour
    {
        // TODO: maybe serialize tags ?
        List<string> objectTags = new List<string>();
        public bool HasTag (string tag) {
            return objectTags.Contains(tag);
        }

        public void RemoveTags (List<string> tags) {
            for (int i = 0; i < tags.Count; i++) {
                objectTags.Remove(tags[i]);
            }
        }
        
        public void AddTags (List<string> tags) {
            objectTags.AddRange(tags);
            if (objectTags.Count > 25) {
                Debug.LogError(name + " tags getting bloated");
            }
        }

        IObjectAttachment[] _attachments;
        protected IObjectAttachment[] attachments { get { 
            if (_attachments == null || _attachments.Length == 0) 
                _attachments = GetComponents<IObjectAttachment>().OrderBy( (a) => a.InitializationPhase() ).ToArray();
            return _attachments;
        } }

        protected virtual void OnEnable () {
            for (int x = 0; x < attachments.Length; x++) {
                attachments[x].Strip();
            }
            StartCoroutine(InitializeAfterLoad());
        }

        const int waitFramesForInitialize = 1;

        IEnumerator InitializeAfterLoad () {
            for (int i = 0; i < waitFramesForInitialize; i++)
                yield return null;

            if (!loaded) {
                for (int x = 0; x < attachments.Length; x++)
                    attachments[x].InitializeDefault();
            }
            loaded = false;
        }

        bool loaded;
            

        protected void SetLoadedVersion (ObjectState state) {
            state.loadedVersion = this;
        }

        protected void GetAttachedStates (ObjectState state) {
            state.attachedStates = new Dictionary<string, ObjectAttachmentState>();            
            for (int x = 0; x < attachments.Length; x++) {
                Type t = attachments[x].GetType();
                // Debug.Log(name + ": Saving Attachment Type: " + t.Name);
                state.attachedStates[t.Name] = attachments[x].GetState(); 
            }
        }

        protected void LoadAttachedStates (ObjectState state) {
            // load all saveable information to attached scripts, 
            // maybe a scene item has an inventory or otehr ocmponents thhat need to save their states
            for (int i = 0; i < attachments.Length; i++) {    
                Type t = attachments[i].GetType();
                ObjectAttachmentState attachedState;
                if (state.attachedStates.TryGetValue(t.Name, out attachedState)) {
                    attachments[i].LoadState(attachedState);
                }
            }
            loaded = true;
        }

        

    }
}
