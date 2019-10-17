using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools {
    public class KeyboardActionsInterface : SimpleUnityInputsInterface
    {
        [NeatArray] public NeatKeyCodeArray actions;
        protected override bool GetActionDown (int action, int controller) {
            if (!CheckActionIndex("Action", action, actions.Length)) return false;
            return Input.GetKeyDown(actions[action]);
        }
        protected override bool GetAction (int action, int controller) {
            if (!CheckActionIndex("Action", action, actions.Length)) return false;
            return Input.GetKey(actions[action]);
        }
        protected override bool GetActionUp (int action, int controller) {
            if (!CheckActionIndex("Action", action, actions.Length)) return false;
            return Input.GetKeyUp(actions[action]);
        }
    }
}
