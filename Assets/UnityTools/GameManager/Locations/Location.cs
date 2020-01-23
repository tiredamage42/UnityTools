
/*


entry points {
    Dictionary<string, Func<float, object, object, oject, float>> entryPoints;
    { entry point name, (value, owner, target, objCheck) => return moddedvalue }
}


perk PerkWEntryPoints : perkAddon {
    class entryPoint {

        entry point name

        modify value type

        value conditions (make simple conditions)
        owner conditions
        target conditions
        object conditions

        // todo: be able to multyply / add based on game values on either owner or value case


        float ModifyValue (value, object perkOwner, object ownerTarget, object objectCheck) {

            if (ConditionsMet()) {

                return modified value;
            }
            return value;
        }

        lvels to use

        entryActive
        checkLevel  (perkowner, perklevel) {

            if (perkLevel in levels to use) {

                if (!entryActive) {
                    perkowner.entrypoints.ModifyEntryPoint (entry point name, modifyValue);
                    entryActive = true;
                }
            }
            ese {
                if (entry acive) {
                    perkowner.entrypoints.UnModifyEntryPoint (entry point name, modifyValue);
                    entry active = fas;
                }
            }
        }


        onPerkGiven () {

        }
    }




    OnPerkGiven () {

    }






}




gv {

    addtovalue (float mod, object cause) {

        //register entry point on game value create...

        mod = actorOwner.ModifyEntryPoint (name + "incomingMultiplier", mod, cause, this);
        
        value += mod;
    }
}

    D&D ============================================
    cap:
    y = b((x+1)^d)-(b(x+1))





*/



using System;
using UnityEditor;
using UnityEngine;

using UnityTools.EditorTools;
using System.Collections.Generic;

using UnityTools.Internal;
namespace UnityTools {

    [Serializable] public class Location {
        public LocationKey key;
        public bool useAlias;
        public string alias;

        public string GetSceneName () {
            LocationKey key = GetKey();
            if (key == null)
                return null;
            return key.scene;
        }

        public LocationKey GetKey() {
            return useAlias ? LocationAliases.GetLocationKey(alias) : key;
        }
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Location))] class LocationDrawer : PropertyDrawer
    {

        float CalcHeight (SerializedProperty useAlias) {
            return GUITools.singleLineHeight * (useAlias.boolValue ? 2 : 3);
        }
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {


            SerializedProperty useAlias = prop.FindPropertyRelative("useAlias");
            GUITools.Box(new Rect(pos.x, pos.y, pos.width, CalcHeight(useAlias)), new Color32(0, 0, 0, 32));

            pos.height = GUITools.singleLineHeight;
            GUITools.Label(pos, label, GUITools.black, GUITools.boldLabel);

            GUITools.DrawIconToggle(useAlias, new GUIContent("A", "Use Alias"), pos.x + (pos.width - GUITools.iconButtonWidth), pos.y );


            pos.y += GUITools.singleLineHeight;
                        
            if (useAlias.boolValue) {
                SerializedProperty aliasProp = prop.FindPropertyRelative("alias");
                DrawKeySelect(pos, aliasProp);

                GUI.enabled = !string.IsNullOrEmpty(aliasProp.stringValue);

                if (GUITools.IconButton(pos.x + (pos.width - GUITools.iconButtonWidth), pos.y, new GUIContent("G", "Go To Scene / Location"), GUITools.white)) {
                    LocationKey key = LocationAliases.GetLocationKey(aliasProp.stringValue);
                    if (key != null) {
                        LocationKeyDrawer.FastTravelToLocation (key.scene, key.name);
                    }
                }
                GUI.enabled = true;
            }
            else {
                EditorGUI.PropertyField(pos, prop.FindPropertyRelative("key"), true);
            }
            
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return CalcHeight(prop.FindPropertyRelative("useAlias"));
        }
    
    

        static void DrawKeySelect (Rect pos, SerializedProperty prop) {
            if (prop.stringValue.EndsWith("@"))
                return;


            
            EditorGUI.LabelField(new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, pos.height), new GUIContent(prop.displayName));
            if (GUITools.Button(pos.x + EditorGUIUtility.labelWidth, pos.y, pos.width - (EditorGUIUtility.labelWidth + GUITools.iconButtonWidth), pos.height, new GUIContent(!string.IsNullOrEmpty(prop.stringValue) ? prop.stringValue : "[ Null ]"), GUITools.popup)) {
                
                List<string> allKeys = LocationAliases.GetAliasList();
                if (!allKeys.Contains(prop.stringValue)) {
                    prop.stringValue = string.Empty;
                }
                
                GenericMenu menu = new GenericMenu();
                
                for (int i = 0; i < allKeys.Count; i++) {
                    string e = allKeys[i];
                    menu.AddItem (new GUIContent(e), e == prop.stringValue, 
                        () => {
                            prop.stringValue = e;
                            prop.serializedObject.ApplyModifiedProperties();
                        }
                    );
                }
                menu.ShowAsContext();  
            }
        }
    }

    /*
        prevent prefab applly from saving and changing location parameters...
        scene portals use the same prefab, but not the same target location...
    */
    public class LocationPrefabApplyPrevention {

        public static void PreventPrefabApply (SerializedProperty location, ref bool tick) {
            if (tick) {
                tick = false;
                AdjustValues(location, true);
            }
            if(!Application.isPlaying) {
                if (!LocationIsPrefabOverride(location)) {
                    tick = true;
                    AdjustValues(location, false);
                }
            }
        }
        
        static readonly string[] overrideProps = new string[] {
            "key.scene",
            "key.name",
            "alias",
            "useAlias"
        };

        static void AdjustValues (SerializedProperty prop, bool restore) {
            SerializedProperty k = prop.FindPropertyRelative("useAlias");
            k.boolValue = !k.boolValue;
            for (int i = 0; i < 3; i++) {
                SerializedProperty p = prop.FindPropertyRelative(overrideProps[i]);
                if (restore) {
                    if (p.stringValue.EndsWith("@"))
                        p.stringValue = p.stringValue.Remove(p.stringValue.Length - 1);
                }
                else {
                    p.stringValue = p.stringValue + "@";
                }
            }
            prop.serializedObject.ApplyModifiedPropertiesWithoutUndo();     
        }
        public static bool LocationIsPrefabOverride (SerializedProperty prop) {
            for (int i = 0; i < overrideProps.Length; i++){
                if (!prop.FindPropertyRelative(overrideProps[i]).prefabOverride) {
                    return false; 
                }
            }
            return true;
        }
    }        
    #endif


}
