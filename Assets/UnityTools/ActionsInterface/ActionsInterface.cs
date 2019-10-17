using System.Collections.Generic;
using UnityEngine;

using System;

using UnityEditor;
using UnityTools.EditorTools;

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
        static Func<int, Vector2> getMousePos;

        static object interfaceInitializer;
        static bool inputFrozen;
        public static void FreezeInput (bool frozen) { inputFrozen = frozen; }
        public static void FreezeInput () { FreezeInput(true); }
        public static void UnfreezeInput () { FreezeInput(false); }
        
        public static bool InitializeActionsInterface (
            Func<int, int, bool> getActionDown, Func<int, int, bool> getAction, Func<int, int, bool> getActionUp, 
            Func<int, int, float> getAxis, Func<int, Vector2> getMousePos,
            int maxControllers, object interfaceInitializer
        ) {

            if (IsInitialized(false)) {
                Debug.Log("Actions Interface already initialized by " + interfaceInitializer.GetType());
                return false;
            }


            ActionsInterface.interfaceInitializer = interfaceInitializer;
            ActionsInterface.getActionDown = getActionDown;
            ActionsInterface.getAction = getAction;
            ActionsInterface.getActionUp = getActionUp;

            ActionsInterface.getAxis = getAxis;
            ActionsInterface.getMousePos = getMousePos;

            ActionsInterface.maxControllers = maxControllers;

            SceneLoading.prepareForSceneLoad += FreezeInput;
            SceneLoading.endSceneLoad += UnfreezeInput;

            return true;
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
            

        static bool IsInitialized (bool throwError = true) {
            if (getActionDown == null || getAction == null || getActionUp == null || getAxis == null || getMousePos == null) {
                if (throwError) Debug.LogError("ActionsInterface not initialized with action functions");
                return false;
            }
            return true;
        }
        public static bool GetActionDown (int action, int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || inputFrozen || (checkOccupied && ActionOccupied(action, controller))) return false;
            return getActionDown(action, controller);
        }
        public static bool GetAction (int action, int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || inputFrozen || (checkOccupied && ActionOccupied(action, controller))) return false;
            return getAction(action, controller);
        }
        public static bool GetActionUp (int action, int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || inputFrozen || (checkOccupied && ActionOccupied(action, controller))) return false;
            return getActionUp(action, controller);
        }
        public static float GetAxis (int axis, int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || inputFrozen || (checkOccupied && AxisOccupied(axis, controller))) return 0;
            return getAxis(axis, controller);
        }
        public static Vector2 GetMousePos (int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || inputFrozen || (checkOccupied && MouseAxisOccupied(controller))) return Vector2.zero;
            return getMousePos(controller);
        }
    }

    public abstract class ActionsInterfaceController : MonoBehaviour {
        void Awake () {
            if (ActionsInterface.InitializeActionsInterface (GetActionDown, GetAction, GetActionUp, GetAxis, GetMousePos, 1, this))
                DontDestroyOnLoad(gameObject);
        }

        protected abstract bool GetActionDown (int action, int controller);
        protected abstract bool GetAction (int action, int controller);
        protected abstract bool GetActionUp (int action, int controller);
        protected abstract float GetAxis (int axis, int controller);
        protected abstract Vector2 GetMousePos (int controller);
        public abstract string ConstructTooltip ();
    }

    public class ActionAttribute : PropertyAttribute { }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ActionAttribute))] 
    public class ActionAttributeDrawer : PropertyDrawer
    {
        static ActionsInterfaceController _sceneController;
        static ActionsInterfaceController sceneController {
            get {
                if (_sceneController == null) _sceneController = GameObject.FindObjectOfType<ActionsInterfaceController>();
                return _sceneController;
            }
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);
            if (sceneController != null) label.tooltip = sceneController.ConstructTooltip();
            EditorGUI.PropertyField(pos, prop, label, true);
            EditorGUI.EndProperty();
        }    
    }
    #endif

    
    
}