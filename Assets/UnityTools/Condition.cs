// using System.Collections.Generic;
// using UnityEngine;

// using UnityEditor;
// using UnityTools.EditorTools;


// namespace UnityTools {
    
//     public enum ConditionLink { And = 0, Or = 1 };
//     public enum NumericalCheck { Equals = 0, NotEquals = 1, LessThan = 2, GreaterThan = 3, LessThanEqualTo = 4, GreaterThanEqualTo = 5 };
        
    
//     [System.Serializable] public class Conditions : NeatArrayWrapper<Condition> { 
//         // public static bool ConditionsMet (Conditions conditions, Dictionary<string, object[]> parameters) {
//         public static bool ConditionsMet (Conditions conditions, GameObject subject, GameObject target) {
            
//             if (conditions == null || conditions.Length == 0) return true;
            
//             bool met = false;
//             ConditionLink link = ConditionLink.Or;

//             bool falseUntilNextOr = false;

//             for (int i = 0; i < conditions.Length; i++) {
//                 if (conditions[i] == null)
//                     continue;

//                 bool conditionMet = false;

//                 if (!falseUntilNextOr) {
//                     Condition condition = conditions[i];
//                     // string paramsKey = condition.ParametersKey();

//                     // object[] conditionParameters;
//                     // if (parameters.TryGetValue(paramsKey, out conditionParameters)) {

//                         // if (conditionParameters.Length != 0) {
//                             // conditionMet = conditions[i].IsMet( conditionParameters);   
//                             conditionMet = conditions[i].IsMet( subject, target );   
//                     //     }
//                     //     else {
//                     //         Debug.LogWarning("No condition parameters supplied for conditions key: " + paramsKey);
//                     //     }
//                     // }
//                     // else {
//                     //     Debug.LogWarning("No condition parameters supplied for conditions key: " + paramsKey);
//                     // }
//                 }
                

//                 if (link == ConditionLink.Or) 
//                     met = met || conditionMet;

//                 else if (link == ConditionLink.And) 
//                     met = met && conditionMet;
                
//                 link = conditions[i].link;

//                 if (link == ConditionLink.Or) {

//                     // met conditions already
//                     if (met) 
//                         return true;


//                     falseUntilNextOr = false;
//                 }

//                 else if (link == ConditionLink.And) {
//                     // and block is already false...
//                     if (!met) 
//                         falseUntilNextOr = true;
//                 }
//             }
//             return met;
//         }
//     }

//     [CustomPropertyDrawer(typeof(Conditions))] class ConditionsDrawer : NeatArrayAttributeDrawer
//     {
//         GUIContent chooseTypeContent = BuiltInIcons.GetIcon("ClothInspector.SelectTool", "Choose Condition Type");

//         public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
//         {

//             float indent1, indent2, indent2Width;
//             bool displayedValue;
//             StartArrayDraw ( pos, ref prop, ref label, out indent1, out indent2, out indent2Width, out displayedValue );

//             DrawAddElement ( pos, prop, indent1, displayedValue );
            
//             float xOffset = indent2 + GUITools.toolbarDividerSize;
//             DrawArrayTitle ( pos, prop, label, xOffset );
            
//             if (displayedValue) {
//                 Object baseObject = prop.serializedObject.targetObject;
//                 bool isAsset = AssetDatabase.Contains(baseObject);
                
                
                
//                 float y = pos.y + GUITools.singleLineHeight;

//                 Rect propRect = new Rect(xOffset + GUITools.iconButtonWidth + GUITools.toolbarDividerSize, y, (indent2Width - GUITools.toolbarDividerSize) - GUITools.iconButtonWidth, EditorGUIUtility.singleLineHeight);

//                 int indexToDelete = -1;

//                 for (int i = 0; i < prop.arraySize; i++) {

//                     if (GUITools.IconButton(indent1, y, deleteContent, GUITools.red))
//                         indexToDelete = i;
                    
//                     SerializedProperty p = prop.GetArrayElementAtIndex(i);
                    
//                     if (GUITools.IconButton(xOffset, y, chooseTypeContent, GUITools.white)) {
//                         System.Type[] conditionTypes = typeof(Condition).FindDerivedTypes(false);

//                         // int origType = -1;
//                         // if (p.objectReferenceValue != null) {
//                         //     for (int x = 0; x < conditionTypes.Length; x++) {
//                         //         if (p.objectReferenceValue.GetType() == conditionTypes[x]) {
//                         //             origType = x;
//                         //             break;
//                         //         }
//                         //     }
//                         // }

//                         GenericMenu menu = new GenericMenu();

//                         Vector2 mousePos = Event.current.mousePosition;
//                         menu.DropDown(new Rect(mousePos.x, mousePos.y, 0, 0));

//                         for (int x = 0; x < conditionTypes.Length; x++) {
//                             bool isType = p.objectReferenceValue != null && p.objectReferenceValue.GetType() == conditionTypes[x];
//                             menu.AddItem(
//                                 new GUIContent(conditionTypes[x].Name.Split('_')[0]), 
//                                 p.objectReferenceValue != null && p.objectReferenceValue.GetType() == conditionTypes[x],
//                                 // x == origType,
//                                 (newType) => {
//                                 // () => {
                                
//                                     if (!isType) {
//                                     // if ((int)newType != origType) {
//                                         Object.DestroyImmediate(p.objectReferenceValue, true);
//                                         // var newObj = ScriptableObject.CreateInstance(conditionTypes[x]);
//                                         // newObj.name = conditionTypes[x].Name;
//                                         var newObj = ScriptableObject.CreateInstance(conditionTypes[(int)newType]);
//                                         newObj.name = conditionTypes[(int)newType].Name;

//                                         if (isAsset) {
//                                             AssetDatabase.AddObjectToAsset(newObj, baseObject);
//                                             AssetDatabase.SaveAssets(); 
//                                         }

//                                         p.objectReferenceValue = newObj;
//                                         p.serializedObject.ApplyModifiedProperties();
//                                         EditorUtility.SetDirty(p.serializedObject.targetObject);
//                                     }

//                                 },
//                                 x
                            
//                             );
//                         }
//                         menu.ShowAsContext();
//                     }

//                     GUITools.DrawToolbarDivider(xOffset+GUITools.iconButtonWidth, y);
        
//                     if (p.objectReferenceValue != null) {
                        
//                         EditorGUI.BeginChangeCheck();
//                         float propWidth = ((Condition)p.objectReferenceValue).DrawGUI(propRect);
//                         if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(p.objectReferenceValue);


//                         GUITools.DrawToolbarDivider(propRect.x + propWidth + GUITools.toolbarDividerSize, y);
        
//                         bool isAnd = ((Condition)p.objectReferenceValue).link == ConditionLink.And;

//                         isAnd = GUITools.DrawToggleButton(isAnd, new GUIContent(isAnd ? "&&" : "||"), propRect.x + propWidth + GUITools.toolbarDividerSize * 2, y, GUITools.white, GUITools.white);
                        
//                         ((Condition)p.objectReferenceValue).link = isAnd ? ConditionLink.And : ConditionLink.Or;
                        
//                     }

                    
                    
//                     float h = EditorGUI.GetPropertyHeight(p, true);
                    
//                     y += h;
//                     propRect.y += h;
//                 }

                
//                 if (indexToDelete != -1) {
//                     SerializedProperty deleted = prop.GetArrayElementAtIndex(indexToDelete);
                    
//                     Object deletedObject = deleted.objectReferenceValue;
                    
//                     if (deletedObject != null) prop.DeleteArrayElementAtIndex(indexToDelete);
                    
//                     prop.DeleteArrayElementAtIndex(indexToDelete);
                    
//                     Object.DestroyImmediate(deletedObject, true);
                    
//                     if (isAsset) {
//                         AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(baseObject));
//                         AssetDatabase.SaveAssets(); 
//                     }                
//                 }
//             }

//             EditorGUI.EndProperty();
//         }
//     }

//     public abstract class Condition : ScriptableObject {

//         protected T CastParameter <T> (object parameter, T defaultValue) {
//             if (parameter == null) {
//                 Debug.LogWarning("Condition parameter null");
//                 return defaultValue;
//             }
//             try {
//                 T r = (T)parameter;
//                 return r;
//             }
//             catch {
//                 Debug.Log(parameter.ToString() + " (" + parameter.GetType().FullName + ") cannot be cast to: " + typeof(T).FullName);
//                 return defaultValue;
//             }
//         }
//         public ConditionLink link;

        
//         protected bool CheckInteger (NumericalCheck condition, int runtimeValue, int conditionThreshold) {
//             if (condition == NumericalCheck.Equals) return runtimeValue == conditionThreshold;
//             else if (condition == NumericalCheck.NotEquals) return runtimeValue != conditionThreshold;
//             else if (condition == NumericalCheck.LessThan) return runtimeValue < conditionThreshold;
//             else if (condition == NumericalCheck.GreaterThan) return runtimeValue > conditionThreshold;
//             else if (condition == NumericalCheck.LessThanEqualTo) return runtimeValue <= conditionThreshold;
//             else if (condition == NumericalCheck.GreaterThanEqualTo) return runtimeValue >= conditionThreshold;
//             return false;
//         }
//         protected bool CheckFloat (NumericalCheck condition, float runtimeValue, float conditionThreshold) {
//             if (condition == NumericalCheck.Equals) return runtimeValue == conditionThreshold;
//             else if (condition == NumericalCheck.NotEquals) return runtimeValue != conditionThreshold;
//             else if (condition == NumericalCheck.LessThan) return runtimeValue < conditionThreshold;
//             else if (condition == NumericalCheck.GreaterThan) return runtimeValue > conditionThreshold;
//             else if (condition == NumericalCheck.LessThanEqualTo) return runtimeValue <= conditionThreshold;
//             else if (condition == NumericalCheck.GreaterThanEqualTo) return runtimeValue >= conditionThreshold;
//             return false;
//         }
//         protected bool CheckFloatSqr (NumericalCheck condition, float runtimeValue, float conditionThreshold) {
//             return CheckFloat(condition, runtimeValue, conditionThreshold * conditionThreshold);
//         }

//         public abstract bool IsMet (GameObject subject, GameObject target);// object[] conditionParameters);
//         // public abstract string ParametersKey();

        
//         static readonly string[] numericalCheckOptions = new string[] { " == ", " != ", " < ", " > ", " <= ", " >= " };
//         protected const float numericalCheckWidth = 40;
//         protected NumericalCheck DrawNumericalCheck (ref Rect pos, NumericalCheck value) {
//             pos.width = numericalCheckWidth;
//             value = (NumericalCheck)EditorGUI.Popup (pos, (int)value, numericalCheckOptions);
//             pos.x += numericalCheckWidth;
//             return value;
//         }
        
//         public abstract float DrawGUI(Rect pos);
//         public virtual float GetPropertyHeight() { 
//             return GUITools.singleLineHeight; 
//         }
//     }
        
//     [CustomPropertyDrawer(typeof(Condition))] class ConditionDrawer : PropertyDrawer {
//         public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
//             if (prop.objectReferenceValue == null) return GUITools.singleLineHeight;
//             return ((Condition)prop.objectReferenceValue).GetPropertyHeight();
//         }
//     }
// }








using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityTools.EditorTools;
using System.Reflection;

namespace UnityTools {
    
    public enum ConditionLink { And = 0, Or = 1 };
    public enum NumericalCheck { Equals = 0, NotEquals = 1, LessThan = 2, GreaterThan = 3, LessThanEqualTo = 4, GreaterThanEqualTo = 5 };
        
    
    [System.Serializable] public class Conditions : NeatArrayWrapper<Condition> { 
        public static bool ConditionsMet (Conditions conditions, GameObject subject, GameObject target) {
            
            if (conditions == null || conditions.Length == 0) return true;
            
            bool met = false;
            ConditionLink link = ConditionLink.Or;

            bool falseUntilNextOr = false;

            for (int i = 0; i < conditions.Length; i++) {
                
                bool conditionMet = false;

                if (!falseUntilNextOr) {
                    Condition condition = conditions[i];
                    conditionMet = conditions[i].IsMet( subject, target );   
                }
                

                if (link == ConditionLink.Or) 
                    met = met || conditionMet;

                else if (link == ConditionLink.And) 
                    met = met && conditionMet;
                
                link = conditions[i].or ? ConditionLink.Or : ConditionLink.And;//.link;

                if (link == ConditionLink.Or) {

                    // met conditions already
                    if (met) 
                        return true;


                    falseUntilNextOr = false;
                }

                else if (link == ConditionLink.And) {
                    // and block is already false...
                    if (!met) 
                        falseUntilNextOr = true;
                }
            }
            return met;
        }
    }

    // [CustomPropertyDrawer(typeof(Conditions))] class ConditionsDrawer : NeatArrayAttributeDrawer
    // {
    //     GUIContent chooseTypeContent = BuiltInIcons.GetIcon("ClothInspector.SelectTool", "Choose Condition Type");

    //     public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    //     {

    //         float indent1, indent2, indent2Width;
    //         bool displayedValue;
    //         StartArrayDraw ( pos, ref prop, ref label, out indent1, out indent2, out indent2Width, out displayedValue );

    //         DrawAddElement ( pos, prop, indent1, displayedValue );
            
    //         float xOffset = indent2 + GUITools.toolbarDividerSize;
    //         DrawArrayTitle ( pos, prop, label, xOffset );
            
    //         if (displayedValue) {
    //             Object baseObject = prop.serializedObject.targetObject;
    //             bool isAsset = AssetDatabase.Contains(baseObject);
                
                
                
    //             float y = pos.y + GUITools.singleLineHeight;

    //             Rect propRect = new Rect(xOffset + GUITools.iconButtonWidth + GUITools.toolbarDividerSize, y, (indent2Width - GUITools.toolbarDividerSize) - GUITools.iconButtonWidth, EditorGUIUtility.singleLineHeight);

    //             int indexToDelete = -1;

    //             for (int i = 0; i < prop.arraySize; i++) {

    //                 if (GUITools.IconButton(indent1, y, deleteContent, GUITools.red))
    //                     indexToDelete = i;
                    
    //                 SerializedProperty p = prop.GetArrayElementAtIndex(i);
                    
    //                 if (GUITools.IconButton(xOffset, y, chooseTypeContent, GUITools.white)) {
    //                     System.Type[] conditionTypes = typeof(Condition).FindDerivedTypes(false);

    //                     // int origType = -1;
    //                     // if (p.objectReferenceValue != null) {
    //                     //     for (int x = 0; x < conditionTypes.Length; x++) {
    //                     //         if (p.objectReferenceValue.GetType() == conditionTypes[x]) {
    //                     //             origType = x;
    //                     //             break;
    //                     //         }
    //                     //     }
    //                     // }

    //                     GenericMenu menu = new GenericMenu();

    //                     Vector2 mousePos = Event.current.mousePosition;
    //                     menu.DropDown(new Rect(mousePos.x, mousePos.y, 0, 0));

    //                     for (int x = 0; x < conditionTypes.Length; x++) {
    //                         bool isType = p.objectReferenceValue != null && p.objectReferenceValue.GetType() == conditionTypes[x];
    //                         menu.AddItem(
    //                             new GUIContent(conditionTypes[x].Name.Split('_')[0]), 
    //                             p.objectReferenceValue != null && p.objectReferenceValue.GetType() == conditionTypes[x],
    //                             // x == origType,
    //                             (newType) => {
    //                             // () => {
                                
    //                                 if (!isType) {
    //                                 // if ((int)newType != origType) {
    //                                     Object.DestroyImmediate(p.objectReferenceValue, true);
    //                                     // var newObj = ScriptableObject.CreateInstance(conditionTypes[x]);
    //                                     // newObj.name = conditionTypes[x].Name;
    //                                     var newObj = ScriptableObject.CreateInstance(conditionTypes[(int)newType]);
    //                                     newObj.name = conditionTypes[(int)newType].Name;

    //                                     if (isAsset) {
    //                                         AssetDatabase.AddObjectToAsset(newObj, baseObject);
    //                                         AssetDatabase.SaveAssets(); 
    //                                     }

    //                                     p.objectReferenceValue = newObj;
    //                                     p.serializedObject.ApplyModifiedProperties();
    //                                     EditorUtility.SetDirty(p.serializedObject.targetObject);
    //                                 }

    //                             },
    //                             x
                            
    //                         );
    //                     }
    //                     menu.ShowAsContext();
    //                 }

    //                 GUITools.DrawToolbarDivider(xOffset+GUITools.iconButtonWidth, y);
        
    //                 if (p.objectReferenceValue != null) {
                        
    //                     EditorGUI.BeginChangeCheck();
    //                     float propWidth = ((Condition)p.objectReferenceValue).DrawGUI(propRect);
    //                     if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(p.objectReferenceValue);


    //                     GUITools.DrawToolbarDivider(propRect.x + propWidth + GUITools.toolbarDividerSize, y);
        
    //                     bool isAnd = ((Condition)p.objectReferenceValue).link == ConditionLink.And;

    //                     isAnd = GUITools.DrawToggleButton(isAnd, new GUIContent(isAnd ? "&&" : "||"), propRect.x + propWidth + GUITools.toolbarDividerSize * 2, y, GUITools.white, GUITools.white);
                        
    //                     ((Condition)p.objectReferenceValue).link = isAnd ? ConditionLink.And : ConditionLink.Or;
                        
    //                 }

                    
                    
    //                 float h = EditorGUI.GetPropertyHeight(p, true);
                    
    //                 y += h;
    //                 propRect.y += h;
    //             }

                
    //             if (indexToDelete != -1) {
    //                 SerializedProperty deleted = prop.GetArrayElementAtIndex(indexToDelete);
                    
    //                 Object deletedObject = deleted.objectReferenceValue;
                    
    //                 if (deletedObject != null) prop.DeleteArrayElementAtIndex(indexToDelete);
                    
    //                 prop.DeleteArrayElementAtIndex(indexToDelete);
                    
    //                 Object.DestroyImmediate(deletedObject, true);
                    
    //                 if (isAsset) {
    //                     AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(baseObject));
    //                     AssetDatabase.SaveAssets(); 
    //                 }                
    //             }
    //         }

    //         EditorGUI.EndProperty();
    //     }
    // }

    
    public enum RunTarget {
        Subject, Target, Reference
    };

    [System.Serializable] public class Condition {

        // protected T CastParameter <T> (object parameter, T defaultValue) {
        //     if (parameter == null) {
        //         Debug.LogWarning("Condition parameter null");
        //         return defaultValue;
        //     }
        //     try {
        //         T r = (T)parameter;
        //         return r;
        //     }
        //     catch {
        //         Debug.Log(parameter.ToString() + " (" + parameter.GetType().FullName + ") cannot be cast to: " + typeof(T).FullName);
        //         return defaultValue;
        //     }
        // }
        // public ConditionLink link;

        public bool or;

        public RunTarget runTarget;
        public GameObject referenceTarget;
        public string callMethod;
        public float threshold; // or check against global game values
        public NumericalCheck numericalCheck;


        public bool hasParameters;

        public ConditionsParameters parameters;




        
        // protected bool CheckInteger (NumericalCheck condition, int runtimeValue, int conditionThreshold) {
        //     if (condition == NumericalCheck.Equals) return runtimeValue == conditionThreshold;
        //     else if (condition == NumericalCheck.NotEquals) return runtimeValue != conditionThreshold;
        //     else if (condition == NumericalCheck.LessThan) return runtimeValue < conditionThreshold;
        //     else if (condition == NumericalCheck.GreaterThan) return runtimeValue > conditionThreshold;
        //     else if (condition == NumericalCheck.LessThanEqualTo) return runtimeValue <= conditionThreshold;
        //     else if (condition == NumericalCheck.GreaterThanEqualTo) return runtimeValue >= conditionThreshold;
        //     return false;
        // }
        protected bool CheckFloat (NumericalCheck condition, float runtimeValue, float conditionThreshold) {
            if (condition == NumericalCheck.Equals) return runtimeValue == conditionThreshold;
            else if (condition == NumericalCheck.NotEquals) return runtimeValue != conditionThreshold;
            else if (condition == NumericalCheck.LessThan) return runtimeValue < conditionThreshold;
            else if (condition == NumericalCheck.GreaterThan) return runtimeValue > conditionThreshold;
            else if (condition == NumericalCheck.LessThanEqualTo) return runtimeValue <= conditionThreshold;
            else if (condition == NumericalCheck.GreaterThanEqualTo) return runtimeValue >= conditionThreshold;
            return false;
        }
        // protected bool CheckFloatSqr (NumericalCheck condition, float runtimeValue, float conditionThreshold) {
        //     return CheckFloat(condition, runtimeValue, conditionThreshold * conditionThreshold);
        // }

        // object[] conditionParameters);
        public bool IsMet (GameObject subject, GameObject target) {
            if (string.IsNullOrEmpty(callMethod) || string.IsNullOrWhiteSpace(callMethod)) {
                Debug.LogWarning("Call Method is blank...");
                return false;
            }

            GameObject obj = subject;

            switch (runTarget) {
                case RunTarget.Subject:
                    obj = subject;
                    break;
                case RunTarget.Target:
                    obj = target;
                    break;
                case RunTarget.Reference:
                    obj = referenceTarget;
                    break;
            }

            if (obj == null) {
                Debug.LogWarning("RunTarget: " + runTarget.ToString() + " is null, can't call condition method: " + callMethod);
                return false;
            }


            List<object> parametersList = new List<object>();

            if (hasParameters) {

                for (int i = 0; i < parameters.Length; i++) {
                    if (parameters[i] != null) {
                        parametersList.Add(parameters[i].GetParamObject());
                    }
                }
            }
            object[] suppliedParameters = parametersList.ToArray();
            
            // object[] paremeters = new object[] {
            //     // false,  // whether the message has been called yet
            //     // "",  // was there an error ?
            //     // 0.0f,   // the return value of teh method called
            // };
            // obj.SendMessage("_condition_" + callMethod, messageContext, SendMessageOptions.DontRequireReceiver);

            float returnValue;
            // bool wasCalled = GetValueFromCallMethod (obj, "_condition_" + callMethod, messageContext, out returnValue);
            bool wasCalled = GetValueFromCallMethod (obj, callMethod, suppliedParameters, out returnValue);

            // bool wasCalled = (bool)messageContext[0];
            if (!wasCalled) {

                string paramTypesNames = "";

                for (int i = 0; i < suppliedParameters.Length; i++) {
                    paramTypesNames += suppliedParameters[i].GetType().Name + ", ";
                }

                Debug.LogError("Run Target: " + obj.name + " does not contain a call method: '" + callMethod + "' with parameter types: (" + paramTypesNames + ")");
                return false;
            }
            
            // string error = (string)messageContext[0];//1];

            // if (string.IsNullOrEmpty(error) && string.IsNullOrWhiteSpace(error)) {
                float runtimeValue = returnValue;// (float)messageContext[2];
                return CheckFloat(numericalCheck, runtimeValue, threshold);
            // }
            // else {
            //     Debug.LogError("Error: " + callMethod + ":: " + error);
            //     return false;
            // }
            // try {
            // }
            // catch {
            //     Debug.LogError("Run Target: " + obj.name + " does not contain a call method: " + callMethod);
            //     return false;
            // }
        }
        // public abstract string ParametersKey();

        
        // static readonly string[] numericalCheckOptions = new string[] { " == ", " != ", " < ", " > ", " <= ", " >= " };
        // protected const float numericalCheckWidth = 40;
        // protected NumericalCheck DrawNumericalCheck (ref Rect pos, NumericalCheck value) {
        //     pos.width = numericalCheckWidth;
        //     value = (NumericalCheck)EditorGUI.Popup (pos, (int)value, numericalCheckOptions);
        //     pos.x += numericalCheckWidth;
        //     return value;
        // }
        
        // public abstract float DrawGUI(Rect pos);
        // public virtual float GetPropertyHeight() { 
        //     return GUITools.singleLineHeight; 
        // }



        static Dictionary<int, Component[]> componentsPerGameObject = new Dictionary<int, Component[]>();
        
        static bool CheckParametersForMethod (MethodInfo method, object[] suppliedParameters) {
            ParameterInfo[] methodParams = method.GetParameters();

            if (methodParams.Length != suppliedParameters.Length)
                return false;

            for (int p = 0; p < methodParams.Length; p++) {
                System.Type methodType = methodParams[p].ParameterType;
                System.Type suppliedType = suppliedParameters[p].GetType();
                if (suppliedType != methodType && !suppliedType.IsSubclassOf(methodType)) 
                    return false;
            }
            return true;
        }
        
        static bool GetValueFromCallMethod (GameObject onGameObject, string callMethod, object[] parameters, out float value) {

            int instanceID = onGameObject.GetInstanceID();
            Component[] allComponentsOnGameObject;
            if (!componentsPerGameObject.TryGetValue(instanceID, out allComponentsOnGameObject)) {
                allComponentsOnGameObject = onGameObject.GetComponents<Component>();
                componentsPerGameObject[instanceID] = allComponentsOnGameObject;
            }

            for (int i = 0; i < allComponentsOnGameObject.Length; i++) {
                Component c = allComponentsOnGameObject[i];
                System.Type componentType = c.GetType();

                MethodInfo[] allMethods = componentType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                for (int m = 0; m < allMethods.Length; m++) {
                    MethodInfo method = allMethods[m];

                    if (method.Name == callMethod) {
                        if (CheckParametersForMethod ( method, parameters )) {
                            if (method.ReturnType == typeof(float) || method.ReturnType == typeof(int)) {
                                value = (float)method.Invoke(c, parameters );
                                return true;
                            }
                            else if (method.ReturnType == typeof(bool)) {
                                value = ((bool)method.Invoke(c, parameters )) ? 1f : 0f;
                                return true;
                            }
                        }
                    }
                }
            }

            value = 0;
            return false;

        }
    }
        
    [CustomPropertyDrawer(typeof(Condition))] class ConditionDrawer : PropertyDrawer {
        static readonly string[] numericalCheckOptions = new string[] { " == ", " != ", " < ", " > ", " <= ", " >= " };
        protected const float numericalCheckWidth = 40;
        
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);

            float origX = pos.x;
            float origWidth = pos.width;



            pos.width = 65;
            // runTarget = (RunTarget)EditorGUI.EnumPopup(pos, runTarget);

            SerializedProperty runTargetProp = prop.FindPropertyRelative("runTarget");
            EditorGUI.PropertyField(pos, runTargetProp, GUITools.noContent, true);


            pos.x += pos.width;

            // if (runTargetProp.enumValueIndex == (int)RunTarget.Reference) {

            //     pos.width = 125;
            //     EditorGUI.PropertyField(pos, prop.FindPropertyRelative("referenceTarget"), GUITools.noContent, true);
            //     // referenceTarget = (GameObject)EditorGUI.ObjectField(pos, referenceTarget, typeof(GameObject), true);
            //     pos.x += pos.width;

            // }
            
            pos.width = 125;

            GUITools.StringFieldWithDefault(pos.x, pos.y, pos.width, pos.height, prop.FindPropertyRelative("callMethod"), "Call Method");
            // callMethod = GUITools.StringFieldWithDefault (pos.x, pos.y, pos.width, pos.height, callMethod, "Call Method");
            
            pos.x += pos.width;

            
            SerializedProperty numCheckProp = prop.FindPropertyRelative("numericalCheck");
            pos.width = numericalCheckWidth;
            numCheckProp.enumValueIndex = EditorGUI.Popup (pos, numCheckProp.enumValueIndex, numericalCheckOptions);
            pos.x += pos.width;
            // numericalCheck = DrawNumericalCheck(ref pos, numericalCheck);
            



            pos.width = 60;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("threshold"), GUITools.noContent, true);
            pos.x += pos.width;
            
            // GUITools.DrawToolbarDivider(propRect.x + propWidth + GUITools.toolbarDividerSize, y);

            SerializedProperty paramsProp = prop.FindPropertyRelative("parameters");
            int paramsCount = paramsProp.FindPropertyRelative("list").arraySize;

            SerializedProperty hasParametersProp = prop.FindPropertyRelative("hasParameters");

            if (paramsCount > 0) {
                hasParametersProp.boolValue = true;
            }
            
            hasParametersProp.boolValue = GUITools.DrawToggleButton(hasParametersProp.boolValue, new GUIContent("P", "Has Parameters"), pos.x, pos.y, GUITools.blue, GUITools.white);
            pos.x += GUITools.iconButtonWidth;
            
            SerializedProperty orProp = prop.FindPropertyRelative("or");
            orProp.boolValue = GUITools.DrawToggleButton(orProp.boolValue, new GUIContent(orProp.boolValue ? "||" : "&&"), pos.x, pos.y, GUITools.white, GUITools.white);



            if (runTargetProp.enumValueIndex == (int)RunTarget.Reference) {
                pos.y += EditorGUIUtility.singleLineHeight;
                pos.x = origX;// + GUITools.iconButtonWidth;
                pos.width = origWidth ;//- GUITools.iconButtonWidth;
                
                // pos.width = 125;
                EditorGUI.LabelField(pos, "Reference:");

                pos.x += 65;
                pos.width -= 65;
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("referenceTarget"), GUITools.noContent, true);
                // referenceTarget = (GameObject)EditorGUI.ObjectField(pos, referenceTarget, typeof(GameObject), true);
                // pos.x += pos.width;

            }
            
                        

            if (hasParametersProp.boolValue) {

                pos.y += EditorGUIUtility.singleLineHeight;
                pos.x = origX ;//+ GUITools.iconButtonWidth;
                pos.width = origWidth ;//- GUITools.iconButtonWidth;
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("parameters"), true);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            
            SerializedProperty hasParametersProp = prop.FindPropertyRelative("hasParameters");
            SerializedProperty runTargetProp = prop.FindPropertyRelative("runTarget");
            
            float h = GUITools.singleLineHeight;

            if (hasParametersProp.boolValue) {
                h += EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("parameters"), true);
            }
            if (runTargetProp.enumValueIndex == (int)RunTarget.Reference) {
                h += GUITools.singleLineHeight;
            }
            return h;
            // return GUITools.singleLineHeight + EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("parameters"), true);
            
            
            // if (prop.objectReferenceValue == null) return GUITools.singleLineHeight;
            // return ((Condition)prop.objectReferenceValue).GetPropertyHeight();
        }
    }
}
