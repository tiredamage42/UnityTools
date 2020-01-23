using System.Collections.Generic;
using UnityEngine;

using System;

using UnityEditor;
using UnityEngine.SceneManagement;

using UnityTools.Internal;
using UnityTools.GameSettingsSystem;
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
            if (action >= 0)
                occupied[action] = controller;
        }
        public void MarkUnoccupied(int action) {
            if (action >= 0)
                occupied[action] = unoccupied;
        }
        public bool IsOccupied (int action, int controller) {     
            if (action >= 0) {
                int controllerVal;
                if (occupied.TryGetValue(action, out controllerVal)) 
                    return controllerVal != unoccupied && (controller == controllerVal || controller < 0);
            }       
            return false;
        }
    }
    public static class ActionsInterface 
    {

        static ActionsInterfaceController control;
        public static bool isInitialized { get { return control != null; } }

        //2 for vr, 1 for fps...
        public static int maxControllers { get { return isInitialized ?  control.MaxControllers() : 1; } } 


        //2 for vr, 1 for fps...
        // public static int maxControllers = 1; 

        // action, controller
        // static Func<int, bool, int, bool> getActionDown, getAction, getActionUp;
        // static Func<int, int, float> getAxis;
        // static Func<int, Vector2> getMousePos, getMouseScrollDelta, getMouseAxis;
        // static Func<int, int, bool> getMouseButtonDown, getMouseButton, getMouseButtonUp;
        
        // static object interfaceInitializer;
        static bool inputFrozen;

        static void PrepareForSceneLoad (string scene, LoadSceneMode mode) { 
            if (mode != LoadSceneMode.Additive)
                FreezeInput(true); 
        }
        static void EndSceneLoad (string scene, LoadSceneMode mode) {
            if (mode != LoadSceneMode.Additive)
                FreezeInput(false);
        }
        
        public static void FreezeInput (bool frozen) { inputFrozen = frozen; }
        
        public static bool InitializeActionsInterface (
            ActionsInterfaceController control
            // Func<int, bool, int, bool> getActionDown, Func<int, bool, int, bool> getAction, Func<int, bool, int, bool> getActionUp, 
            // Func<int, int, float> getAxis, Func<int, Vector2> getMousePos, Func<int, Vector2> getMouseScrollDelta, Func<int, Vector2> getMouseAxis,
            // Func<int, int, bool> getMouseButtonDown, Func<int, int, bool> getMouseButton, Func<int, int, bool> getMouseButtonUp, 
            // int maxControllers, object interfaceInitializer
            
        ) {
            
            if (isInitialized) {
                Debug.Log("Actions Interface already initialized by " + control.GetType());
                return false;
            }
            ActionsInterface.control = control;

            // if (IsInitialized(false)) {
            //     Debug.Log("Actions Interface already initialized by " + interfaceInitializer.GetType());
            //     return false;
            // }


            // ActionsInterface.interfaceInitializer = interfaceInitializer;
            // ActionsInterface.getActionDown = getActionDown;
            // ActionsInterface.getAction = getAction;
            // ActionsInterface.getActionUp = getActionUp;


            // ActionsInterface.getAxis = getAxis;

            // ActionsInterface.getMousePos = getMousePos;
            // ActionsInterface.getMouseScrollDelta = getMouseScrollDelta;
            // ActionsInterface.getMouseAxis = getMouseAxis;

            
            // ActionsInterface.getMouseButtonDown = getMouseButtonDown;
            // ActionsInterface.getMouseButton = getMouseButton;
            // ActionsInterface.getMouseButtonUp = getMouseButtonUp;

            // ActionsInterface.maxControllers = maxControllers;

            SceneLoading.onSceneLoadStart += PrepareForSceneLoad;
            SceneLoading.onSceneLoadEnd += EndSceneLoad;

            return true;
        }

        static OccupiedInput occupiedActions = new OccupiedInput();
        public static void MarkActionOccupied(int action, int controller) { occupiedActions.MarkOccupied(action, controller); }
        public static void MarkActionUnoccupied(int action) { occupiedActions.MarkUnoccupied(action); }            
        public static bool ActionOccupied (int action, int controller) { return occupiedActions.IsOccupied(action, controller); }

        static OccupiedInput occupiedAxes = new OccupiedInput();
        public static void MarkAxisOccupied(int axis, int controller) { occupiedAxes.MarkOccupied(axis, controller); }
        public static void MarkAxisUnoccupied(int axis) { occupiedAxes.MarkUnoccupied(axis); }
        public static bool AxisOccupied (int axis, int controller) { return occupiedAxes.IsOccupied(axis, controller); }

        static OccupiedInput occupiedMouseAxis = new OccupiedInput();
        public static void MarkMouseAxisOccupied(int controller) { occupiedMouseAxis.MarkOccupied(0, controller); }
        public static void MarkMouseAxisUnoccupied() { occupiedMouseAxis.MarkUnoccupied(0); }
        public static bool MouseAxisOccupied (int controller) { return occupiedMouseAxis.IsOccupied(0, controller); }

        static OccupiedInput occupiedMouseScroll = new OccupiedInput();
        public static void MarkMouseScrollOccupied(int controller) { occupiedMouseScroll.MarkOccupied(0, controller); }
        public static void MarkMouseScrollUnoccupied() { occupiedMouseScroll.MarkUnoccupied(0); }
        public static bool MouseScrollOccupied (int controller) { return occupiedMouseScroll.IsOccupied(0, controller); }
            
        
        static OccupiedInput occupiedMouseButtons = new OccupiedInput();
        public static void MarkMouseButtonOccupied(int button, int controller) { occupiedMouseButtons.MarkOccupied(button, controller); }
        public static void MarkMouseButtonUnoccupied(int button) { occupiedMouseButtons.MarkUnoccupied(button); }
        public static bool MouseButtonOccupied (int button, int controller) { return occupiedMouseButtons.IsOccupied(button, controller); }

        

        static bool IsInitialized (bool throwError = true) {
            if (control == null) {
            // if (getActionDown == null || getAction == null || getActionUp == null || getAxis == null || getMousePos == null) {
                if (throwError) Debug.LogError("ActionsInterface not initialized with action functions");
                return false;
            }
            return true;
        }


        public static string Action2String (int action) {
            if (!IsInitialized() || action < 0) return null;
            return control.Action2String(action);
        }

        public static bool GetActionDown (int action, bool checkingAxis=false, int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || action < 0 || inputFrozen) return false;
            if (checkOccupied) {
                if (checkingAxis) {
                    if (AxisOccupied(action, controller)) return false;
                }
                else {
                    if (ActionOccupied(action, controller)) return false;
                }
            }
            // return getActionDown(action, checkingAxis, controller);
            return control.GetActionDown(action, checkingAxis, controller);
        }
        public static bool GetAction (int action, bool checkingAxis=false, int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || action < 0 || inputFrozen) return false;
            if (checkOccupied) {
                if (checkingAxis) {
                    if (AxisOccupied(action, controller)) return false;
                }
                else {
                    if (ActionOccupied(action, controller)) return false;
                }
            }
            
            // return getAction(action, checkingAxis, controller);
            return control.GetAction(action, checkingAxis, controller);
        }

        public static bool GetActionUp (int action, bool checkingAxis=false, int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || action < 0|| inputFrozen) return false;
            if (checkOccupied) {
                if (checkingAxis) {
                    if (AxisOccupied(action, controller)) return false;
                }
                else {
                    if (ActionOccupied(action, controller)) return false;
                }
            }
            
            // return getActionUp(action, checkingAxis, controller);
            return control.GetActionUp(action, checkingAxis, controller);
        }
        public static float GetAxis (int axis, int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || axis < 0 || inputFrozen || (checkOccupied && AxisOccupied(axis, controller))) return 0;
            // return getAxis(axis, controller);
            return control.GetAxis(axis, controller);
        }
        public static Vector2 GetMousePos (int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || inputFrozen || (checkOccupied && MouseAxisOccupied(controller))) return Vector2.zero;
            // return getMousePos(controller);
            return control.GetMousePos(controller);
        }
        public static Vector2 GetMouseAxis (int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || inputFrozen || (checkOccupied && MouseAxisOccupied(controller))) return Vector2.zero;
            // return getMouseAxis(controller);
            return control.GetMouseAxis(controller);
        }

        public static Vector2 GetMouseScrollDelta (int controller = 0, bool checkOccupied=true) {
            if (!IsInitialized() || inputFrozen || (checkOccupied && MouseScrollOccupied(controller))) return Vector2.zero;
            // return getMouseScrollDelta(controller);
            return control.GetMouseScrollDelta(controller);
        }

        public static bool GetMouseButtonDown (int action, int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || action < 0 || inputFrozen) return false;
            if (checkOccupied && MouseButtonOccupied(action, controller)) return false;
            // return getMouseButtonDown(action, controller);
            return control.GetMouseButtonDown(action, controller);
        }
        public static bool GetMouseButton (int action, int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || action < 0 || inputFrozen) return false;
            if (checkOccupied && MouseButtonOccupied(action, controller)) return false;
            // return getMouseButton(action, controller);
            return control.GetMouseButton(action, controller);
        }
        public static bool GetMouseButtonUp (int action, int controller=0, bool checkOccupied=true) {
            if (!IsInitialized() || action < 0|| inputFrozen) return false;
            if (checkOccupied && MouseButtonOccupied(action, controller)) return false;
            // return getMouseButtonUp(action, controller);
            return control.GetMouseButtonUp(action, controller);
        }
    }

    public abstract class ActionsInterfaceController : GameSettingsObject {
        
        public void InitializeActionsInterface () {
            ActionsInterface.InitializeActionsInterface (
                this
                // GetActionDown, GetAction, GetActionUp, GetAxis, 
                // GetMousePos, GetMouseScrollDelta, GetMouseAxis, 
                // GetMouseButtonDown, GetMouseButton, GetMouseButtonUp,
                // MaxControllers(), this
            );
        }
        protected bool CheckActionIndex (string type, int action, int length) {
            if (action < 0 || action >= length) {
                Debug.LogWarning(type + ": " + action + " is out of range [" + length + "]");
                return false;
            }
            return true;
        }      

        public abstract string Action2String (int action);  
        public abstract bool GetActionDown (int action, bool checkingAxis, int controller);
        public abstract bool GetAction (int action, bool checkingAxis, int controller);
        public abstract bool GetActionUp (int action, bool checkingAxis, int controller);
        public abstract float GetAxis (int axis, int controller);
        public abstract Vector2 GetMousePos (int controller);
        public abstract Vector2 GetMouseAxis (int controller);
        public abstract Vector2 GetMouseScrollDelta (int controller);
        public abstract bool GetMouseButtonDown (int button, int controller);
        public abstract bool GetMouseButton (int button, int controller);
        public abstract bool GetMouseButtonUp (int button, int controller);
        public abstract int MaxControllers ();
        public abstract string ConstructTooltip ();
    }


    public class ActionAttribute : PropertyAttribute { }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ActionAttribute))] class ActionAttributeDrawer : PropertyDrawer
    {
        static GameManagerSettings _settings;
        static GameManagerSettings settings {
            get {
                if (_settings == null) _settings = GameSettings.GetSettings<GameManagerSettings>();
                return _settings;
            }
        }
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            if (settings != null && settings.actionsController != null) 
                label.tooltip = settings.actionsController.ConstructTooltip();
            EditorGUI.PropertyField(pos, prop, label, true);
        }    
    }
    #endif
}