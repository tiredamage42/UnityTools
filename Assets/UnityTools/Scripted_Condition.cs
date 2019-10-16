using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityTools.EditorTools;
using UnityEditor;
using System.Reflection;

namespace UnityTools {


    // public enum RunTarget {
    //     Subject, Target, Reference
    // };
    public class Scripted_Condition //: Condition
    {
        // public RunTarget runTarget;
        // public GameObject referenceTarget;
        // public string callMethod;
        // public float threshold; // or check against global game values
        // public NumericalCheck numericalCheck;



        
        // public override float DrawGUI(Rect pos) {
            
        //     // useSuppliedValues = GUITools.DrawToggleButton(useSuppliedValues, useSecondaryParameterGUI, pos.x, pos.y, GUITools.blue, GUITools.white);
        //     // pos.x += GUITools.iconButtonWidth;
            
        //     // trueIfNoValue = GUITools.DrawToggleButton(trueIfNoValue, trueIfNoValueGUI, pos.x, pos.y, GUITools.blue, GUITools.white);
        //     // pos.x += GUITools.iconButtonWidth;
            
        //     pos.width = 75;
        //     runTarget = (RunTarget)EditorGUI.EnumPopup(pos, runTarget);
        //     pos.x += pos.width;

        //     if (runTarget == RunTarget.Reference) {
        //         pos.width = 100;
        //         referenceTarget = (GameObject)EditorGUI.ObjectField(pos, referenceTarget, typeof(GameObject), true);
        //         pos.x += pos.width;
        //     }
            
        //     pos.width = 100;
        //     callMethod = GUITools.StringFieldWithDefault (pos.x, pos.y, pos.width, pos.height, callMethod, "Call Method");
        //     pos.x += pos.width;

            
            
        //     numericalCheck = DrawNumericalCheck(ref pos, numericalCheck);

        //     pos.width = 60;
        //     threshold = EditorGUI.FloatField(pos, threshold);
            
        //     return 100 + 75 + numericalCheckWidth + 60 + (runTarget == RunTarget.Reference ? 100 : 0);
        // }        
                    


        // public override bool IsMet(GameObject subject, GameObject target) {

            // if (string.IsNullOrEmpty(callMethod) || string.IsNullOrWhiteSpace(callMethod)) {
            //     Debug.LogWarning("Call Method is blank...");
            //     return false;
            // }

            // GameObject obj = subject;

            // switch (runTarget) {
            //     case RunTarget.Subject:
            //         obj = subject;
            //         break;
            //     case RunTarget.Target:
            //         obj = target;
            //         break;
            //     case RunTarget.Reference:
            //         obj = referenceTarget;
            //         break;
            // }

            // if (obj == null) {
            //     Debug.LogWarning("RunTarget: " + runTarget.ToString() + " is null, can't call condition method: " + callMethod);
            //     return false;
            // }
            
            // object[] paremeters = new object[] {
            //     // false,  // whether the message has been called yet
            //     // "",  // was there an error ?
            //     // 0.0f,   // the return value of teh method called
            // };
            // // obj.SendMessage("_condition_" + callMethod, messageContext, SendMessageOptions.DontRequireReceiver);

            // float returnValue;
            // // bool wasCalled = GetValueFromCallMethod (obj, "_condition_" + callMethod, messageContext, out returnValue);
            // bool wasCalled = GetValueFromCallMethod (obj, callMethod, paremeters, out returnValue);

            // // bool wasCalled = (bool)messageContext[0];
            // if (!wasCalled) {

            //     string paramTypesNames = "";

            //     for (int i = 0; i < paremeters.Length; i++) {
            //         paramTypesNames += paremeters[i].GetType().Name + ", "
            //     }

            //     Debug.LogError("Run Target: " + obj.name + " does not contain a call method: '" + callMethod + "' with parameter types: (" + paramTypesNames + ")");
            //     return false;
            // }
            
            // // string error = (string)messageContext[0];//1];

            // // if (string.IsNullOrEmpty(error) && string.IsNullOrWhiteSpace(error)) {
            //     float runtimeValue = returnValue;// (float)messageContext[2];
            //     return CheckFloat(numericalCheck, runtimeValue, threshold);
            // // }
            // // else {
            // //     Debug.LogError("Error: " + callMethod + ":: " + error);
            // //     return false;
            // // }
            // // try {
            // // }
            // // catch {
            // //     Debug.LogError("Run Target: " + obj.name + " does not contain a call method: " + callMethod);
            // //     return false;
            // // }
        // }

        // static Dictionary<int, Component[]> componentsPerGameObject = new Dictionary<int, Component[]>();
        
        // static bool CheckParametersForMethod (MethodInfo method, object[] suppliedParameters) {
        //     ParameterInfo[] methodParams = method.GetParameters();

        //     if (methodParams.Length != suppliedParameters.Length)
        //         return false;

        //     for (int p = 0; p < methodParams.Length; p++) {
        //         System.Type methodType = methodParams[p].ParameterType;
        //         System.Type suppliedType = suppliedParameters[p].GetType();
        //         if (suppliedType != methodType && !suppliedType.IsSubclassOf(methodType)) 
        //             return false;
        //     }
        //     return true;
        // }
        
        // static bool GetValueFromCallMethod (GameObject onGameObject, string callMethod, object[] parameters, out float value) {

        //     int instanceID = onGameObject.GetInstanceID();
        //     Component[] allComponentsOnGameObject;
        //     if (!componentsPerGameObject.TryGetValue(instanceID, out allComponentsOnGameObject)) {
        //         allComponentsOnGameObject = onGameObject.GetComponents<Component>();
        //         componentsPerGameObject[instanceID] = allComponentsOnGameObject;
        //     }

        //     for (int i = 0; i < allComponentsOnGameObject.Length; i++) {
        //         Component c = allComponentsOnGameObject[i];
        //         System.Type componentType = c.GetType();

        //         MethodInfo[] allMethods = componentType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        //         for (int m = 0; m < allMethods.Length; m++) {
        //             MethodInfo method = allMethods[m];

        //             if (method.Name == callMethod) {
        //                 if (CheckParametersForMethod ( method, parameters )) {
        //                     if (method.ReturnType == typeof(float) || method.ReturnType == typeof(int)) {
        //                         value = (float)method.Invoke(c, parameters );
        //                         return true;
        //                     }
        //                     else if (method.ReturnType == typeof(bool)) {
        //                         value = ((bool)method.Invoke(c, parameters )) ? 1f : 0f;
        //                         return true;
        //                     }
        //                 }
        //             }
        //         }
        //     }

        //     value = 0;
        //     return false;

        // }
    }
}
