using System.Collections.Generic;

using UnityTools.EditorTools;
using UnityEngine;
using UnityEditor;

using System;

namespace UnityTools {
    [CreateAssetMenu(menuName="Unity Tools/Prefabs/Random Prefab Choice", fileName="RandomPrefabChoice")]
    public class RandomPrefabChoice : ScriptableObject
    {
        public RandomPrefabChoices choices;
        public PrefabReference GetRandomPrefabChoice (Dictionary<string, object> runtimeSubjects) {
            return choices.GetRandomPrefabChoice(runtimeSubjects);
        }
    }

    [Serializable] public class RandomPrefabChoices {
        [NeatArray] public RandomPrefabArray choices;
        public PrefabReference GetRandomPrefabChoice (Dictionary<string, object> runtimeSubjects) {
        
            List<PrefabReference> refs = new List<PrefabReference>();
            for (int i = 0; i < choices.Length; i++) {
                if (Conditions.ConditionsMet(choices[i].conditions, runtimeSubjects)) {        
                    refs.Add(choices[i].prefab);
                }
            }
            return refs.GetRandom(new PrefabReference(null, null));
        }
    }


    [Serializable] public class RandomPrefabArray : NeatArrayWrapper<RandomPrefab> { }
    [Serializable] public class RandomPrefab {
        public PrefabReference prefab;
        [NeatArray] public Conditions conditions;
    }

    #if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(RandomPrefabChoices))] class RandomPrefabChoicesDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("choices"), label, true);
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("choices"), true);
        }
    }

    [CustomPropertyDrawer(typeof(RandomPrefab))]
    class RandomPrefabDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            SerializedProperty prefabProp = prop.FindPropertyRelative("prefab");
            EditorGUI.PropertyField(pos, prefabProp, GUIContent.none, true);
            pos.y += EditorGUI.GetPropertyHeight(prefabProp, GUIContent.none, true);
            
            SerializedProperty conditionsProp = prop.FindPropertyRelative("conditions");
            EditorGUI.PropertyField(pos, conditionsProp, new GUIContent("Conditions"), true);
        }

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("conditions"), true) + EditorGUI.GetPropertyHeight(prop.FindPropertyRelative("prefab"), GUIContent.none, true);
        }
    }

    #endif

}
