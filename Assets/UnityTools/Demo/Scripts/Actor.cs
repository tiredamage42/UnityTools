
using UnityEngine;

using UnityTools;
namespace UnityToolsDemo {

    [System.Serializable] public class ActorState : ObjectAttachmentState {
        public string actorName;
        public ActorState (Actor instance) { 
            this.actorName = instance.actorName;
        }       
    }

    // used for npc's or player
    [System.Serializable] public class Actor : MonoBehaviour, IObjectAttachment
    {

        public int InitializationPhase () {
            return 0;
        }

        public void InitializeDefault () {
                     
        }

        public void Strip () {
        
        }

        public ObjectAttachmentState GetState () {
            return new ActorState (this);
        }

        public void LoadState (ObjectAttachmentState state) {
            ActorState actorState = state as ActorState; 
            actorName = actorState.actorName;
        }

        public string actorName;
        
    }
}
