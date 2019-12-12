using UnityEngine;
using UnityTools;
using UnityTools.EditorTools;

namespace UnityToolsDemo {
    public class TestConditions : MonoBehaviour {
        [NeatArray] public GameValueModifierArray modifiers;

        bool modsAdded;
        void AddRemoveModifiersModifiers () {    
            if (modsAdded)
                GetComponent<GameValuesContainer>().RemoveModifiers(modifiers, 1, "Testing Mods");
            else
                GetComponent<GameValuesContainer>().AddModifiers(modifiers, 1, "Testing Mods", false, gameObject, null);
            modsAdded = !modsAdded;
        }

        public Conditions conditions;
        public bool checkConditions, addRemoveMods;


        void Update () {
            if (checkConditions) {
                Debug.Log("Conditions Return Value: " + Conditions.ConditionsMet(conditions, gameObject, null));
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