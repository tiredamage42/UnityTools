using UnityEngine;

using UnityEditor;
using UnityTools.EditorTools;
using System.Collections.Generic;
namespace UnityTools {

    [System.Serializable] public class GameValueModifierArray : NeatArrayWrapper<GameValueModifier> { }
    [System.Serializable] public class GameValueModifierArray2D { 
        public bool displayed;
        [NeatArray] public GameValueModifierArray[] list; 
    }
    
    public enum GameValueModifierBehavior { Add, Multiply, Set };
        
    [System.Serializable] public class GameValueModifier {
        public bool isPermanent {
            get {
                return modifyValueComponent == GameValueComponent.BaseValue
                    || modifyValueComponent == GameValueComponent.BaseMinValue
                    || modifyValueComponent == GameValueComponent.BaseMaxValue;
            }
        }
        public bool isTemporary {
            get {
                return !isPermanent && modifyDuration > 0;
            }
        }

        public Conditions conditions;
        
        public float modifyDuration;

        public GameValueModifier (GameValueComponent modifyValueComponent, GameValueModifierBehavior modifyBehavior, float modifyValueMin, float modifyValueMax, float modifyDuration) {
            this.modifyValueComponent = modifyValueComponent;
            this.modifyBehavior = modifyBehavior;
            // this.modifyValue = modifyValue;
            this.modifyValueMin = modifyValueMin;
            this.modifyValueMax = modifyValueMax;
            this.modifyDuration = modifyDuration;
            // this.count = 1;
            this.count = 0;
            AddCountsWorth(1);
        }

        public GameValueModifier () {
            count = 1;
        }
        
        public GameValueModifier (GameValueModifier template, int count, int key, string description) {
            this.key = key;

            this.count = 0;
            AddCountsWorth(count);
            // this.count = count;
            this.description = description;

            modifyDuration = template.modifyDuration;
            gameValueName = template.gameValueName;
            modifyValueComponent = template.modifyValueComponent;
            modifyBehavior = template.modifyBehavior;
            // modifyValue = template.modifyValue;
            this.modifyValueMin = template.modifyValueMin;
            this.modifyValueMax = template.modifyValueMax;
        }
            

        public string description;
        [HideInInspector] public int key;
        [HideInInspector] public int count = 1;
        public bool isStackable;
        public string gameValueName = "Game Value Name";
        public GameValueComponent modifyValueComponent;
        public GameValueModifierBehavior modifyBehavior;
        
        // public float modifyValue = 0;
        public float modifyValueMin = 0;
        public float modifyValueMax = 0;
        public List<float> timers = new List<float>();
        
        public bool showConditions;
        int getCount { get { return isStackable ? count : 1; } }

        

        float getModifyValue {
            get {
                if (modifyValueMin == modifyValueMax)
                    return modifyValueMin;
                
                if (!isPermanent)
                    return modifyValueMin;
                
                return Random.Range(modifyValueMin, modifyValueMax);
            }
        }

        public void AddCountsWorth (int countWorth) {
            count += countWorth;
            if (isTemporary) {
                for (int i = 0; i < countWorth; i++) {
                    timers.Add(0);
                }
            }
        }

        public bool RemoveCountsWorth (int countWorth) {
            count -= countWorth;
            if (isTemporary) {
                int li = timers.Count - 1;
                for (int i = 0; i < countWorth; i++) {
                    timers.RemoveAt(li - i);
                }
            }
            return count <= 0;
        }

        public bool UpdateTimers (float deltaTime) {
            for (int i = timers.Count - 1; i >= 0; i--) {
                timers[i] += deltaTime;
                if (timers[i] >= modifyDuration) {
                    timers.RemoveAt(i);
                    count--;
                    if (count <= 0)
                        return true;
                }
            }
            return false;
        }

        public float Modify(float baseValue, List<GameValueModifier> modifyModifiers=null) {
            float modValue = getModifyValue;

            if (modifyBehavior == GameValueModifierBehavior.Set)
                return modValue;
            
            else if (modifyBehavior == GameValueModifierBehavior.Add) 
            {
                if (modValue == 0)
                    return baseValue;

                if (modifyModifiers != null && modifyModifiers.Count > 0) {
                    GameValueComponent modDamageOrBuff = modValue < 0 ? GameValueComponent.IncomingDamage : GameValueComponent.IncomingDamage;

                    float mod = modValue;
                    for (int i = 0; i < modifyModifiers.Count; i++) {
                        if (modifyModifiers[i].modifyValueComponent == modDamageOrBuff) {
                            mod = modifyModifiers[i].Modify(mod, null);
                        }
                    }
                    
                    return baseValue + (mod * getCount);
                }
                else {
                    return baseValue + (modValue * getCount);
                }
            }
            else if (modifyBehavior == GameValueModifierBehavior.Multiply)
            {
                if (modValue >= 1) {
                    return baseValue * (modValue * getCount);
                }
                else {
                    float mod = 1;
                    
                    for (int i = 0; i < getCount; i++) 
                        mod *= modValue;

                    return baseValue * mod;
                }
            }
            return baseValue;
        }
    }
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(GameValueModifier))] public class GameValueModifierDrawer : PropertyDrawer
    {
        static GUIContent _stackableContent;
        static GUIContent stackableContent { get { 
            if (_stackableContent == null) _stackableContent = BuiltInIcons.GetIcon("UnityEditor.SceneHierarchyWindow", "Is Stackable"); 
            return _stackableContent;
        } }

        static GUIContent _showConditionsContent;
        static GUIContent showConditionsContent { get { 
            if (_showConditionsContent == null) _showConditionsContent = new GUIContent("?", "Show Conditions"); 
            return _showConditionsContent;
        } }
        static GUIContent _modifyTimeContent;
        static GUIContent modifyTimeContent { get { 
            if (_modifyTimeContent == null) _modifyTimeContent = BuiltInIcons.GetIcon("UnityEditor.ProfilerWindow", "Modify Duration In Seconds (Value 0 and below are non temporary)"); 
            return _modifyTimeContent;
        } }

        
        // static readonly float[] widths = new float[] { 60, 100, 90, 80, 60, 15, };
        
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {

            EditorGUI.BeginProperty(pos, label, prop);

            pos.height = EditorGUIUtility.singleLineHeight;

            // int i = 0;

            float origX = pos.x;
            float origW = pos.width;
            
            pos.width = 60;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("modifyBehavior"), GUITools.noContent);
            pos.x += pos.width;
                        
            pos.width = 100;
            GUITools.StringFieldWithDefault ( pos.x, pos.y, 100, EditorGUIUtility.singleLineHeight, prop.FindPropertyRelative("gameValueName"), "Value Name");
            pos.x += pos.width;
            

            SerializedProperty modifyComponentProp = prop.FindPropertyRelative("modifyValueComponent");

            pos.width = 100;
            EditorGUI.PropertyField(pos, modifyComponentProp, GUITools.noContent);
            pos.x += pos.width;
            
            GameValueComponent component = (GameValueComponent)modifyComponentProp.enumValueIndex;
            
            bool isPermanent = component == GameValueComponent.BaseValue
                            || component == GameValueComponent.BaseMinValue
                            || component == GameValueComponent.BaseMaxValue;


            GUITools.DrawToolbarDivider(pos.x, pos.y);
            pos.x += GUITools.toolbarDividerSize;
            
            pos.width = 70;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("modifyValueMin"), GUITools.noContent);
            pos.x += pos.width;

            if (isPermanent) {
                pos.width = 70;
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("modifyValueMax"), GUITools.noContent);
                pos.x += pos.width;
            }

            GUITools.DrawToolbarDivider(pos.x, pos.y);
            pos.x += GUITools.toolbarDividerSize;

            
            

            
            if (!isPermanent) {
                GUITools.DrawToolbarDivider(pos.x, pos.y);
                pos.x += GUITools.toolbarDividerSize;

                GUITools.DrawToggleButton(false, modifyTimeContent, pos.x, pos.y, GUITools.blue, GUITools.white);
                pos.x += GUITools.iconButtonWidth;

                pos.width = 50;
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("modifyDuration"), GUITools.noContent);
                pos.x += pos.width;
            }

            GUITools.DrawToolbarDivider(pos.x, pos.y);
            pos.x += GUITools.toolbarDividerSize;
                
            
            GUITools.DrawToggleButton(prop.FindPropertyRelative("isStackable"), stackableContent, pos.x, pos.y, GUITools.blue, GUITools.white);
            pos.x += GUITools.iconButtonWidth;

            SerializedProperty showConditions = prop.FindPropertyRelative("showConditions");
            SerializedProperty conditionsProp = prop.FindPropertyRelative("conditions");

            if (!showConditions.boolValue) {
                if (conditionsProp.FindPropertyRelative("list").arraySize > 0) {
                    showConditions.boolValue = true;
                }
            }

            GUITools.DrawToggleButton(showConditions, showConditionsContent, pos.x, pos.y, GUITools.blue, GUITools.white);
            
            if (showConditions.boolValue) {
                pos.x = origX;
                pos.y += EditorGUIUtility.singleLineHeight;
                pos.width = origW;
                EditorGUI.PropertyField(pos, conditionsProp, new GUIContent("Conditions"));
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            return GUITools.singleLineHeight + (prop.FindPropertyRelative("showConditions").boolValue ? EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("conditions"), true) : 0);
        }
    }
#endif
}
