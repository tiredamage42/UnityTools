
using System.Collections.Generic;
using UnityEngine;
using UnityTools.DevConsole;
namespace UnityTools {
    /*
        allow systems to modify values as predifined points in the code....
    */
    public class ModifierEntryPoints : MonoBehaviour {
        public delegate float EntryPointModifier (float originalValue, Dictionary<string, object> runtimeSubjects);
        Dictionary<string, Dictionary<int, EntryPointModifier>> modifiers = new Dictionary<string, Dictionary<int, EntryPointModifier>>();
        
        List<string> checkedEntryPoints = new List<string>();

        [Command("entrypointschecked", "Get a list of the checked entry points on an object", "Entry Points", true)]
        public string GetCheckedEntryPoints () {
            return string.Join(",\n", checkedEntryPoints);
        }

        [Command("entrypoints", "Get a list of the added entry points on an object", "Entry Points", true)]
        public string GetEntryPointsWithModifiers () {
            return string.Join(",\n", modifiers.Keys);
        }

        public float ModifyValue (string entryPoint, float originalValue, Dictionary<string, object> runtimeSubjects) {
            if (!checkedEntryPoints.Contains(entryPoint))
                checkedEntryPoints.Add(entryPoint);
            
            // if modifiers found for entry point
            if (!modifiers.TryGetValue(entryPoint, out Dictionary<int, EntryPointModifier> entryPointModifiers)) 
                return originalValue;
            
            foreach (var modifier in entryPointModifiers.Values) 
                originalValue = modifier(originalValue, runtimeSubjects);
            
            return originalValue;
        }

        public void RegisterModifier  (string entryPoint, int id, EntryPointModifier modifier) {
            if (!modifiers.TryGetValue(entryPoint, out Dictionary<int, EntryPointModifier> entryPointModifiers)) {
                entryPointModifiers = new Dictionary<int, EntryPointModifier>();
                modifiers[entryPoint] = entryPointModifiers;
            }
            else {
                if (entryPointModifiers.ContainsKey(id)) {
                    Debug.LogWarning("Entry Point: " + entryPoint + ", Already has modifier for id: " + id);
                    return;
                }
            }
            entryPointModifiers[id] = modifier;
        }

        public void UnregisterModifier (string entryPoint, int id) {
            if (modifiers.TryGetValue(entryPoint, out Dictionary<int, EntryPointModifier> entryPointModifiers)) 
                if (entryPointModifiers.ContainsKey(id)) 
                    entryPointModifiers.Remove(id);
        }
    }
}