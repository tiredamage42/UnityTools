using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityTools;
using UnityTools.EditorTools;

namespace UnityToolsDemo {
    public class TestConditions : MonoBehaviour {
        [NeatArray] public GameValueModifierArray modifiers;

        [NeatArray] public GameValueArray gameValues;

        Dictionary<string, GameValue> gameValuesDict;

        bool modsAdded;
        void AddRemoveModifiersModifiers () {
            MakeValuesDictionaryIfNull();

            if (modsAdded) {
                GameValue.RemoveModifiers(gameValuesDict, modifiers, 1, "TestingMods");
            }
            else {
                GameValue.AddModifiers(gameValuesDict, modifiers, 1, "TestingMods", false, gameObject, null);
            }
            modsAdded = !modsAdded;

        }

        void MakeValuesDictionaryIfNull () {
            if (gameValuesDict == null) {
                gameValuesDict = new Dictionary<string, GameValue>();

                for (int i = 0; i< gameValues.Length; i++) {
                    gameValuesDict[gameValues[i].name] = gameValues[i];
                }
            }
        }

        bool HasGameValue (string name) {
            MakeValuesDictionaryIfNull();
            return gameValuesDict.ContainsKey(name);
        }


        float GetGameValueComponent (string name, GameValue.GameValueComponent component) {
            MakeValuesDictionaryIfNull();
            if (!HasGameValue(name)) {
                Debug.Log(this.name + " does not have a game value named "+ name);
                return 0;
            }
            return gameValuesDict[name].GetValueComponent(component);
        }
        float GetGameValue (string name) {
            return GetGameValueComponent(name, GameValue.GameValueComponent.Value);
        }

        void Start () {
            for (int i = 0; i < gameValues.Length; i++) {
                gameValues[i].ReInitialize();
            }
        }



        // [NeatArray] 
        public Conditions conditions;

        public bool checkConditions, addRemoveMods;


        void Update () {
            if (checkConditions) {

                bool conditionsTrue = Conditions.ConditionsMet(conditions, gameObject, null);

                Debug.Log("Conditions Return Value: " + conditionsTrue);

                checkConditions = false;
            }

            if (addRemoveMods) {
                AddRemoveModifiersModifiers();

                addRemoveMods = false;
                
            }
        }

        float GetTrueValue () {

            return 1.0f;
        }

        float GetLargeValue () {

            return 99999;
        }

        float GetParamsValue (float param1) {

            return param1;
        }
        

    }
}