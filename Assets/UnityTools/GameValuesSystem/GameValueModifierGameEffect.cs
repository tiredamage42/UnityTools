
using UnityEngine;
using UnityEditor;
using UnityTools.EditorTools;

namespace UnityTools {
    [CreateAssetMenu(menuName="Unity Tools/Game Effects/GameValue Modifier Effect", fileName="GameValueModifierEffect")]
    public class GameValueModifierGameEffect : GameEffect {

        [Tooltip("Magnitude is multiplied by delta time when updated")]
        public bool byTime;
        public string componentDescription = "";

        [HideInInspector] public string valueName;
        [HideInInspector] public GameValueModifierComponent componentToModify;
        [HideInInspector] public GameValueModifierBehavior behavior;

        protected override bool PlayerOnly() { return false; }
        protected override bool CreateCopy() { return false; }
        protected override bool AddToEffectsList() {
            switch (componentToModify) {
                case GameValueModifierComponent.BaseMod:   return behavior != GameValueModifierBehavior.Set;
                case GameValueModifierComponent.Base:      return usesDeltaTime;
                case GameValueModifierComponent.Ranged:    return usesDeltaTime;
            }
            return false;
        }
        string ModifyBehaviorString (float magnitude) {
            if (behavior == GameValueModifierBehavior.Add)
                return magnitude > 0 ? "+" : "-";
            else if (behavior == GameValueModifierBehavior.Multiply) 
                return "x";
            return "";
        }
        
        public override string GetDescription(float magnitude, float duration) { 
            // max HP +10;

            string d = componentDescription;
            if (!string.IsNullOrEmpty(componentDescription))
                d += " ";
            
            d += valueName + " " + ModifyBehaviorString(magnitude) + Mathf.Abs(magnitude);

            if (duration > 0)
                d += "(" + duration + " sec)";

            return null; 
        }

        // cant modify by delta time if modifying base modifier or setting values
        bool usesDeltaTime { get { return byTime && componentToModify != GameValueModifierComponent.BaseMod && behavior != GameValueModifierBehavior.Set; } }
            
        public override void OnEffectRemove (DynamicObject caster, DynamicObject target, float magnitude, float duration) {
            if (byTime)
                return;
            
            GameValuesContainer container = target.GetObjectScript<GameValuesContainer>();
            if (container != null) {
                if (behavior == GameValueModifierBehavior.Multiply) 
                    container.ModifyValue (valueName, componentToModify, behavior, 1f/magnitude );
                else if (behavior == GameValueModifierBehavior.Add) 
                    container.ModifyValue (valueName, componentToModify, behavior, -magnitude );
            }
        }

        protected override void OnEffectUpdate(DynamicObject caster, DynamicObject target, float deltaTime, float timeAdded, float currentTime, float magnitude, float duration, out bool removeEffect) {
            removeEffect = false;
            if (byTime) 
                target.GetObjectScript<GameValuesContainer>().ModifyValue (valueName, componentToModify, behavior, magnitude * deltaTime );
        }
            
        protected override bool EffectValid(DynamicObject caster, DynamicObject affectedObject) {
            GameValuesContainer container = affectedObject.GetObjectScript<GameValuesContainer>();

            if (container == null)
                return false;
            
            // if the user wants to be affected by delta time
            if (byTime) 
                // return true if we can actually modify based on delta time, if we not, we dont do anything
                return usesDeltaTime;
            
            return true;
        }
            

        protected override bool OnEffectStart(DynamicObject caster, DynamicObject affectedObject, float magnitude, float duration) {
            GameValuesContainer container = affectedObject.GetObjectScript<GameValuesContainer>();
            if (byTime) 
                return true;
            return container.ModifyValue (valueName, componentToModify, behavior, magnitude );
        }
    }


    #if UNITY_EDITOR
    [CustomEditor(typeof(GameValueModifierGameEffect))]
    public class GameValueModifierGameEffectEditor : Editor {
        
        public override void OnInspectorGUI() {

            EditorGUILayout.BeginHorizontal();

            GUITools.StringFieldWithDefault(serializedObject.FindProperty("valueName"), "Value Name");

            SerializedProperty behaviorProp = serializedObject.FindProperty("behavior");
            EditorGUILayout.PropertyField(behaviorProp, GUITools.noContent, true);
            GameValueModifierBehavior behavior = (GameValueModifierBehavior)behaviorProp.enumValueIndex;

            SerializedProperty componentProp = serializedObject.FindProperty("componentToModify");
            EditorGUILayout.PropertyField(componentProp, GUITools.noContent, true);
            GameValueModifierComponent component = (GameValueModifierComponent)componentProp.enumValueIndex;

            SerializedProperty byTimeProp = serializedObject.FindProperty("byTime");
            
            // cant modify by delta time if modifying base modifier or setting values
            if (component == GameValueModifierComponent.BaseMod || behavior == GameValueModifierBehavior.Set) {
                byTimeProp.boolValue = false;
            }
            else {
                GUITools.DrawIconToggle(byTimeProp, BuiltInIcons.GetIcon("UnityEditor.AnimationWindow", "Magnitude is multiplied by delta time when updated"));
            }
            
            EditorGUILayout.EndHorizontal();

            if (component == GameValueModifierComponent.BaseMod && behavior == GameValueModifierBehavior.Set) 
                EditorGUILayout.HelpBox("Cant set base value modifier.  Changes need to be able to be unmodified...", MessageType.Error);
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("conditions"), new GUIContent("Conditions"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keywords"), new GUIContent("Keywords"), true);

            serializedObject.ApplyModifiedProperties();
        }

    }
    #endif
}
