using System.Collections.Generic;
using UnityEngine;

using UnityTools.EditorTools;
using UnityTools.Internal;
namespace UnityTools {
    
    public enum NumericalCheck { Equals = 0, NotEquals = 1, LessThan = 2, GreaterThan = 3, LessThanEqualTo = 4, GreaterThanEqualTo = 5 };
    public enum RunTarget { Subject, Target, Reference, Static };

    [System.Serializable] public class Conditions : NeatArrayWrapper<Condition> { 
        public static bool ConditionsMet (Conditions conditions, GameObject subject, GameObject target) {
            
            if (conditions == null || conditions.Length == 0) return true;
            
            bool met = false;
            bool isOr = true;
            bool falseUntilNextOr = false;

            for (int i = 0; i < conditions.Length; i++) {
                
                bool conditionMet = falseUntilNextOr ? false : conditions[i].IsTrue( subject, target );

                if (isOr)
                    met = met || conditionMet;
                else
                    met = met && conditionMet;
                
                isOr = conditions[i].or;
                
                if (isOr) {
                    // met conditions already
                    if (met) return true;
                    falseUntilNextOr = false;
                }

                else {
                    // and block is already false...
                    if (!met) falseUntilNextOr = true;
                }
            }
            return met;
        }
    }

}
namespace UnityTools.Internal {
    [System.Serializable] public class ConditionsParameters : NeatArrayWrapper<ConditionsParameter> { }

    public abstract class ConditionsParameter : ScriptableObject {
        public abstract object GetParamObject ();


        #if UNITY_EDITOR
        public abstract void DrawGUI (Rect pos);
        public abstract void DrawGUIFlat (Rect pos);
        public virtual float GetPropertyHeight () { 
            return GUITools.singleLineHeight; 
        }
        #endif
    }
    
    
    [System.Serializable] public class Condition {

        public bool or;
        public RunTarget runTarget;
        public GameObject referenceTarget;
        public string callMethod;
        public float threshold; // or check against global game values
        public bool useGlobalValueThreshold;
        public string globalValueThresholdName;
        public NumericalCheck numericalCheck;
        public ConditionsParameters parameters;
        public bool showParameters;


        protected bool CheckValue (float value, float threshold) {
            if      (numericalCheck == NumericalCheck.Equals) return value == threshold;
            else if (numericalCheck == NumericalCheck.NotEquals) return value != threshold;
            else if (numericalCheck == NumericalCheck.LessThan) return value < threshold;
            else if (numericalCheck == NumericalCheck.GreaterThan) return value > threshold;
            else if (numericalCheck == NumericalCheck.LessThanEqualTo) return value <= threshold;
            else if (numericalCheck == NumericalCheck.GreaterThanEqualTo) return value >= threshold;
            return false;
        }
        
        public bool IsTrue (GameObject subject, GameObject target) {
            if (string.IsNullOrEmpty(callMethod) || string.IsNullOrWhiteSpace(callMethod)) {
                Debug.LogWarning("Call Method is blank...");
                return false;
            }

            GameObject obj = subject;

            switch (runTarget) {
                case RunTarget.Subject: obj = subject; break;
                case RunTarget.Target: obj = target; break;
                case RunTarget.Reference: obj = referenceTarget; break;
                case RunTarget.Static: obj = null; break;
            }

            if (obj == null && runTarget != RunTarget.Static) {
                Debug.LogWarning("RunTarget: " + runTarget.ToString() + " is null, can't call condition method: " + callMethod);
                return false;
            }
            
            object[] suppliedParameters = new object[0];

            if (parameters.Length > 0) {

                List<object> parametersList = new List<object>();
                for (int i = 0; i < parameters.Length; i++) {
                    if (parameters[i] != null) 
                        parametersList.Add(parameters[i].GetParamObject());
                }
                suppliedParameters = parametersList.ToArray();
            }

            float returnValue;
            if (runTarget == RunTarget.Static) {
                if (!SystemTools.CallStaticMethod(callMethod, suppliedParameters, out returnValue))
                    return false;
            }
            else {
                if (!obj.CallMethod ( callMethod, suppliedParameters, out returnValue))
                    return false;
            }   

            float checkThreshold = threshold;
            if (useGlobalValueThreshold) {
                checkThreshold = GlobalGameValues.GetGlobalValue(globalValueThresholdName);
            }
            
            return CheckValue(returnValue, threshold);
        }
    }
}
