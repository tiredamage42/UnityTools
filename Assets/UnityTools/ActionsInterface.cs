using System.Collections.Generic;
using UnityEngine;

using System;
namespace UnityTools {
    /*
        interface to standardize inputs with systems
        in case we're using custom inputs
        i.e. VR / custom input managers
    */

    class OccupiedInput {

        const int unoccupied = int.MinValue;
        // action, controller
        Dictionary<int, int> occupied = new Dictionary<int, int>();
        public void MarkOccupied(int action, int controller) {
            occupied[action] = controller;
        }
        public void MarkUnoccupied(int action) {
            occupied[action] = unoccupied;
        }
        public bool IsOccupied (int action, int controller) {            
            int controllerVal;
            if (occupied.TryGetValue(action, out controllerVal)) 
                return controllerVal != unoccupied && (controller == controllerVal || controller < 0);
            return false;
        }
    }
    public static class ActionsInterface 
    {
        //2 for vr, 1 for fps...
        public static int maxControllers = 1; 

        // action, controller
        static Func<int, int, bool> getActionDown, getAction, getActionUp;
        static Func<int, int, float> getAxis;
        static Func<int, int, Vector2> getMousePos;

        public static void InitializeActionsInterface (
            Func<int, int, bool> getActionDown, 
            Func<int, int, bool> getAction, 
            Func<int, int, bool> getActionUp, 
            Func<int, int, float> getAxis, 
            Func<int, int, Vector2> getMousePos,
            int maxControllers
        ) {
        
            ActionsInterface.getActionDown = getActionDown;
            ActionsInterface.getAction = getAction;
            ActionsInterface.getActionUp = getActionUp;

            ActionsInterface.getAxis = getAxis;
            ActionsInterface.getMousePos = getMousePos;

            ActionsInterface.maxControllers = maxControllers;
        }

        static OccupiedInput occupiedActions = new OccupiedInput();
        static OccupiedInput occupiedAxes = new OccupiedInput();
        static OccupiedInput occupiedMouseAxis = new OccupiedInput();

        public static void MarkActionOccupied(int action, int controller) { occupiedActions.MarkOccupied(action, controller); }
        public static void MarkActionUnoccupied(int action) { occupiedActions.MarkUnoccupied(action); }            
        public static bool ActionOccupied (int action, int controller) { return occupiedActions.IsOccupied(action, controller); }

        public static void MarkAxisOccupied(int axis, int controller) { occupiedAxes.MarkOccupied(axis, controller); }
        public static void MarkAxisUnoccupied(int axis) { occupiedAxes.MarkUnoccupied(axis); }
        public static bool AxisOccupied (int axis, int controller) { return occupiedAxes.IsOccupied(axis, controller); }

        public static void MarkMouseAxisOccupied(int controller) { occupiedMouseAxis.MarkOccupied(0, controller); }
        public static void MarkMouseAxisUnoccupied() { occupiedMouseAxis.MarkUnoccupied(0); }
        public static bool MouseAxisOccupied (int controller) { return occupiedMouseAxis.IsOccupied(0, controller); }
            

        static bool UninitializedCheck () {
            if (getActionDown == null || getAction == null || getActionUp == null || getAxis == null || getMousePos == null) {
                Debug.LogError("ActionsInterface not initialized with action functions");
                return true;
            }
            return false;
        }
        public static bool GetActionDown (int action, int controller) {
            if (UninitializedCheck()) return false;
            return getActionDown(action, controller);
        }
        public static bool GetAction (int action, int controller) {
            if (UninitializedCheck()) return false;
            return getAction(action, controller);
        }
        public static bool GetActionUp (int action, int controller) {
            if (UninitializedCheck()) return false;
            return getActionUp(action, controller);
        }
    }
}