using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools {
   
    public abstract class SimpleUnityInputsInterface : ActionsInterface
    {
        [NeatArray] public NeatStringArray axisNames;
        protected override int MaxControllers() { return 1; }
        
        protected bool CheckActionIndex (string type, int action, int length) {
            if (action < 0 || action >= length) {
                Debug.LogWarning(type + ": " + action + " is out of range [" + length + "]");
                return false;
            }
            return true;
        }        
        protected override float _GetAxis (int axis, int controller) {
            if (!CheckActionIndex("Axis", axis, axisNames.Length)) return 0;
            return Input.GetAxis(axisNames[axis]);
        }
        protected override Vector2 _GetMousePos (int controller) {
            return Input.mousePosition;
        }
    }
}
