using UnityEngine;
using UnityTools;
namespace UnityToolsDemo {

    [System.Serializable] public class SceneItemState : ObjectAttachmentState {
        public SceneItemState (SceneItem instance) { 

        }
    }

    public class SceneItem : DynamicObjectScript<SceneItem>, IObjectAttachment {
        
        public int InitializationPhase () {
            return 0;
        }

        public void InitializeDefault () {
                     
        }

        public void Strip () {
        
        }

        
        void Awake () {
            dynamicObject.AddAvailabilityForLoadCheck(
                () => true // not is equipped to inventory....
            );
        }

        public ObjectAttachmentState GetState () {
            return new SceneItemState (this);
        }

        public void LoadState (ObjectAttachmentState state) {
            SceneItemState itemState = state as SceneItemState; 

        }
    }
}
