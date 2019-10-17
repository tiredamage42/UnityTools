using UnityEngine;
using UnityTools.EditorTools;
namespace UnityTools {
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
        protected override float GetAxis (int axis, int controller) {
            if (!CheckActionIndex("Axis", axis, axesPos.Length)) return 0;
            if (!CheckActionIndex("Axis", axis, axesNeg.Length)) return 0;
            float r = 0;
            if (Input.GetKey(axesPos[axis])) r += 1;
            if (Input.GetKey(axesNeg[axis])) r -= 1;
            return r;
        }
        
    }
}
