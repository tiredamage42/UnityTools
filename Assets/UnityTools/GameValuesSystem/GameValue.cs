using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections;
using UnityTools.EditorTools;

using UnityEditor;

namespace UnityTools {

    [System.Serializable] public class GameValueArray : NeatArrayWrapper<GameValue> { }
    

    /*
        change components:

            base ( gets reset during reinitialization )

            baseAddMod (permanent)
            baseAddMult (permanent)
            
            rangedValue (rangedValueModifier) ( gets reset during reinitialization )

    */

    public enum GameValueModifierBehavior { Add, Multiply, Set };
    public enum GameValueModifierComponent { Base, BaseMod, Ranged };
    public enum GameValueChangedComponent { All = 0, BaseValue = 1, RangedValue = 2, }


    public class GameValueCheck {
        public string name;
        public bool useBaseModifiers, isRangedValue;
        public GameValueCheck(string name, bool useBaseModifiers, bool isRangedValue) {
            this.name = name;
            this.useBaseModifiers = useBaseModifiers;
            this.isRangedValue = isRangedValue;
        }
    }
    public class GameValueChecker {
        public static bool CheckGameValues(string debugName, GameValuesContainer container, GameValueCheck[] checks) {
            if (container == null) {
                Debug.LogError(debugName + " doesnt have an attached GameValuesContainer!");
                return false;
            }
            bool checksOK = true;
            for (int i = 0; i < checks.Length; i++) {
                GameValue gv = container.GetGameValueObject(checks[i].name);
                if (gv == null) {
                    checksOK = false;
                }
                else {
                    if (gv.useBaseModifiers != checks[i].useBaseModifiers) {
                        Debug.LogError(debugName + " Needs Game Value Named '" + checks[i].name + "' To " + (checks[i].useBaseModifiers ? "" : "Not") + " Use Base Modifiers");
                        checksOK = false;
                    }
                    if (gv.isRangedValue != checks[i].isRangedValue) {
                        Debug.LogError(debugName + " Needs Game Value Named '" + checks[i].name + "' To " + (checks[i].isRangedValue ? "" : "Not") + " Be A Ranged Value");
                        checksOK = false;
                    }
                }
            }
            return checksOK;
        }
    }

    
    /*
        a float value wrapper,

        we can cap it dynamically, and add modifiers to it
    */
    [System.Serializable] public class GameValue
    {

        public string name;
        [TextArea] public string description;

        //Options
        public bool useBaseModifiers;
        public bool isRangedValue;
        public bool randomInitialization;
        public float initMin, initMax;
        public float rangedMax;

        public GameValue (GameValue template) {
            this.name = template.name;
            this.description = template.description;

            this.useBaseModifiers = template.useBaseModifiers;
            this.isRangedValue = template.isRangedValue;
            this.randomInitialization = template.randomInitialization;
            this.initMin = template.initMin;
            this.initMax = template.initMax;
            this.rangedMax = template.rangedMax;

            ReInitialize();
        }
        

        //Values
        public float baseValue;
        public float baseValueMultiplyModifier; // (permanent)
        public float baseValueAdditiveModifier; // (permanent)
        public float rangedValueModifier;
        
        //Editor...
        public bool showAdvanced;

        void MakeInitial (float initial) {
            if (isRangedValue) {
                this.baseValue = rangedMax;
                SetRangedModifierForTargetValue(initial, GetBaseValue());
            }
            else {
                this.baseValue = initial;
            }
        }

        public void ReInitialize () {
            MakeInitial(randomInitialization ? UnityEngine.Random.Range(initMin, initMax) : initMin);
        }

        [field: NonSerialized]
        event ChangeListener onValueChange;

        void SubscribeToChangeListener (ChangeListener callback) {
            onValueChange += callback;
        }

        public void RemoveChangeListener (ChangeListener callback) {
            onValueChange -= callback;
        }

        public delegate void ChangeListener ( GameValue valueChanged, GameValueChangedComponent changedComponent, float baseDelta, float rangedValueDelta );

        
        public void AddChangeListener (ChangeListener listener, bool callListener) {
            
            SubscribeToChangeListener(listener);
            
            if (callListener) 
                listener (this, GameValueChangedComponent.All, 0, 0);
            
        }
        void BroadcastValueChange (GameValueChangedComponent changedComponent, float baseDelta, float rangedDelta) {
            if (onValueChange != null) 
                onValueChange (this, changedComponent, baseDelta, rangedDelta);
            
        }

        public float GetBaseValue () {
            float v = baseValue;
            if (useBaseModifiers) {
                // multiply, then add, to try and prevent crazy stacking....
                v *= baseValueMultiplyModifier;
                v += baseValueAdditiveModifier;
            }
            return v;
        }

        public float GetValue () {
            float v = GetBaseValue();            
            if (isRangedValue) {
                // used to calculate the 'actual' value out of the max.
                v -= rangedValueModifier;
            }
            return v;
        }

        void GetValueComponents (out float baseVal, out float rangedVal) {
            baseVal = GetBaseValue ();
            rangedVal = isRangedValue ? baseVal - rangedValueModifier : 0;
        }

        void BroadcastBaseValueChange (float originalBaseValue, float originalRangedValue) {
            float newBaseValue = GetBaseValue();

            // if ranged value, reset ranged value to the original value before the change
            // so changes made to base value (max value) dont affect the actual calculated value
            if (isRangedValue) 
                rangedValueModifier = Mathf.Clamp(newBaseValue - originalRangedValue, 0, newBaseValue);
            
            BroadcastValueChange (GameValueChangedComponent.BaseValue, newBaseValue - originalBaseValue, 0);
        }

        bool BaseModifierModdingOK (string methodCheck) {
            if (!useBaseModifiers) {
                Debug.LogError("'" + name + "' GameValue is not using Base Modifiers, '" + methodCheck +"' not allowed!");
                return false;
            }
            return true;
        }
        bool RangedValueModdingOK (string methodCheck) {
            if (!isRangedValue) {
                Debug.LogError("'" + name + "' GameValue is not ranged, '" + methodCheck +"' not allowed!");
                return false;
            }
            return true;
        }

        void SetRangedModifierForTargetValue (float targetValue, float baseVal) {
            rangedValueModifier = Mathf.Clamp(baseVal - targetValue, 0, baseVal);
        }

        void BroadcastRangedValueChange (float newValue, float originalValue) {
            BroadcastValueChange (GameValueChangedComponent.RangedValue, 0, newValue - originalValue);
        }



        bool ModBase (float v, Action<float> onMod) {
            // check entry points to adjust value (if not Set Method)
            GetValueComponents (out float originalBaseValue, out float originalRangedValue);
            onMod(v);
            BroadcastBaseValueChange ( originalBaseValue, originalRangedValue );
            return true;
        }
        bool ModRanged (string methodName, float value, Func<float, float, float, float> onMod) {
            if (!RangedValueModdingOK(methodName)) 
                return false;
            // check entry points to modify value (if not Set ranged value)
            GetValueComponents (out float baseVal, out float originalRangedValue);
            BroadcastRangedValueChange (onMod(value, baseVal, originalRangedValue), originalRangedValue);
            return true;
        }

        public bool ModifyValue (GameValueModifierComponent component, GameValueModifierBehavior behavior, float value) {
            switch (component) 
            {
                case GameValueModifierComponent.Base:
                    switch (behavior)  {
                        case GameValueModifierBehavior.Set: return SetBaseValue(value);
                        case GameValueModifierBehavior.Add: return AddToBaseValue(value);
                        case GameValueModifierBehavior.Multiply: return MultiplyBaseValue(value);
                    }
                    break;
                case GameValueModifierComponent.BaseMod:
                    switch (behavior) {
                        case GameValueModifierBehavior.Set:
                            Debug.LogError("Game Value: '" + name + "': setting base modifiers is not allowed");
                            return false;

                        case GameValueModifierBehavior.Add: return AddToBaseModifier(value);
                        case GameValueModifierBehavior.Multiply: return MultiplyBaseModifier(value);
                    }
                    break;
                case GameValueModifierComponent.Ranged:
                    switch (behavior)  {
                        case GameValueModifierBehavior.Set: return SetRangedValue(value);
                        case GameValueModifierBehavior.Add: return AddToRangedValue(value);
                        case GameValueModifierBehavior.Multiply: return MultiplyRangedValue(value);
                    }
                    break;
            }
            return false;
        }
        
        public bool SetBaseValue (float targetValue) {
            return ModBase(targetValue, (v) => baseValue = v);
        }
        public bool AddToBaseValue (float value) {
            return ModBase(value, (v) => baseValue += v);
        }
        public bool MultiplyBaseValue (float value) {
            return ModBase(value, (v) => baseValue *= v);
        }
        
        public bool AddToBaseModifier (float value) {
            if (BaseModifierModdingOK("AddToBaseModifier")) 
                return ModBase(value, (v) => baseValueAdditiveModifier += v);
                
            return false;
        }
    
        // careful with stacking....
        // to remove multiply by reciprocal of orinal mutliplier value
        public bool MultiplyBaseModifier (float value) {
            if (BaseModifierModdingOK("MultiplyBaseModifier")) 
                return ModBase(value, (v) => baseValueMultiplyModifier *= v);
                
            return false;
        }

        public bool SetRangedValue (float targetValue) {
            return ModRanged("SetRangedValue", targetValue, (v, baseVal, originalRangedValue) => { 
                SetRangedModifierForTargetValue(v, baseVal); 
                return v; 
            });
        }
        public bool AddToRangedValue (float value) {
            return ModRanged("AddToRangedValue", value, (v, baseVal, originalRangedValue) => { 
                rangedValueModifier = Mathf.Clamp(rangedValueModifier - v, 0, baseVal);
                return baseVal - rangedValueModifier;
            });
        }
        public bool MultiplyRangedValue (float value) {
            return ModRanged("MultiplyRangedValue", value, (v, baseVal, originalRangedValue) => { 
                float targetValue = originalRangedValue * v;
                SetRangedModifierForTargetValue(targetValue, baseVal);
                return targetValue;
            });
        }

        /*

        options:
        
        isInt, 
            prevent 0

        */
    }

    
#if UNITY_EDITOR
    
    [CustomPropertyDrawer(typeof(GameValue))] public class GameValueDrawer : PropertyDrawer
    {
        static GUIContent _showAdvancedGUI;
        static GUIContent showAdvancedGUI {
            get {
                if (_showAdvancedGUI == null) _showAdvancedGUI = BuiltInIcons.GetIcon("TerrainInspector.TerrainToolSettings", "Show Advanced");
                return _showAdvancedGUI;
            }
        }
        static GUIContent _cappedGUI;
        static GUIContent cappedGUI {
            get {
                if (_cappedGUI == null) _cappedGUI = BuiltInIcons.GetIcon("AssemblyLock", "Value Ranged");
                return _cappedGUI;
            }
        }
        static GUIContent _useBaseModsGUI;
        static GUIContent useBaseModsGUI {
            get {
                if (_useBaseModsGUI == null) _useBaseModsGUI = BuiltInIcons.GetIcon("preAudioLoopOff", "Use and Keep Track Of Base Modifiers (If false, any modifications to the base value are reset during reinitialization)");
                return _useBaseModsGUI;
            }
        }

        static GUIContent _randomInitGUI;
        static GUIContent randomInitGUI {
            get {
                if (_randomInitGUI == null) _randomInitGUI = BuiltInIcons.GetIcon("Preset.Context", "Random Initialization");
                return _randomInitGUI;
            }
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);

            pos.height = EditorGUIUtility.singleLineHeight;

            float origX = pos.x;
            float origW = pos.width;

            SerializedProperty isRangedProp = prop.FindPropertyRelative("isRangedValue");
            SerializedProperty randomInitProp = prop.FindPropertyRelative("randomInitialization");

            SerializedProperty useBaseModifiersProp = prop.FindPropertyRelative("useBaseModifiers");
                        
            bool randomInit = randomInitProp.boolValue;
            bool isRanged = isRangedProp.boolValue;


            int buttonsCount = 4;//valueCapped ? 4 : 3;

            float allButtonsSpace = GUITools.iconButtonWidth * buttonsCount;

            float spaceWithoutButtons = pos.width - allButtonsSpace;

            // float offset = (allButtonsSpace + (spaceWithoutButtons * .5f));
    
            GUITools.StringFieldWithDefault ( pos.x, pos.y, pos.width - (allButtonsSpace + (spaceWithoutButtons * .5f)), pos.height, prop.FindPropertyRelative("name"), "Value Name");
                        
            randomInit = GUITools.DrawIconToggle(randomInitProp, randomInitGUI, pos.x + (pos.width - GUITools.iconButtonWidth * buttonsCount), pos.y, GUITools.blue, GUITools.white);
            
            buttonsCount--;
            isRanged = GUITools.DrawIconToggle(isRangedProp, cappedGUI, pos.x + (pos.width - GUITools.iconButtonWidth * buttonsCount), pos.y, GUITools.blue, GUITools.white);
            
            buttonsCount--;
            GUITools.DrawIconToggle(useBaseModifiersProp, useBaseModsGUI, pos.x + (pos.width - GUITools.iconButtonWidth * buttonsCount), pos.y, GUITools.blue, GUITools.white);
            
            
            bool showAdvanced = GUITools.DrawIconToggle(prop.FindPropertyRelative("showAdvanced"), showAdvancedGUI, pos.x + (pos.width - GUITools.iconButtonWidth), pos.y, GUITools.blue, GUITools.white);
            
            pos.x += spaceWithoutButtons * .5f;
            
            float labelWidth = 40;
            pos.width = labelWidth;
            EditorGUI.LabelField(pos, new GUIContent("Initial:"));
            pos.x += pos.width;
            
            pos.width = ((spaceWithoutButtons * .5f) - labelWidth) * (randomInit ? .5f : 1);
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("initMin"), GUITools.noContent);
            pos.x += pos.width;

            if (randomInit) {
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("initMax"), GUITools.noContent);
                pos.x += pos.width;
            }
            
            if (isRanged) {
                pos.y += GUITools.singleLineHeight;
                pos.x = origX;
            
                labelWidth = 75;
                float w4 = (origW - labelWidth);// * .5f;

            
                pos.width = labelWidth;
                EditorGUI.LabelField(pos, new GUIContent("Max Value:"));
                pos.x += pos.width;

                pos.width = w4;
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("rangedMax"), GUITools.noContent);
                pos.x += pos.width;
            }
                

            if (showAdvanced) {
                GUITools.StringFieldWithDefault ( origX, pos.y, origW, EditorGUIUtility.singleLineHeight * 3, prop.FindPropertyRelative("description"), "Description...");

                if (Application.isPlaying) {
                    pos.x = origX;
                    pos.y += GUITools.singleLineHeight * 3;
                    pos.width = origW;

                    GUI.enabled = false;
                    EditorGUI.PropertyField( pos, prop.FindPropertyRelative("baseValue"), true);
                    EditorGUI.PropertyField( pos, prop.FindPropertyRelative("baseValueMultiplyModifier"), true);
                    EditorGUI.PropertyField( pos, prop.FindPropertyRelative("baseValueAdditiveModifier"), true);
                    EditorGUI.PropertyField( pos, prop.FindPropertyRelative("rangedValueModifier"), true);
                    GUI.enabled = true;
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {   
            bool isRanged = prop.FindPropertyRelative("isRangedValue").boolValue;
            if (prop.FindPropertyRelative("showAdvanced").boolValue) {
                float h = GUITools.singleLineHeight * (isRanged ? 4 : 3);
                if (Application.isPlaying) 
                    h += GUITools.singleLineHeight * 4;
                
                return h;
            }
            
            return GUITools.singleLineHeight * (isRanged ? 2 : 1);
        }
    }
    
#endif
    
}
