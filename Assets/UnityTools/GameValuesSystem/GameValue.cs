using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections;
using UnityTools.EditorTools;

using UnityEditor;

namespace UnityTools {




    [System.Serializable] public class GameValueArray : NeatArrayWrapper<GameValue> { }
    [System.Serializable] public class GameValueList : NeatListWrapper<GameValue> { }
    
    public enum GameValueComponent { 
        BaseValue = 0, 
        BaseMinValue = 1, 
        BaseMaxValue = 2, 
        Value = 3, 
        MinValue = 4, 
        MaxValue = 5,
        IncomingDamage = 6, 
        IncomingBuff = 7
    };

    public enum GameValueDisposal {
        Replace, Remove
    }
        
    
    /*
        a float value wrapper,

        we can cap it dynamically, and add modifiers to it
    */
    [System.Serializable] public class GameValue
    {


        static GameValue GetGameValue (Dictionary<string, GameValue> gameValues, string name) {
            if (gameValues.ContainsKey(name)) return gameValues[name];
            return null;
        }

        public static void AddModifiers (Dictionary<string, GameValue> gameValues, GameValueModifier[] mods, int count, string description, bool assertPermanent, GameObject subject, GameObject target) {
            for (int i =0 ; i < mods.Length; i++) {
                if (assertPermanent && (!mods[i].isPermanent && !mods[i].isTemporary)) continue;
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



        public string name;
        [TextArea] public string description;
        public float baseValue;
        public bool randomInitialization;
        public float initMin, initMax;
        public bool rollOver;
        public bool valueCapped;
        public float capMin, capMax = 500;
        //not using a dictionary in order to keep thses serializable by unity
        [HideInInspector] public List<GameValueModifier> modifiers = new List<GameValueModifier>();
        public bool showAdvanced;


        public GameValue GetSaveVersion () {
            GameValue r = new GameValue(name, description);
            r.baseValue = baseValue;
            r.randomInitialization = randomInitialization;
            r.initMin = initMin;
            r.initMax = initMax;

            r.SetCaps (valueCapped, rollOver, capMin, capMax);

            // Don’t serialize saved modifiers with game values, unless they’re temporary
            // Rebuilding perks/inventory should add them back when loading...
            // or else we risk having modifiers hanging around without anyone to remove them
            List<GameValueModifier> mods = new List<GameValueModifier>();
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].isTemporary) {
                    mods.Add(modifiers[i]);
                }
            }
            r.modifiers = mods;
            return r;
        }
        
        public GameValue (string name, string description) {
            SetNameAndDescription (name, description);
        }

        void SetNameAndDescription (string name, string description) {
            this.name = name;
            this.description = description;
        }


        void InitializeGameValue (string name, float baseValue, string description) {
            SetNameAndDescription (name, description);
            initMax = initMin = baseValue;
            ReInitialize(true);
        }

        void SetCaps (bool capped, bool rollOver, float capMin, float capMax) {
            this.valueCapped = capped;
            this.rollOver = rollOver;
            this.capMin = capMin;
            this.capMax = capMax;
        }

        public GameValue(string name, float baseValue, Vector2 baseMinMax, bool rollOver, string description){
            InitializeGameValue (name, baseValue, description);
            SetCaps (true, rollOver, baseMinMax.x, baseMinMax.y);
        }
        public GameValue(string name, float baseValue, string description){
            InitializeGameValue (name, baseValue, description);
        }
        public GameValue (GameValue template) {
            SetNameAndDescription (template.name, template.description);
            this.randomInitialization = template.randomInitialization;
            this.initMin = template.initMin;
            this.initMax = template.initMax;

            ReInitialize(true);
            SetCaps (template.valueCapped, template.rollOver, template.capMin, template.capMax);
        }
            
        public void ReInitialize (bool removeModifiers) {
            if (randomInitialization) {
                this.baseValue = UnityEngine.Random.Range(initMin, initMax);
            }
            else {
                this.baseValue = initMin;
            }
            if (removeModifiers) {
                if (modifiers.Count > 0)
                    modifiers.Clear();
            }
        }


        // gameValue, 
        // componenet change

        // component delta
        // component base delta
        // component base delta uncapped

        // current base
        // current calculated

        // min base
        // min calculated

        // max base
        // max calculated


        public delegate void ChangeListener (
            GameValue valueChanged, 
            GameValueComponent componentChanged,
            float componentDelta, 
            float componentDeltaUnclamped, 
            bool passedLowerLimit, bool passedUpperLimit, 
            Dictionary <GameValueComponent, float> gameValueInfo,
            bool isRolloverChange
        );

        event Action<GameValue, GameValueDisposal> onDisposed;
        List<Action<GameValue, GameValueDisposal>> onDisposedCallbacks = new List<Action<GameValue, GameValueDisposal>>();

        void SubscribeToDispose (Action<GameValue, GameValueDisposal> callback) {
            onDisposed += callback;
            onDisposedCallbacks.Add(callback);
        }


        void SubscribeToChangeListener (ChangeListener callback) {
            onValueChange += callback;
            onValueChangeCallbacks.Add(callback);
        }

        event ChangeListener onValueChange;
        List<ChangeListener> onValueChangeCallbacks = new List<ChangeListener>();

        public void RemoveChangeListener (ChangeListener callback) {
            onValueChange -= callback;
            onValueChangeCallbacks.Remove(callback);
        }


        public void OnDispose (GameValueDisposal disposal) {
            if (onDisposed != null)
                onDisposed(this, disposal);

            for (int i = 0; i < onDisposedCallbacks.Count; i++) onDisposed -= onDisposedCallbacks[i];
            onDisposedCallbacks.Clear();

            for (int i = 0; i < onValueChangeCallbacks.Count; i++) onValueChange -= onValueChangeCallbacks[i];
            onValueChangeCallbacks.Clear();
        }

            

        public void AddChangeListener (ChangeListener listener, Action<GameValue, GameValueDisposal> onDisposed, bool callListener) {
            
            SubscribeToChangeListener(listener);
            SubscribeToDispose(onDisposed);
            
            if (callListener) {
                Dictionary<GameValueComponent, float> gameValueInfo = new Dictionary<GameValueComponent, float>() {
                    { GameValueComponent.Value, GetValue() },
                    { GameValueComponent.MinValue, GetMinValue(false) },
                    { GameValueComponent.MaxValue, GetMaxValue(false) },
                    { GameValueComponent.BaseValue, baseValue },
                    { GameValueComponent.BaseMinValue, valueCapped ? capMin : 0 },
                    { GameValueComponent.BaseMaxValue, valueCapped ? capMax : 0 },
                };
                    
                // TODO: maybe change component to an "ALL" componenet...
                listener (this, GameValueComponent.BaseValue, 0, 0, false, false, gameValueInfo, false);
            }
        }

        
        // delta, current, min, max
        // event Action<GameValue, GameValue.GameValueComponent, float, float, float, float, float, bool> onValueChange;
        // public void AddChangeListener (Action<GameValue, GameValue.GameValueComponent, float, float, float, float, float, bool>  listener) {
        //     onValueChange += listener;
        // }
        // public void RemoveChangeListener (Action<GameValue, GameValue.GameValueComponent, float, float, float, float, float, bool>  listener) {
        //     onValueChange -= listener;
        // }

        

        public static string ModifyBehaviorString (GameValueModifierBehavior modifyBehavior) {
            if (modifyBehavior == GameValueModifierBehavior.Set)
                return "Set";
            else if (modifyBehavior == GameValueModifierBehavior.Add)
                return "+";
            else if (modifyBehavior == GameValueModifierBehavior.Multiply) 
                return "x";
            return "";
        }
    
        public string GetModifiersSummary () {
            string r = "";

            for (int i = 0; i < modifiers.Count; i++) {
                GameValueModifier m = modifiers[i];
                r +=    m.description + ": " +  
                        m.modifyValueComponent + " " + 
                        ModifyBehaviorString(m.modifyBehavior) + 
                        (m.isPermanent ? ("(" + m.modifyValueMin + "-" + m.modifyValueMax + ")") : m.modifyValueMin.ToString()) +
                        // m.modifyValue + 
                        (m.isStackable ? "(" + m.count + ")" : "") + "\n";
            }

            return r;
        }


        

        float GetModifiedValue (GameValueComponent checkType, float value, bool clamp, float min = float.MinValue, float max = float.MaxValue) {
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].modifyValueComponent == checkType) {
                    value = modifiers[i].Modify(value);
                }
            }
            if (clamp)
                return Mathf.Clamp(value, min, max);
            
            return value;
        }
        public float GetValue () {
            return GetModifiedValue (GameValueComponent.Value, baseValue, valueCapped, GetMinValue(false), GetMaxValue(false));
        }
        public float GetMinValue (bool showUncappedWarning=true) {
            if (valueCapped) return GetModifiedValue(GameValueComponent.MinValue, capMin, false);
            if (showUncappedWarning) Debug.LogWarning("Game Value '" + name + "' is uncapped, GetMinValue will always return 0");
            return 0;
        }
            
        public float GetMaxValue (bool showUncappedWarning=true) {
            if (valueCapped) return GetModifiedValue(GameValueComponent.MaxValue, capMax, false);
            if (showUncappedWarning) Debug.LogWarning("Game Value '" + name + "' is uncapped, GetMaxValue will always return 0");   
            return 0;
        }
            
        public float GetValueComponent (GameValueComponent checkType, bool showUncappedWarning=true) {
            switch (checkType) {
                case GameValueComponent.Value:
                    return GetValue();
                case GameValueComponent.MinValue:
                    return GetMinValue(showUncappedWarning);
                case GameValueComponent.MaxValue:
                    return GetMaxValue(showUncappedWarning);
                case GameValueComponent.BaseValue:
                    return baseValue;
                case GameValueComponent.BaseMinValue:
                    if (!valueCapped) {
                        if (showUncappedWarning) Debug.LogWarning("Game Value '" + name + "' is uncapped, BaseMinValue will always return 0");
                        return 0;
                    }
                    return capMin;
                case GameValueComponent.BaseMaxValue:
                    if (!valueCapped) {
                        if (showUncappedWarning) Debug.LogWarning("Game Value '" + name + "' is uncapped, BaseMaxValue will always return 0");
                        return 0;
                    }
                    return capMax;
                case GameValueComponent.IncomingBuff:
                    Debug.LogWarning("Game Value '" + name + "': cant get GameValueComponent.IncomingBuff, will always return 0");
                    return 0;
                case GameValueComponent.IncomingDamage:
                    Debug.LogWarning("Game Value '" + name + "': cant get GameValueComponent.IncomingBuff, will always return 0");
                    return 0;
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

        void BroadcastValueChange (
            // GameValueComponent componentChanged, float origValue, float uncappedDelta, float minVal, float maxVal, bool isRollover
                GameValueComponent componentChanged, float componentDelta, float componentDeltaUnclamped, bool passedLowerLimit, bool passedUpperLimit, bool isRolloverChange
        ) {
            if (onValueChange != null) {
                
                Dictionary<GameValueComponent, float> gameValueInfo = new Dictionary<GameValueComponent, float>() {
                    { GameValueComponent.Value, GetValue() },
                    { GameValueComponent.MinValue, GetMinValue(false) },
                    { GameValueComponent.MaxValue, GetMaxValue(false) },
                    { GameValueComponent.BaseValue, baseValue },
                    { GameValueComponent.BaseMinValue, valueCapped ? capMin : 0 },
                    { GameValueComponent.BaseMaxValue, valueCapped ? capMax : 0 },
                };
                    
                onValueChange (this, componentChanged, componentDelta, componentDeltaUnclamped, passedLowerLimit, passedUpperLimit, gameValueInfo, isRolloverChange);
                
                // delta, current, min, max
                // float newVal = GetValue();
                // onValueChange(this, componentChange, newVal - origValue, uncappedDelta, newVal, minVal, maxVal, isRollover);
            }
        }

        List<GameValueModifier> GetModifiersByIncoming () {
            List<GameValueModifier> incomingChangeModifiers = null; 
            for (int i = 0; i < modifiers.Count; i++) {
                if (modifiers[i].modifyValueComponent == GameValueComponent.IncomingDamage || modifiers[i].modifyValueComponent == GameValueComponent.IncomingBuff) {
                    if (incomingChangeModifiers == null)
                        incomingChangeModifiers = new List<GameValueModifier>();
                    incomingChangeModifiers.Add(modifiers[i]);
                }
            }
            return incomingChangeModifiers;
        }

        void ModifyPermanent (GameValueModifier modifier, bool isRolloverChange) {
            
            
            // float origValue = GetValue();
            float origComponentValue = GetValueComponent(modifier.modifyValueComponent, false);

            // float origBaseValue = baseValue;


            List<GameValueModifier> incomingChangeModifiers = null;
            if (modifier.modifyBehavior == GameValueModifierBehavior.Add) {
                incomingChangeModifiers = GetModifiersByIncoming();
                // // incoming damage
                // if (modifier.modifyValue < 0) {
                //     incomingChangeModifiers = GetModifiersByType(GameValueComponent.IncomingDamage);
                // }
                // // incoming buff
                // else if (modifier.modifyValue > 0) {
                //     incomingChangeModifiers = GetModifiersByType(GameValueComponent.IncomingBuff);
                // }
            }

            if (modifier.modifyValueComponent == GameValueComponent.BaseValue) {   
                baseValue = modifier.Modify(baseValue, incomingChangeModifiers);
            }



            if (modifier.modifyValueComponent == GameValueComponent.BaseMinValue) {
                if (!valueCapped) Debug.LogWarning("Game Value '" + name + "' is uncapped, BaseMinValue cant be modified");
                else capMin = modifier.Modify(capMin, incomingChangeModifiers);
            }
            if (modifier.modifyValueComponent == GameValueComponent.BaseMaxValue) {
                if (!valueCapped) Debug.LogWarning("Game Value '" + name + "' is uncapped, BaseMaxValue cant be modified");
                else capMax = modifier.Modify(capMax, incomingChangeModifiers);
            }

            float newComponentValue = GetValueComponent(modifier.modifyValueComponent, false);


            float uncappedDelta = newComponentValue - origComponentValue;

            float cappedDelta = uncappedDelta;

            bool doRollover = false;
            bool passedUpperLimit = false;
            float rolloverValue = 0;
            
            if (valueCapped) {
                float minVal = GetMinValue(false);
                float maxVal = GetMaxValue(false);
                
                if (rollOver && modifier.modifyValueComponent == GameValueComponent.BaseValue) {
                    passedUpperLimit = baseValue >= maxVal;
                    doRollover = passedUpperLimit || baseValue < minVal;
                    if (doRollover) 
                        rolloverValue = baseValue - (passedUpperLimit ? maxVal : minVal);
                }

                //clamp the base value
                baseValue = Mathf.Clamp(baseValue, minVal, maxVal);

                cappedDelta = GetValueComponent(modifier.modifyValueComponent, false) - origComponentValue;
            }



            // float minVal = GetMinValue(false);
            // float maxVal = GetMaxValue(false);


                            
            // float uncappedDelta = baseValue - origBaseValue;
            // if (valueCapped) {
            //     if (modifier.modifyValueComponent == GameValueComponent.BaseValue) {
            //         if (rollOver) {
            //             if (baseValue >= maxVal) {
            //                 doRollover = true;
            //                 rolloverValue = baseValue - maxVal;
            //                 rolloverPos = true;
            //             }
            //             else if (baseValue < minVal) {
            //                 doRollover = true;
            //                 rolloverValue = baseValue - minVal;
            //             }
            //         }
            //     }
            //     //clamp the base value
            //     baseValue = Mathf.Clamp(baseValue, minVal, maxVal);
            // }

            BroadcastValueChange ( modifier.modifyValueComponent, cappedDelta, uncappedDelta, doRollover && !passedUpperLimit, doRollover && passedUpperLimit, isRolloverChange );

            // BroadcastValueChange ( modifier.modifyValueComponent, origValue, uncappedDelta, minVal, maxVal, isRollover );

            if (doRollover) {
                // recalculate maximums as they could have been changed when BroadcastValueChange called
                float minVal = GetMinValue(false);
                float maxVal = GetMaxValue(false);
                
                /*
                if we're rolling 'up', we loop back around to the min value
                
                if we're rolling down set to max value, that and subtract from there 
                (rollover value will always be < 0 since we check if base value is below min)
                so this way we dont land on max val and 'rollover' back up...
                */
                baseValue = passedUpperLimit ? minVal : maxVal;


                if (rolloverValue != 0) {
                    ModifyPermanent(
                        new GameValueModifier(
                            GameValueComponent.BaseValue, 
                            GameValueModifierBehavior.Add,
                            rolloverValue, rolloverValue, 0
                        ), true
                    );

                }
            }
        }


        // IEnumerator RemoveModifierAfterSeconds (GameValueModifier modifier, int count, float seconds) {
        //     yield return new WaitForSeconds(seconds);

        //     // dont doanything if already removed...
        //     if (modifier.count > 0)
        //         RemoveFoundModifier(modifier, count);
        // }

        

        void AddToOrAddModifier (GameValueModifier modifier, int count, int key, string description) {
            GameValueModifier existingModifier = GetModifier ( key );
            if (existingModifier != null) {
                if (existingModifier.description != description) {
                    Debug.LogWarning("Game Value '" + name + "' Description mismatch for same key! 1) " + description + " :: 2) " + existingModifier.description);
                }

                existingModifier.AddCountsWorth(count);
                // existingModifier.count += count;
            }
            else {
                // existingModifier = new GameValueModifier(modifier, count, key, description);
                modifiers.Add(new GameValueModifier(modifier, count, key, description));
            }
            // if (existingModifier.isTemporary) {

            //     existingModifier.StartTimer(count);

            //     // IEnumerator removeCoroutine = RemoveModifierAfterSeconds(existingModifier, count, existingModifier.modifyTime);
            //     // UpdateManager.instance.StartCoroutine(removeCoroutine);
            // }
        }

        // anything modifying base values is permanent, and doesnt get stored in 
        // our modifiers list
        public void AddModifier (GameValueModifier modifier, int count, int key, string description) {
            if (modifier.isPermanent) {
                ModifyPermanent(modifier, false);
                return;
            }

            // float origValue = GetValue();

            if (!valueCapped) {
                if (modifier.modifyValueComponent == GameValueComponent.MinValue) {
                    Debug.LogWarning("Game Value '" + name + "' is uncapped, MinValue cant be modified");
                    return;
                }
                if (modifier.modifyValueComponent == GameValueComponent.MaxValue) {
                    Debug.LogWarning("Game Value '" + name + "' is uncapped, MaxValue cant be modified");
                    return;
                }   
            }

            float delta = 0;
            if (modifier.modifyValueComponent == GameValueComponent.IncomingBuff || modifier.modifyValueComponent == GameValueComponent.IncomingDamage) {
                AddToOrAddModifier ( modifier, count, key, description );
            }
            else {
                float origComponentValue = GetValueComponent(modifier.modifyValueComponent, false);
                AddToOrAddModifier ( modifier, count, key, description );
                delta = GetValueComponent(modifier.modifyValueComponent, false) - origComponentValue;
            }
            BroadcastValueChange (modifier.modifyValueComponent, delta, delta, false, false, false );
        }

        void RemoveFoundModifier (GameValueModifier existingModifier, int count) {
            
            if (existingModifier.RemoveCountsWorth(count))
                modifiers.Remove(existingModifier);
            
            // existingModifier.count -= count;
            // if (existingModifier.count <= 0) {
            //     modifiers.Remove(existingModifier);
            // }
        }

        public void UpdateModifiers (float deltaTime) {
            for (int i = modifiers.Count - 1; i >= 0; i--) {
                if (modifiers[i].isTemporary) {
                    if (modifiers[i].UpdateTimers(deltaTime)) {
                        modifiers.Remove(modifiers[i]);
                    }
                }
            }
        }


        public void RemoveModifier (GameValueModifier modifier, int count, int key){
            if (modifier.isPermanent) return;
            
            GameValueModifier existingModifier = GetModifier ( key );
            if (existingModifier != null) {
                float delta = 0;
                if (modifier.modifyValueComponent == GameValueComponent.IncomingBuff || modifier.modifyValueComponent == GameValueComponent.IncomingDamage) {
                    RemoveFoundModifier (existingModifier, count);
                }
                else {
                    float origComponentValue = GetValueComponent(modifier.modifyValueComponent, false);
                    RemoveFoundModifier (existingModifier, count);
                    delta = GetValueComponent(modifier.modifyValueComponent, false) - origComponentValue;
                }
                BroadcastValueChange (modifier.modifyValueComponent, delta, delta, false, false, false);
            }
        }
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
        static GUIContent _rolloverGUI;
        static GUIContent rolloverGUI {
            get {
                if (_rolloverGUI == null) _rolloverGUI = BuiltInIcons.GetIcon("preAudioLoopOff", "Value Rolls Over After Reaching Clamped Thresholds");
                return _rolloverGUI;
            }
        }


        static GUIContent _randomInitGUI;
        static GUIContent randomInitGUI {
            get {
                if (_randomInitGUI == null) _randomInitGUI = BuiltInIcons.GetIcon("Preset.Context", "Random Initialization");
                return _randomInitGUI;
            }
        }

        void PrintModifiersSummary (Rect pos, SerializedProperty prop) {
            
            SerializedProperty modifiers = prop.FindPropertyRelative("modifiers");
            
            for (int i = 0; i < modifiers.arraySize; i++) {
                SerializedProperty m = modifiers.GetArrayElementAtIndex(i);

                SerializedProperty modifyComponentProp = m.FindPropertyRelative("modifyValueComponent");
                GameValueComponent component = (GameValueComponent)modifyComponentProp.enumValueIndex;

                bool isPermanent = component == GameValueComponent.BaseValue
                                || component == GameValueComponent.BaseMinValue
                                || component == GameValueComponent.BaseMaxValue;
            




                
                EditorGUI.LabelField(pos, 
                    m.FindPropertyRelative("description").stringValue + ": " +  
                    ((GameValueComponent)m.FindPropertyRelative("modifyValueComponent").enumValueIndex) + " " + 
                    GameValue.ModifyBehaviorString((GameValueModifierBehavior)m.FindPropertyRelative("modifyBehavior").enumValueIndex) + 
                    
                    // m.FindPropertyRelative("modifyValue").floatValue + 
                    (isPermanent ? ("(" + m.FindPropertyRelative("modifyValueMin").floatValue + "-" + m.FindPropertyRelative("modifyValueMax").floatValue + ")") : m.FindPropertyRelative("modifyValueMin").floatValue.ToString()) +
                        
                    
                    
                    (m.FindPropertyRelative("isStackable").boolValue ? "(" + m.FindPropertyRelative("count").intValue + ")" : "")
                );
                pos.y += GUITools.singleLineHeight;
            }            
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);

            pos.height = EditorGUIUtility.singleLineHeight;

            float origX = pos.x;
            float origW = pos.width;


            SerializedProperty valueCappedProp = prop.FindPropertyRelative("valueCapped");
            SerializedProperty randomInitProp = prop.FindPropertyRelative("randomInitialization");
            
            
            bool randomInit = randomInitProp.boolValue;
            bool valueCapped = valueCappedProp.boolValue;


            int buttonsCount = valueCapped ? 4 : 3;

            float allButtonsSpace = GUITools.iconButtonWidth * buttonsCount;

            float spaceWithoutButtons = pos.width - allButtonsSpace;

            // float offset = (allButtonsSpace + (spaceWithoutButtons * .5f));
    
            GUITools.StringFieldWithDefault ( pos.x, pos.y, pos.width - (allButtonsSpace + (spaceWithoutButtons * .5f)), pos.height, prop.FindPropertyRelative("name"), "Value Name");
                        
            randomInit = GUITools.DrawToggleButton(randomInitProp, randomInitGUI, pos.x + (pos.width - GUITools.iconButtonWidth * buttonsCount), pos.y, GUITools.blue, GUITools.white);
            buttonsCount--;
            valueCapped = GUITools.DrawToggleButton(valueCappedProp, cappedGUI, pos.x + (pos.width - GUITools.iconButtonWidth * buttonsCount), pos.y, GUITools.blue, GUITools.white);
            
            // buttonsCount--;
            if (valueCapped) {
                GUITools.DrawToggleButton(prop.FindPropertyRelative("rollOver"), rolloverGUI, pos.x + (pos.width - GUITools.iconButtonWidth * 2), pos.y, GUITools.blue, GUITools.white);
                // buttonsCount--;
            }

            
            bool showAdvanced = GUITools.DrawToggleButton(prop.FindPropertyRelative("showAdvanced"), showAdvancedGUI, pos.x + (pos.width - GUITools.iconButtonWidth), pos.y, GUITools.blue, GUITools.white);
            
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
            

            if (valueCapped) {
                pos.y += GUITools.singleLineHeight;
                pos.x = origX;
            
                labelWidth = 75;
                float w4 = (origW - labelWidth) * .5f;

            
                pos.width = labelWidth;
                EditorGUI.LabelField(pos, new GUIContent("Min/Max:"));
                pos.x += pos.width;

                pos.width = w4;
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("capMin"), GUITools.noContent);
                pos.x += pos.width;
                
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("capMax"), GUITools.noContent);
                pos.x += pos.width;
            }

            
            if (showAdvanced) {
                GUITools.StringFieldWithDefault ( origX, pos.y, origW, EditorGUIUtility.singleLineHeight * 3, prop.FindPropertyRelative("description"), "Description...");

                if (Application.isPlaying) {
                    pos.x = origX;
                    pos.y += GUITools.singleLineHeight * 3;
                    pos.width = origW;
                    
                    EditorGUI.PropertyField( pos, prop.FindPropertyRelative("baseValue"), true);
                    
                    pos.y += GUITools.singleLineHeight;
                    PrintModifiersSummary (pos, prop);
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            if (prop.FindPropertyRelative("showAdvanced").boolValue) {
                float h = GUITools.singleLineHeight * (prop.FindPropertyRelative("valueCapped").boolValue ? 4 : 3);
                if (Application.isPlaying) {
                    h += GUITools.singleLineHeight;
                    h += GUITools.singleLineHeight * prop.FindPropertyRelative("modifiers").arraySize;
                }
                return h;
            }
            
            return GUITools.singleLineHeight * (prop.FindPropertyRelative("valueCapped").boolValue ? 2 : 1);
        }
    }
    
#endif
    
}
