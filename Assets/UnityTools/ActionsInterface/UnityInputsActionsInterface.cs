using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools {
    
    public class UnityInputsActionsInterface : SimpleUnityInputsInterface
    {
        [NeatArray] public NeatStringArray actionButtons;
        [NeatArray] public NeatStringArray axisNames;

        public override string ConstructTooltip () {
            string r = GetType().Name + "\n\nActions:\n";
            for (int i = 0; i < actionButtons.Length; i++) r += i.ToString() + ": " + actionButtons[i] + "\n";
            r += "Axes:\n";
            for (int i = 0; i < axisNames.Length; i++) r += i.ToString() + ": " + axisNames[i] + "\n";
            return r;
        }
        
        protected override bool GetActionDown (int action, bool checkingAxis, int controller) {
            if (!CheckActionIndex("Action", action, checkingAxis ? axisNames.Length : actionButtons.Length)) return false;
            return Input.GetButtonDown(checkingAxis ? axisNames[action] : actionButtons[action]);
        }
        protected override bool GetAction (int action, bool checkingAxis, int controller) {
            if (!CheckActionIndex("Action", action, checkingAxis ? axisNames.Length : actionButtons.Length)) return false;
            return Input.GetButton(checkingAxis ? axisNames[action] : actionButtons[action]);
        }
        protected override bool GetActionUp (int action, bool checkingAxis, int controller) {
            if (!CheckActionIndex("Action", action, checkingAxis ? axisNames.Length : actionButtons.Length)) return false;
            return Input.GetButtonUp(checkingAxis ? axisNames[action] : actionButtons[action]);
        }
        protected override float GetAxis (int axis, int controller) {
            if (!CheckActionIndex("Axis", axis, axisNames.Length)) return 0;
            return Input.GetAxis(axisNames[axis]);
        }
        
    }
}
