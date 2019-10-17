using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools {
   
    public abstract class SimpleUnityInputsInterface : ActionsInterfaceController
    {
        protected override Vector2 GetMousePos (int controller) {
            return Input.mousePosition;
        }
        protected bool CheckActionIndex (string type, int action, int length) {
            if (action < 0 || action >= length) {
                Debug.LogWarning(type + ": " + action + " is out of range [" + length + "]");
                return false;
            }
            return true;
        }        
    }
}
