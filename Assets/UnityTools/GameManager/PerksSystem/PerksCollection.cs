using System.Collections.Generic;
using UnityEngine;

using UnityTools.GameSettingsSystem;
namespace UnityTools {
    // [CreateAssetMenu()]
    public class PerksCollection : GameSettingsObjectSingleton<PerksCollection>
    {
        public override bool ShowInGameSettingsWindow() {
            return false;
        }
        public Perk[] allPerks;
        // use a dictionary to lookups at run time for performance
        static Dictionary<string, Perk> perkLookups;

        static void InitializeRuntimePerksLookup () {
            perkLookups = new Dictionary<string, Perk>();
            for (int i = 0; i < instance.allPerks.Length; i++) {
                Perk perk = instance.allPerks[i];
                perkLookups[perk.name] = perk;
            }
        }

        public static Perk GetPerk (string name) {
            if (instance == null)
                return null;

            // during editor, just search by for loop
            if (!Application.isPlaying) {
                for (int i = 0; i < instance.allPerks.Length; i++) {
                    Perk perk = instance.allPerks[i];
                    if (perk.name == name)
                        return perk;
                }   
            }
            else {
                if (perkLookups == null) {
                    InitializeRuntimePerksLookup ();
                }
                Perk perk;
                if (perkLookups.TryGetValue(name, out perk)) {
                    return perk;
                }
            }
            Debug.LogError("Couldnt Perk Named: " + name);
            return null;
        }
    }
}
