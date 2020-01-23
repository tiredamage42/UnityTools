using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools {

    [CreateAssetMenu(menuName="Unity Tools/Action Interface Controllers/Keyboard Actions Controller", fileName="Keyboard_Actions_Controller")]
    public class KeyboardActionsInterface : SimpleUnityInputsInterface
    {
        [NeatArray] public NeatKeyCodeArray actions = new NeatKeyCodeArray(
            new KeyCode[] {
                KeyCode.Space,
                KeyCode.Mouse0,
                KeyCode.LeftShift,
            }
        );
        [NeatArray] public NeatKeyCodeArray axesPos = new NeatKeyCodeArray(
            new KeyCode[] {
                KeyCode.D,
                KeyCode.W,
                KeyCode.E,
                KeyCode.RightArrow,
                KeyCode.UpArrow
            }
        );
        [NeatArray] public NeatKeyCodeArray axesNeg = new NeatKeyCodeArray(
            new KeyCode[] {
                KeyCode.A,
                KeyCode.S,
                KeyCode.Q,
                KeyCode.LeftArrow,
                KeyCode.DownArrow
            }
        );

        public override string ConstructTooltip () {
            string r = GetType().Name + "\n\nActions:\n";
            for (int i = 0; i < actions.Length; i++) {
                r += i.ToString() + ": " + actions[i].ToString() + "\n";
            }   
            r += "Axes:\n";
            for (int i = 0; i < axesPos.Length; i++) {
                r += i.ToString() + ": -" + axesNeg[i].ToString() + ", +" + axesPos[i].ToString() + "\n";
            }   
            return r;
        }

        public override string Action2String (int action) {
            if (!CheckActionIndex("Action", action, actions.Length)) return null;
            return actions[action].ToString();
        }


        public override bool GetActionDown (int action, bool checkingAxis, int controller) {
            if (!CheckActionIndex("Action", action, checkingAxis ? axesPos.Length : actions.Length)) return false;
            if (checkingAxis && !CheckActionIndex("Action", action, axesNeg.Length)) return false;
            if (checkingAxis) return Input.GetKeyDown(axesNeg[action]) || Input.GetKeyDown(axesPos[action]);
            return Input.GetKeyDown(actions[action]);
        }
        public override bool GetAction (int action, bool checkingAxis, int controller) {
            if (!CheckActionIndex("Action", action, checkingAxis ? axesPos.Length : actions.Length)) return false;
            if (checkingAxis && !CheckActionIndex("Action", action, axesNeg.Length)) return false;
            if (checkingAxis) return Input.GetKey(axesNeg[action]) || Input.GetKey(axesPos[action]);
            return Input.GetKey(actions[action]);
        }
        public override bool GetActionUp (int action, bool checkingAxis, int controller) {
            if (!CheckActionIndex("Action", action, checkingAxis ? axesPos.Length : actions.Length)) return false;
            if (checkingAxis && !CheckActionIndex("Action", action, axesNeg.Length)) return false;
            if (checkingAxis) return Input.GetKeyUp(axesNeg[action]) || Input.GetKeyUp(axesPos[action]);
            return Input.GetKeyUp(actions[action]);
        }
        public override float GetAxis (int axis, int controller) {
            if (!CheckActionIndex("Axis", axis, axesPos.Length)) return 0;
            if (!CheckActionIndex("Axis", axis, axesNeg.Length)) return 0;
            float r = 0;
            if (Input.GetKey(axesPos[axis])) r += 1;
            if (Input.GetKey(axesNeg[axis])) r -= 1;
            return r;
        }
    }
}
