using System.Collections.Generic;
using UnityEngine;
using UnityTools.EditorTools;
using System;

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
                this.values.Add(values[k].GetSaveVersion());
        }
    }
    public class GameValuesContainer : MonoBehaviour, IObjectAttachment {
        
        public int InitializationPhase () {
            return -2;
        }

        public void InitializeDefault () {
            RebuildValuesFromTemplate();           
        }

        public void Strip () {
            BuildOrClearDictionary(0);
        }




        public ObjectAttachmentState GetState () {
            return new GameValuesUnloadedState(gameValues);
        }

        public void LoadState (ObjectAttachmentState state) {
            GameValuesUnloadedState valuesState = state as GameValuesUnloadedState; 
            // BuildOrClearDictionary(valuesState.values.Count);
            for (int i = 0; i< valuesState.values.Count; i++) 
                gameValues[valuesState.values[i].name] = valuesState.values[i];
        }

        // void Awake () {
        //     BuildOrClearDictionary(0);
        //     RebuildValuesFromTemplate();
        // }

        void RebuildValuesFromTemplate () {
            for (int i = 0; i < templates.Length; i++) {
                AddGameValues(templates[i].element);
            }
        }

        [NeatArray] public GameValuesTemplateArray templates;        
        Dictionary<string, GameValue> gameValues;

        void BuildOrClearDictionary (int count) {
            if (gameValues == null)
                gameValues = new Dictionary<string, GameValue>(count);
            else {
                foreach (var k in gameValues.Keys)
                    gameValues[k].OnDispose(GameValueDisposal.Remove);
                gameValues.Clear();
            }
        }

        public void ReinitializeValues (bool removeModifiers) {
            foreach (var k in gameValues.Keys)
                gameValues[k].ReInitialize(removeModifiers);
        }

        public void AddModifiers (GameValueModifier[] modifiers, int count, string description, bool assertPermanent, GameObject subject, GameObject target) {
            GameValue.AddModifiers(gameValues, modifiers, count, description, assertPermanent, subject, target);
        }
        public void RemoveModifiers (GameValueModifier[] modifiers, int count, string description) {
            GameValue.RemoveModifiers(gameValues, modifiers, count, description);
        }

        public void AddGameValues (GameValuesTemplate[] templates, bool logRepeat=true) {
            for (int i = 0; i < templates.Length; i++) 
                AddGameValues(templates[i], logRepeat);
        }
        public void AddGameValues (GameValuesTemplate template, bool logRepeat=true) {
            if (template != null)
                AddGameValues(template.gameValues, logRepeat);
        }
        public void AddGameValues (GameValue[] template, bool logRepeat=true) {
            for (int i = 0; i < template.Length; i++)
                AddGameValue(template[i], logRepeat);
        }

        public void AddGameValue (GameValue template, bool logRepeat=true) {
            string k = template.name;
            if (gameValues.ContainsKey(k)) {
                Debug.LogWarning(this.name + ": adding duplicate game value named: '" + k + "'... only original will be used");
            }
            else {
                gameValues[k] = new GameValue(template);
            }
        }

        public void RemoveGameValues (GameValuesTemplate[] templates) {
            for (int i = 0; i < templates.Length; i++) 
                RemoveGameValues(templates[i]);
        }
        public void RemoveGameValues (GameValuesTemplate template) {
            if (template != null)
                RemoveGameValues(template.gameValues);
        }
        public void RemoveGameValues (GameValue[] template) {
            for (int i = 0; i < template.Length; i++)
                RemoveGameValue(template[i]);
        }
        public void RemoveGameValue (string name) {
            if (HasGameValue(name)) {
                GameValue gv = gameValues[name];
                gameValues.Remove(name);
                gv.OnDispose(GameValueDisposal.Remove);
            }
        }
        public void RemoveGameValue (GameValue template) {
            RemoveGameValue(template.name);
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
        public float GetGameValueComponent (string name, GameValueComponent component) {
            GameValue gv = GetGameValueObject(name);
            if (gv == null) return 0;
            return gv.GetValueComponent(component);
        }

        public float GetGameValue (string name) {
            return GetGameValueComponent(name, GameValueComponent.Value);
        }

        void Update () {
            if (GameManager.isPaused)
                return;
            
            float deltaTime = Time.deltaTime;
            foreach (var k in gameValues.Keys) 
                gameValues[k].UpdateModifiers(deltaTime);
        }
    }
}