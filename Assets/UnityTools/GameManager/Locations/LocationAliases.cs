using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using UnityTools.GameSettingsSystem;
using UnityTools.EditorTools;
using UnityTools.DevConsole;

namespace UnityTools.Internal {

    [System.Serializable] public class LocationAliasArray : NeatArrayWrapper<LocationAlias> { }
    [System.Serializable] public class LocationAlias {
        public string alias;
        public LocationKey key;
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LocationAlias))] class LocationAliasDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("alias"), true);
            pos.y += GUITools.singleLineHeight;
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("key"), true);
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight * 3;
        }   
    }
    #endif


    [CreateAssetMenu(menuName="Unity Tools/Locations/Location Aliases", fileName="LocationAliases")]
    public class LocationAliases : GameSettingsObject
    {
        [NeatArray] public LocationAliasArray aliases;

        // use a dictionary to lookups at run time for performance
        static Dictionary<string, LocationKey> aliasLookup;

        static void InitializeLookups () {
            if (aliasLookup == null) {
                aliasLookup = new Dictionary<string, LocationKey>();
                List<LocationAliases> allAliases = GameSettings.GetSettingsOfType<LocationAliases>();
                for (int i = 0; i < allAliases.Count; i++) {
                    LocationAliases collection = allAliases[i];
                    for (int j = 0; j < collection.aliases.Length; j++) {
                        string alias = collection.aliases[j].alias;
                        if (!string.IsNullOrEmpty(alias)) {
                            if (aliasLookup.ContainsKey(alias)) {
                                Debug.LogError("Multiple Location Aliases: '" + alias + "', using first defined [Scene/Location]: " + aliasLookup[alias].scene + "/" + aliasLookup[alias].name);
                            }
                            else {
                                aliasLookup.Add(alias, collection.aliases[j].key);
                            }
                        }
                    }
                }
            }
        }
        public static LocationKey GetLocationKey (string name) {
            if (string.IsNullOrEmpty(name))
                return null;

            // during editor, just search by for loop
            if (!Application.isPlaying) {
                List<LocationAliases> allAliases = GameSettings.GetSettingsOfType<LocationAliases>();

                for (int i = 0; i < allAliases.Count; i++) {
                    LocationAliases collection = allAliases[i];
                    for (int j = 0; j < collection.aliases.Length; j++) {
                        string cAlias = collection.aliases[j].alias;
                        if (!string.IsNullOrEmpty(cAlias)) {
                            if (cAlias == name) {
                                return collection.aliases[j].key;
                            }
                        }
                    }
                } 
            }
            else {
                InitializeLookups ();
                if (aliasLookup.TryGetValue(name, out LocationKey key)) 
                    return key;
            }

            Debug.LogError("Couldnt find Location Alias: " + name);
            return null;
        }

        [Command("travelaliases", "List all the location aliases available", "Game", false)]
        static string ListAliases () {
            return string.Join("\n", GetAliasList());
        }

        public static List<string> GetAliasList () {
            // during editor, just search by for loop
            if (!Application.isPlaying) {
                List<string> r = new List<string>();

                List<LocationAliases> allAliases = GameSettings.GetSettingsOfType<LocationAliases>();
                for (int i = 0; i < allAliases.Count; i++) {
                    LocationAliases collection = allAliases[i];
                    for (int j = 0; j < collection.aliases.Length; j++) {
                        string alias = collection.aliases[j].alias;
                        if (!string.IsNullOrEmpty(alias)) {
                            if (!r.Contains(alias)) {
                                r.Add(alias);
                            }
                        }
                    }
                }

                return r;
            }
            else {
                InitializeLookups ();
                return aliasLookup.Keys.ToList();
            }
        }
    }
}