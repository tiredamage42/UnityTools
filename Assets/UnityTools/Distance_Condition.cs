// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// using UnityTools.EditorTools;
// using UnityEditor;
// namespace UnityTools {

//     public class Distance_Condition : Condition
//     {
        
//         public NumericalCheck condition;
        
//         public float threshold;

//         public bool overrideAPos;
//         public Transform transformA;

//         public bool overrideBPos;
//         public Transform transformB;
        
        
//         bool Check_A_Position (Vector3 posA, Vector3 posB) {
//             if (overrideBPos && transformB != null) {
//                 return CheckFloatSqr(condition, Vector3.SqrMagnitude(posA - transformB.position), threshold);            
//             }
//             else {
//                 return CheckFloatSqr(condition, Vector3.SqrMagnitude(posA - posB), threshold);            
//             }
//         }

//         bool CheckDistanceCondition (Vector3 posA, Vector3 posB) {
//             if (overrideAPos && transformA != null) {
//                 Check_A_Position(transformA.position, posB);
//             }
//             else {
//                 Check_A_Position(posA, posB);   
//             }
//             return false;
//         }

//         static readonly Vector3 errorPosition = Vector3.down * 99999;

//         // public override bool IsMet (object[] conditionParameters) {
//         public override bool IsMet (GameObject subject, GameObject target){
        
//             // if (conditionParameters.Length != 2) {
//             //     Debug.LogWarning("Need to supply 2 positions to check distance");
//             //     return false;
//             // }

//             // Vector3 posA = CastParameter<Vector3>(conditionParameters[0], errorPosition);
//             // if (posA == errorPosition)
//             //     return false;
            
//             // Vector3 posB = CastParameter<Vector3>(conditionParameters[1], errorPosition);
//             // if (posB == errorPosition)
//             //     return false;
            

//             // return CheckDistanceCondition ( posA, posB );

//             return false;
//         }
            
//         // public override string ParametersKey() {
//         //     return "Distance";
//         // }

//         static GUIContent _overrideTransfromGUI;
//         static GUIContent overrideTransfromGUI {
//             get {
//                 if (_overrideTransfromGUI == null) _overrideTransfromGUI = BuiltInIcons.GetIcon("Transform Icon", "Override With Scene Transform");
//                 return _overrideTransfromGUI;
//             }
//         }


//         public override float DrawGUI(Rect pos) {
//             pos.width = 60;
//             EditorGUI.LabelField(pos, "Distance: ");
//             pos.x += pos.width;

//             overrideAPos = GUITools.DrawToggleButton(overrideAPos, overrideTransfromGUI, pos.x, pos.y, GUITools.blue, GUITools.white);
//             pos.x += GUITools.iconButtonWidth;

//             if (overrideAPos) {
//                 pos.width = 60;
//                 transformA = (Transform)EditorGUI.ObjectField(pos, transformA, typeof(Transform), true);
//             }
//             else {
//                 pos.width = 60;
//                 EditorGUI.LabelField(pos, "Position A");
//             }
//             pos.x += pos.width;

                
//             pos.width = 25;
//             EditorGUI.LabelField(pos, "=>");
//             pos.x += pos.width;

//             overrideBPos = GUITools.DrawToggleButton(overrideBPos, overrideTransfromGUI, pos.x, pos.y, GUITools.blue, GUITools.white);
//             pos.x += GUITools.iconButtonWidth;

//             if (overrideBPos) {
//                 pos.width = 60;
//                 transformB = (Transform)EditorGUI.ObjectField(pos, transformB, typeof(Transform), true);
//             }
//             else {
//                 pos.width = 60;
//                 EditorGUI.LabelField(pos, "Position B");
//             }
//             pos.x += pos.width;

//             condition = DrawNumericalCheck(ref pos, condition);

//             pos.width = 60;
//             threshold = EditorGUI.FloatField(pos, threshold);
            
//             return GUITools.iconButtonWidth * 2 + 25 + 60 * 2 + 50 + numericalCheckWidth + 60;
            



            
//             // DrawTransformSelection ( newPos, property, 0, ref newX);
//             // DrawLabel ( newPos, ref newX, "And", 40);
//             // DrawTransformSelection ( newPos, property, 1, ref newX);

//             // DrawConditionCheck ( position, property, ref x);  
//             // DrawFloatValueForCheck ( position, property, ref x);            
            
            
//             // useSuppliedValues = GUITools.DrawToggleButton(useSuppliedValues, useSecondaryParameterGUI, pos.x, pos.y, GUITools.blue, GUITools.white);
//             // pos.x += GUITools.iconButtonWidth;
            
//             // trueIfNoValue = GUITools.DrawToggleButton(trueIfNoValue, trueIfNoValueGUI, pos.x, pos.y, GUITools.blue, GUITools.white);
//             // pos.x += GUITools.iconButtonWidth;
            
//             // pos.width = 75;
//             // valueName = GUITools.StringFieldWithDefault (pos.x, pos.y, pos.width, pos.height, valueName, "Value Name");
//             // pos.x += pos.width;
            
//             // pos.width = 90;
//             // component = (GameValue.GameValueComponent)EditorGUI.EnumPopup(pos, component);
//             // pos.x += pos.width;
            
//             // condition = DrawNumericalCheck(ref pos, condition);

//             // pos.width = 60;
//             // threshold = EditorGUI.FloatField(pos, threshold);
            
//             // return GUITools.iconButtonWidth * 2 + 75 + 90 + numericalCheckWidth + 60;
                    
//         }     
//     }
// }
// /*

// // using System.Collections.Generic;
// using UnityEngine;

// using Game.PerkSystem;
// using Game.InventorySystem;
// using Game.QuestSystem;
// #if UNITY_EDITOR
// using UnityEditor;
// #endif

// namespace Game {




//     [System.Serializable] public class ActorValueConditionArray : NeatArrayWrapper<ActorValueCondition> { }
        
// #if UNITY_EDITOR
//     [CustomPropertyDrawer(typeof(ActorValueCondition))] public class ActorValueConditionDrawer : PropertyDrawer
//     {
//         static readonly string[] selfSuppliedOptions = new string[] { "Self", "Supplied" };
//         static readonly string[] conditionTypeOptions = new string[] { "Game Value", "Inventory", "Perk Level", "Quest", "Distance", "Angle" };
//         static readonly string[] conditionsOptions = new string[] { " == ", " != ", " < ", " > ", " <= ", " >= " };
//         static readonly string[] inventoryCheckOptions = new string[] { "Stash Count", "Equipped To Slot", "Not Equipped To Slot" };
        

//         static readonly string[] transformTypeOptions = new string[] { 
//             "Self Actor" , "Supplied Actor" , "Any Scene Item" , "All Scene Items" , "Transform" 
//         };
        
        

//         void DrawNameValue (Rect position, SerializedProperty property, ref float x) {
//             SerializedProperty p = property.FindPropertyRelative("valueName");
//             string s = p.stringValue;
//             if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) p.stringValue = "Name";
//             DrawProperty(position, p, 125, ref x);
//         }


//         void DrawProperty (Rect position, SerializedProperty property, float width, ref float x) {
//             EditorGUI.PropertyField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property, GUIContent.none);
//             x+= width;
//         }
//         void DrawProperty (Rect position, SerializedProperty property, string propName, float width, ref float x) {
//             DrawProperty(position, property.FindPropertyRelative(propName), width, ref x);
//         }

//         void DrawEnumWithCustomOptions (Rect position, SerializedProperty property, string propName, string[] customOptions, float width, ref float x) {
//             SerializedProperty enumProp = property.FindPropertyRelative(propName);
//             enumProp.enumValueIndex = EditorGUI.Popup (new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), enumProp.enumValueIndex, customOptions);
//             x+= width;                
//         }
        
//         void DrawConditionCheck (Rect position, SerializedProperty property, ref float x) {
//             DrawEnumWithCustomOptions(position, property, "condition", conditionsOptions, 40, ref x);
//         }

//         void DrawLabel (Rect position, ref float x, string txt, float width) {
//             EditorGUI.LabelField(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), txt);
//             x+= width;
//         }

//         void DrawItemCheck (Rect position, SerializedProperty property, int index, ref float x) {
//             float width = 175;
//             InventorySystemEditor.itemSelector.Draw(new Rect(x, position.y, width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("itemCheck" + (index == 0 ? "A" : "B")), GUIContent.none);
//             x+= width;
//         }

        
//         void DrawFloatValueForCheck (Rect position, SerializedProperty property, ref float x) {
//             DrawProperty(position, property, "valueCheck", 60, ref x);
//         }
//         void DrawIntValueForCheck (Rect position, SerializedProperty property, ref float x) {
//             DrawProperty(position, property, "intCheckLevel", 60, ref x);
//         }


//         void DrawTransformSelection (Rect position, SerializedProperty property, int index, ref float x) {
            

//             string propName = index == 0 ? "aTransformCheckType" : "bTransformCheckType";

//             DrawEnumWithCustomOptions ( position, property, propName, transformTypeOptions, 90, ref x);

//             SerializedProperty transformType = property.FindPropertyRelative(propName);

//             // DrawProperty(position, transformType, 120, ref x);
            
//             if (transformType.enumValueIndex == 4) {
//                 DrawProperty(position, property, index == 0 ? "transformA" : "transformB", 150, ref x);
//             }
//             else if (transformType.enumValueIndex == 2 || transformType.enumValueIndex == 3) {
//                 DrawItemCheck ( position, property, 0, ref x);
//             }
//         }

            
        
            


//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//         {
            
//         }

//         public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//         {
//             SerializedProperty conditionType = property.FindPropertyRelative("conditionType");


//             if (conditionType.enumValueIndex == 4 || conditionType.enumValueIndex == 5){
//             return EditorGUIUtility.singleLineHeight * 2;
           
//             }
// return EditorGUIUtility.singleLineHeight;
           
//         }
//     }
// #endif

// }

//  */