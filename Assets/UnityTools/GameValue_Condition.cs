// using System.Collections.Generic;

// using UnityEngine;

// using UnityEditor;
// using UnityTools.EditorTools;
// namespace UnityTools {

//     public class GameValue_Condition : Condition
//     {
//         public bool useSuppliedValues;
//         public bool trueIfNoValue;
//         public string valueName;
//         public NumericalCheck condition;
        
//         public GameValue.GameValueComponent component;

//         public float threshold;

//         // public override bool IsMet (object[] conditionParameters) {
//         public override bool IsMet (GameObject subject, GameObject target){
        
            
            
//             // if (useSuppliedValues && conditionParameters.Length != 2) {
//             //     Debug.LogWarning("Need to supply 2 game values dictionaries in order to use 'supplied' game values");
//             //     return false;
//             // }

//             // int paramsToUse = useSuppliedValues ? 1 : 0;

//             // Dictionary<string, GameValue> gameValues = CastParameter<Dictionary<string, GameValue>>(conditionParameters[paramsToUse], null);
//             // if (gameValues == null)
//             //     return false;
            

//             // GameValue gameValue;
//             // if (!gameValues.TryGetValue(valueName, out gameValue)) {
//             //     Debug.LogError("Cant find game value: " + valueName);
//             //     return trueIfNoValue;
//             // }

//             // float value = gameValue.GetValueComponent(component);

//             // return CheckFloat(condition, value, threshold);

//             return false;
//         }
//         // public override string ParametersKey() {
//         //     return "GameValues";
//         // }


//         static GUIContent _useSecondaryParameterGUI;
//         static GUIContent useSecondaryParameterGUI {
//             get {
//                 if (_useSecondaryParameterGUI == null) _useSecondaryParameterGUI = BuiltInIcons.GetIcon("PreMatLight1", "Use Secondary Game Values");
//                 return _useSecondaryParameterGUI;
//             }
//         }
//         static GUIContent _trueIfNoValueGUI;
//         static GUIContent trueIfNoValueGUI {
//             get {
//                 if (_trueIfNoValueGUI == null) _trueIfNoValueGUI = BuiltInIcons.GetIcon("toggle on@2x", "True If No Value Found");
//                 return _trueIfNoValueGUI;
//             }
//         }
//         public override float DrawGUI(Rect pos) {
            
//             useSuppliedValues = GUITools.DrawToggleButton(useSuppliedValues, useSecondaryParameterGUI, pos.x, pos.y, GUITools.blue, GUITools.white);
//             pos.x += GUITools.iconButtonWidth;
            
//             trueIfNoValue = GUITools.DrawToggleButton(trueIfNoValue, trueIfNoValueGUI, pos.x, pos.y, GUITools.blue, GUITools.white);
//             pos.x += GUITools.iconButtonWidth;
            
//             pos.width = 75;
//             valueName = GUITools.StringFieldWithDefault (pos.x, pos.y, pos.width, pos.height, valueName, "Value Name");
//             pos.x += pos.width;
            
//             pos.width = 90;
//             component = (GameValue.GameValueComponent)EditorGUI.EnumPopup(pos, component);
//             pos.x += pos.width;
            
//             condition = DrawNumericalCheck(ref pos, condition);

//             pos.width = 60;
//             threshold = EditorGUI.FloatField(pos, threshold);
            
//             return GUITools.iconButtonWidth * 2 + 75 + 90 + numericalCheckWidth + 60;
                    
//         }        
//     }
// }


