using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools {
   
    public abstract class SimpleUnityInputsInterface : MonoBehaviour
    {
        [NeatArray] public NeatStringArray axisNames;

        void Awake () {
            if (ActionsInterface.InitializeActionsInterface (GetActionDown, GetAction, GetActionUp, GetAxis, GetMousePos, 1, this))
                DontDestroyOnLoad(gameObject);
        }

        protected abstract bool GetActionDown (int action, int controller);
        protected abstract bool GetAction (int action, int controller);
        protected abstract bool GetActionUp (int action, int controller);
        
        protected bool CheckActionIndex (string type, int action, int length) {
            if (action < 0 || action >= length) {
                Debug.LogWarning(type + ": " + action + " is out of range [" + length + "]");
                return false;
            }
            return true;
        }        
        float GetAxis (int axis, int controller) {
            if (!CheckActionIndex("Axis", axis, axisNames.Length)) return 0;
            return Input.GetAxis(axisNames[axis]);
        }
        Vector2 GetMousePos (int controller) {
            return Input.mousePosition;
        }
    }
}
