using System.Collections.Generic;
using UnityEngine;

using UnityTools.EditorTools;
using UnityTools.Internal;
namespace UnityTools {
    
    public enum NumericalCheck { Equals = 0, NotEquals = 1, LessThan = 2, GreaterThan = 3, LessThanEqualTo = 4, GreaterThanEqualTo = 5 };
    [System.Serializable] public class Conditions : NeatArrayWrapper<Condition> { 

        
        public static bool ConditionsMet (Conditions conditions, Dictionary<string, object> checkObjects) {
        // public static bool ConditionsMet (Conditions conditions, GameObject subject, GameObject target) {
            
            if (conditions == null || conditions.Length == 0) return true;
            
            bool met = false;
            bool isOr = true;
            bool falseUntilNextOr = false;

            for (int i = 0; i < conditions.Length; i++) {
                
                // bool conditionMet = falseUntilNextOr ? false : conditions[i].IsTrue( subject, target );
                bool conditionMet = falseUntilNextOr ? false : conditions[i].IsTrue( checkObjects );

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
    
    [System.Serializable] public class Condition {

        public bool or;
        public RunTarget runTarget;


        public bool trueIfSubjectNull;
        public string runtimeSubjectName;
        public GameObject referenceTarget;
        public string callMethod;
        public float threshold; // or check against global game values
        public bool useGlobalValueThreshold;
        public string globalValueThresholdName;
        public NumericalCheck numericalCheck;
        [NeatArray] public Parameters parameters;


        protected bool CheckValue (float value, float threshold) {
            if      (numericalCheck == NumericalCheck.Equals) return value == threshold;
            else if (numericalCheck == NumericalCheck.NotEquals) return value != threshold;
            else if (numericalCheck == NumericalCheck.LessThan) return value < threshold;
            else if (numericalCheck == NumericalCheck.GreaterThan) return value > threshold;
            else if (numericalCheck == NumericalCheck.LessThanEqualTo) return value <= threshold;
            else if (numericalCheck == NumericalCheck.GreaterThanEqualTo) return value >= threshold;
            return false;
        }
        
        // public bool IsTrue (GameObject subject, GameObject target) {
        public bool IsTrue (Dictionary<string, object> checkObjects) {
            object[] suppliedParameters;

            
            // GameObject obj;
            // if (!Messaging.PrepareForMessageSend(callMethod, runTarget, subject, target, referenceTarget, parameters, out obj, out suppliedParameters))
            object obj;
            if (!Messaging.PrepareForMessageSend(callMethod, runTarget, checkObjects, runtimeSubjectName, referenceTarget, parameters, out obj, out suppliedParameters))
                return trueIfSubjectNull;
                // return false;
        
            float returnValue;
            if (runTarget == RunTarget.Static) {
                if (!Messaging.CallStaticMethod(callMethod, suppliedParameters, out returnValue))
                    return trueIfSubjectNull;
                    // return false;
            }
            else {
                if (!obj.CallMethod ( callMethod, suppliedParameters, out returnValue))
                    return trueIfSubjectNull;
                    // return false;
            }   

            float checkThreshold = threshold;
            if (useGlobalValueThreshold) {
                checkThreshold = GlobalGameValues.GetGlobalValue(globalValueThresholdName);
            }
            
            return CheckValue(returnValue, threshold);
        }
    }
}
