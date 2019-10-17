using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools {
    public class KeyboardActionsInterface : SimpleUnityInputsInterface
    {
        [NeatArray] public NeatKeyCodeArray actions;
        protected override bool _GetActionDown (int action, int controller) {
            if (!CheckActionIndex("Action", action, actions.Length)) return false;
            return Input.GetKeyDown(actions[action]);
        }
        protected override bool _GetAction (int action, int controller) {
            if (!CheckActionIndex("Action", action, actions.Length)) return false;
            return Input.GetKey(actions[action]);
        }
        protected override bool _GetActionUp (int action, int controller) {
            if (!CheckActionIndex("Action", action, actions.Length)) return false;
            return Input.GetKeyUp(actions[action]);
        }
    }
}
