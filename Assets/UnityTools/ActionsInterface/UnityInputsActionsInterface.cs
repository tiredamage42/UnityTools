using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools {
    
    public class UnityInputsActionsInterface : SimpleUnityInputsInterface
    {
        [NeatArray] public NeatStringArray actionButtons;
        protected override bool GetActionDown (int action, int controller) {
            if (!CheckActionIndex("Action", action, actionButtons.Length)) return false;
            return Input.GetButtonDown(actionButtons[action]);
        }
        protected override bool GetAction (int action, int controller) {
            if (!CheckActionIndex("Action", action, actionButtons.Length)) return false;
            return Input.GetButton(actionButtons[action]);
        }
        protected override bool GetActionUp (int action, int controller) {
            if (!CheckActionIndex("Action", action, actionButtons.Length)) return false;
            return Input.GetButtonUp(actionButtons[action]);
        }
    }
}
