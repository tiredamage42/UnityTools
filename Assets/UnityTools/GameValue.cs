using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

using UnityTools.EditorTools;

using UnityEditor;

namespace UnityTools {
    /*
        a float value wrapper,

        we can cap it dynamically, and add modifiers to it
    */


    [System.Serializable] public class GameValueArray : NeatArrayWrapper<GameValue> { }
    [System.Serializable] public class GameValue
    {
        
        static GameValue GetGameValue (Dictionary<string, GameValue> gameValues, string name) {
            if (gameValues.ContainsKey(name)) return gameValues[name];
            return null;
        }


        public static void AddModifiers (Dictionary<string, GameValue> gameValues, GameValueModifier[] mods, int count, string description, bool assertPermanent, GameObject subject, GameObject target) {
            for (int i =0 ; i < mods.Length; i++) {
                if (assertPermanent && !mods[i].isPermanent) continue;
                if (Conditions.ConditionsMet (mods[i].conditions, subject, target)) {
                    GameValue gameValue = GetGameValue(gameValues, mods[i].gameValueName);
                    if (gameValue != null) {
                        gameValue.AddModifier(mods[i], count, (description + i.ToString()).GetPersistentHashCode(), description);
                    }
                }
            }
        }
        public static void RemoveModifiers (Dictionary<string, GameValue> gameValues, GameValueModifier[] mods, int count, string description) {
            for (int i =0 ; i < mods.Length; i++) {
                GameValue gameValue = GetGameValue(gameValues, mods[i].gameValueName);
                if (gameValue != null) {
                    gameValue.RemoveModifier(mods[i], count, (description + i.ToString()).GetPersistentHashCode());
                }
            }
        }



        public enum GameValueComponent { BaseValue, BaseMinValue, BaseMaxValue, Value, MinValue, MaxValue };
        public string name;
        [TextArea] public string description;
        // [HideInInspector] 
        public float baseValue;
        // public Vector2 baseMinMax = new Vector2(0,500);
        // public Vector2 initializationRange;
        // public Vector4 capInitRange = new Vector4(0, 500, 0, 0);

        public float initMin, initMax;
        public float capMin, capMax = 500;

        public bool showAdvanced;

        
        public GameValue(string name, float baseValue, Vector2 baseMinMax, string description){
            this.name = name;
            this.baseValue = baseValue;

            capMin = baseMinMax.x;
            capMax = baseMinMax.y;
            initMax = initMin = baseValue;
            
            // this.capInitRange = new Vector4(baseMinMax.x, baseMinMax.y, baseValue, baseValue);
            // this.baseMinMax = baseMinMax;

            this.description = description;
        }

        public GameValue (GameValue template) {
            this.name = template.name;
            // this.baseValue = UnityEngine.Random.Range(template.initializationRange.x, template.initializationRange.y);
            // this.baseValue = UnityEngine.Random.Range(template.capInitRange.z, template.capInitRange.w);
            this.baseValue = UnityEngine.Random.Range(template.initMin, template.initMax);
            
            // this.baseMinMax = template.baseMinMax;
            // this.capInitRange = template.capInitRange;

            capMin = template.capMin;
            capMax = template.capMax;
            initMin = template.initMin;
            initMax = template.initMax;
            
            this.description = template.description;
        }

        public void ReInitialize () {
            this.baseValue = UnityEngine.Random.Range(initMin, initMax);
            // this.baseValue = UnityEngine.Random.Range(initializationRange.x, initializationRange.y);
        }


        // delta, current, min, max
        event System.Action<float, float, float, float> onValueChange;
        public void AddChangeListener (Action<float, float, float, float> listener) {
            onValueChange += listener;
        }
        public void RemoveChangeListener (Action<float, float, float, float> listener) {
            onValueChange -= listener;
        }

        //not using a dictionary in order to keep thses serializable by unity
        [HideInInspector] public List<GameValueModifier> modifiers = new List<GameValueModifier>();


        public static string ModifyBehaviorString (GameValueModifier.ModifyBehavior modifyBehavior) {
            if (modifyBehavior == GameValueModifier.ModifyBehavior.Set)
                return "Set";
            else if (modifyBehavior == GameValueModifier.ModifyBehavior.Add)
                return "+";
            else if (modifyBehavior == GameValueModifier.ModifyBehavior.Multiply) 
                return "x";
            return "";
        }
        

        public string GetModifiersSummary () {
            string r = "";

            for (int i = 0; i < modifiers.Count; i++) {
                GameValueModifier m = modifiers[i];
                r += m.description + ": " +  m.modifyValueComponent + " " + ModifyBehaviorString(m.modifyBehavior) + m.modifyValue + (m.isStackable ? "(" + m.count + ")" : "") + "\n";
            }

            return r;
        }


        float GetModifiedValue (GameValueComponent checkType, float value, float min = float.MinValue, float max = float.MaxValue) {
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].modifyValueComponent == checkType) {
                    value = modifiers[i].Modify(value);
                }
            }
            return Mathf.Clamp(value, min, max);
        }
        public float GetValue () {
            return GetModifiedValue (GameValueComponent.Value, baseValue, GetMinValue(), GetMaxValue());
        }
        public float GetMinValue () {
            return GetModifiedValue(GameValueComponent.MinValue, capMin);
        }
        public float GetMaxValue () {
            return GetModifiedValue(GameValueComponent.MaxValue, capMax);
        }
        public float GetValueComponent (GameValueComponent checkType) {
            switch (checkType) {
                case GameValueComponent.Value:
                    return GetValue();
                case GameValueComponent.MinValue:
                    return GetMinValue();
                case GameValueComponent.MaxValue:
                    return GetMaxValue();
                case GameValueComponent.BaseValue:
                    return baseValue;
                case GameValueComponent.BaseMinValue:
                    return capMin;
                case GameValueComponent.BaseMaxValue:
                    return capMax;
            }
            return 0;
        }

    
        GameValueModifier GetModifier (int key) {    
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].key == key) {
                    return modifiers[i];
                }
            }
            return null;
        }

        void BroadcastValueChange (float origValue, float minVal, float maxVal) {
            if (onValueChange != null) {
                // delta, current, min, max
                float newVal = GetValue();
                onValueChange(newVal - origValue, newVal, minVal, maxVal);
            }
        }

        void ModifyPermanent (GameValueModifier modifier) {
            float origValue = GetValue();
            
            if (modifier.modifyValueComponent == GameValueComponent.BaseValue) {
                baseValue = modifier.Modify(baseValue);
            }
            if (modifier.modifyValueComponent == GameValueComponent.BaseMinValue) {
                capMin = modifier.Modify(capMin);
            }
            if (modifier.modifyValueComponent == GameValueComponent.BaseMaxValue) {
                capMax = modifier.Modify(capMax);
            }

            float minVal = GetMinValue();
            float maxVal = GetMaxValue();
                            
            baseValue = modifier.Modify(baseValue);
                
            //clamp the base value
            baseValue = Mathf.Clamp(baseValue, minVal, maxVal);

            BroadcastValueChange ( origValue, minVal, maxVal );
        }

        // anything modifying base values is permanent, and doesnt get stored in 
        // our modifiers list
        public void AddModifier (GameValueModifier modifier, int count, int key, string description) {
            if (modifier.isPermanent) {
                ModifyPermanent(modifier);
                return;
            }
            
            float origValue = GetValue();

            GameValueModifier existingModifier = GetModifier ( key );
            if (existingModifier != null) {
                if (existingModifier.description != description) {
                    Debug.LogWarning("Description mismatch for same key! 1) " + description + " :: 2) " + existingModifier.description);
                }
                existingModifier.count += count;
            }
            else {
                modifiers.Add(new GameValueModifier(modifier, count, key, description));
            }

            BroadcastValueChange ( origValue, GetMinValue(), GetMaxValue() );
        }

        public void RemoveModifier (GameValueModifier modifier, int count, int key){
            if (modifier.isPermanent) return;
            
            GameValueModifier existingModifier = GetModifier ( key );
            if (existingModifier != null) {
                float origValue = GetValue();
                
                existingModifier.count -= count;
                if (existingModifier.count <= 0) {
                    modifiers.Remove(existingModifier);
                }

                BroadcastValueChange ( origValue, GetMinValue(), GetMaxValue() );
            }
        }
    }

    
#if UNITY_EDITOR
    
    [CustomPropertyDrawer(typeof(GameValue))] public class GameValueDrawer : PropertyDrawer
    {
        protected GUIContent showAdvancedGUI = BuiltInIcons.GetIcon("TerrainInspector.TerrainToolSettings", "Show Advanced");


        public void PrintModifiersSummary (Rect pos, SerializedProperty prop) {
            
            SerializedProperty modifiers = prop.FindPropertyRelative("modifiers");
            
            for (int i = 0; i < modifiers.arraySize; i++) {
                SerializedProperty m = modifiers.GetArrayElementAtIndex(i);
                EditorGUI.LabelField(pos, m.FindPropertyRelative("description").stringValue + ": " +  ((GameValue.GameValueComponent)m.FindPropertyRelative("modifyValueComponent").enumValueIndex) + " " + GameValue.ModifyBehaviorString((GameValueModifier.ModifyBehavior)m.FindPropertyRelative("modifyBehavior").enumValueIndex) + m.FindPropertyRelative("modifyValue").floatValue + (m.FindPropertyRelative("isStackable").boolValue ? "(" + m.FindPropertyRelative("count").intValue + ")" : ""));
                pos.y += EditorGUIUtility.singleLineHeight;
            }            
        }



        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            float singleLineHeight = EditorGUIUtility.singleLineHeight;
            
            EditorGUI.BeginProperty(position, label, property);


            SerializedProperty showAdvancedProp = property.FindPropertyRelative("showAdvanced");
            float x = position.x;
            float origX = x;
    
            GUITools.StringFieldWithDefault ( x, position. y, position.width - GUITools.iconButtonWidth, singleLineHeight, property.FindPropertyRelative("name"), "Game Value Name");
            
            GUITools.DrawToggleButton(showAdvancedProp, showAdvancedGUI, x + (position.width - GUITools.iconButtonWidth), position.y, GUITools.blue, GUITools.white);
            // x += 125;


            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(new Rect(x, position.y, 55, singleLineHeight), new GUIContent("Range:"));
            
            // EditorGUI.PropertyField(new Rect(x + 100, position.y, position.width - 100, singleLineHeight), property.FindPropertyRelative("capInitRange"), GUIContent.none);

            position.x += 55;
            float w = position.width - 110;

            float w4 = w * .25f;
            
            EditorGUI.PropertyField(new Rect(position.x, position.y, w4, singleLineHeight), property.FindPropertyRelative("capMin"), GUIContent.none);
            position.x += w4;

            EditorGUI.PropertyField(new Rect(position.x, position.y, w4, singleLineHeight), property.FindPropertyRelative("capMax"), GUIContent.none);
            position.x += w4;

            EditorGUI.LabelField(new Rect(position.x, position.y, 55, singleLineHeight), new GUIContent("Initial:"));
            position.x += 55;
            

            EditorGUI.PropertyField(new Rect(position.x, position.y, w4, singleLineHeight), property.FindPropertyRelative("initMin"), GUIContent.none);
            position.x += w4;

            EditorGUI.PropertyField(new Rect(position.x, position.y, w4, singleLineHeight), property.FindPropertyRelative("initMax"), GUIContent.none);
            position.x += w4;

            // EditorGUI.LabelField(new Rect(x, position.y, 45, singleLineHeight), new GUIContent("Range:"));
            // EditorGUI.PropertyField(new Rect(x + 45, position.y, 90, singleLineHeight), property.FindPropertyRelative("baseMinMax"), GUIContent.none);
            // x += 90 + 45 + 10;
            // EditorGUI.LabelField(new Rect(x, position.y, 64, singleLineHeight), new GUIContent("Init Range:"));
            // EditorGUI.PropertyField(new Rect(x + 64, position.y, 90, singleLineHeight), property.FindPropertyRelative("initializationRange"), GUIContent.none);
            

            if (showAdvancedProp.boolValue) {
                GUITools.StringFieldWithDefault ( origX, position.y, position.width, singleLineHeight * 3, property.FindPropertyRelative("description"), "Description...");

                // if (Application.isPlaying) {

                    position.y += singleLineHeight * 3;
                    EditorGUI.PropertyField( new Rect(origX, position.y, position.width, position.height), property.FindPropertyRelative("baseValue"), true);
                    
                    position.y += EditorGUIUtility.singleLineHeight;

                    PrintModifiersSummary (new Rect(origX, position.y, position.width, position.height), property);

                // }
            }
            
            
            EditorGUI.EndProperty();
            
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty showAdvancedProp = property.FindPropertyRelative("showAdvanced");

            if (showAdvancedProp.boolValue) {

                float h = GUITools.singleLineHeight * 4;

                // if (Application.isPlaying) {
                    h += GUITools.singleLineHeight;

                    h += GUITools.singleLineHeight * property.FindPropertyRelative("modifiers").arraySize;
                // }
                return h;
                
            }

            return GUITools.singleLineHeight * 2;
        }
    }
    
#endif
    
}
