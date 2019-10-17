using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools {
    
    public class UnityInputsActionsInterface : SimpleUnityInputsInterface
    {
        [NeatArray] public NeatStringArray actionButtons;
        protected override bool _GetActionDown (int action, int controller) {
            if (!CheckActionIndex("Action", action, actionButtons.Length)) return false;
            return Input.GetButtonDown(actionButtons[action]);
        }
        protected override bool _GetAction (int action, int controller) {
            if (!CheckActionIndex("Action", action, actionButtons.Length)) return false;
            return Input.GetButton(actionButtons[action]);
        }
        protected override bool _GetActionUp (int action, int controller) {
            if (!CheckActionIndex("Action", action, actionButtons.Length)) return false;
            return Input.GetButtonUp(actionButtons[action]);
        }
    }
}
