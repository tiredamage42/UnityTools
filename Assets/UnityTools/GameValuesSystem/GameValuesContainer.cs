using System.Collections.Generic;
using UnityEngine;
using UnityTools.EditorTools;
using System;


using UnityTools.DevConsole;
/*
    
    perks shouldnt adjust inventory for now...

*/

/*

on enable () {

    // order doent matter (everything should be at 0 anyways)
    strip down ();
        invnetory
        perks
        game values

    if (!loading)
        rebuild from templates ();
            game values // needs to be first, as perks and inventory might add modifiers that depend on 
                        // template values

            perks    // might add game values that inventory depends on   

            invneotry   // last, shouldnt add any game values... 
                        // and might depend on perk values for conditions for adding modifiers, etc
                        // might add perks on equip / store

    if loading:
        load ();
            game values
            perks (if adding game values, should already be loaded, but still add modifiers)
            inventory ( if adding game values or perks via store/equip, should already be loaded)


}


*/

namespace UnityTools {
    
    /*
        module for scripts to be able to use game values
    */

    [System.Serializable] public class GameValuesUnloadedState : ObjectAttachmentState {

        public List<GameValue> values;

        public GameValuesUnloadedState (Dictionary<string, GameValue> values) {
            this.values = new List<GameValue>();
            foreach (var k in values.Keys)
                this.values.Add(values[k]);
        }
    }
    public class GameValuesContainer : MonoBehaviour, IObjectAttachment {
        
        public int InitializationPhase () {
            return -2;
        }

        void Awake () {
            gameValues = new Dictionary<string, GameValue>();
            RebuildValuesFromTemplate();
        }
        public void InitializeDefault () {
            ReinitializeValues();
        }

        public void Strip () {

        }




        public ObjectAttachmentState GetState () {
            return new GameValuesUnloadedState(gameValues);
        }

        public void LoadState (ObjectAttachmentState state) {
            GameValuesUnloadedState valuesState = state as GameValuesUnloadedState; 
            for (int i = 0; i< valuesState.values.Count; i++) 
                gameValues[valuesState.values[i].name] = valuesState.values[i];
        }

        void RebuildValuesFromTemplate () {
            for (int i = 0; i < templates.Length; i++) {
                AddGameValues(templates[i].element);
            }
        }

        [NeatArray] public GameValuesTemplateArray templates;        
        Dictionary<string, GameValue> gameValues;

        
        public void ReinitializeValues () {
            foreach (var k in gameValues.Keys)
                gameValues[k].ReInitialize();
        }

        
        [Command("gvmod", "modify game value [Base, BaseMod, Ranged], [Set, Add, Multiply]", "Game Values", true)]    
        public bool ModifyValue (string name, GameValueModifierComponent component, GameValueModifierBehavior behavior, float value) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) 
                return false;
            return gv.ModifyValue(component, behavior, value);
        }


        public bool SetBaseValue (string name, float value) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) 
                return false;
            return gv.SetBaseValue(value);
        }
        
        public bool AddToBaseValue (string name, float value) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) 
                return false;
            return gv.AddToBaseValue(value);
        }
        
        public bool MultiplyBaseValue (string name, float value) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) 
                return false;
            return gv.MultiplyBaseValue(value);
        }
        
        public bool AddToBaseModifier (string name, float value) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) 
                return false;
            return gv.AddToBaseModifier(value);
        }
    
        // careful with stacking....
        // to remove multiply by reciprocal of orinal mutliplier value
        
        public bool MultiplyBaseModifier (string name, float value) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) 
                return false;
            return gv.MultiplyBaseModifier(value);
        }
        

        public bool SetRangedValue (string name, float value) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) 
                return false;
            return gv.SetRangedValue(value);
        }

        
        public bool AddToRangedValue (string name, float value) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) 
                return false;
            return gv.AddToRangedValue(value);
        }

        public bool MultiplyRangedValue (string name, float value) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) 
                return false;
            return gv.MultiplyRangedValue(value);
        }


        void AddGameValues (GameValuesTemplate[] templates, bool logRepeat=true) {
            for (int i = 0; i < templates.Length; i++) 
                AddGameValues(templates[i], logRepeat);
        }
        void AddGameValues (GameValuesTemplate template, bool logRepeat=true) {
            if (template != null)
                AddGameValues(template.gameValues, logRepeat);
        }
        void AddGameValues (GameValue[] template, bool logRepeat=true) {
            for (int i = 0; i < template.Length; i++)
                AddGameValue(template[i], logRepeat);
        }

        void AddGameValue (GameValue template, bool logRepeat=true) {
            string k = template.name;
            if (gameValues.ContainsKey(k)) {
                Debug.LogWarning(this.name + ": adding duplicate game value named: '" + k + "'... only original will be used");
            }
            else {
                gameValues[k] = new GameValue(template);
            }
        }

        public bool HasGameValue (string name) {
            return gameValues.ContainsKey(name);
        }

        public GameValue GetGameValueObject (string name) {
            if (!HasGameValue(name)) {
                Debug.Log(this.name + " does not have a game value named "+ name);
                return null;
            }
            return gameValues[name];
        }


        [Command("gvvalue", "get game value value", "Game Values", true)]    
        public float GetGameValue (string name) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) return 0;
            return gv.GetValue();
        }
            
        [Command("gvbasevalue", "get game value base value", "Game Values", true)]    
        public float GetBaseGameValue (string name) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) return 0;
            return gv.GetBaseValue();
        }
    }
}