// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityTools.GameSettingsSystem;
using UnityTools.Spawning;
using UnityTools.EditorTools;
using System.Linq;
namespace UnityTools.FastTravelling {
    // [CreateAssetMenu()]
    public class FastTravelSettings : GameSettingsObjectSingleton<FastTravelSettings>
    {
        [Header("Spawn Options")] 
        public SpawnOptions fastTravelSpawnOptions;


        [NeatArray] public FastTravelAliasArray spawnAliases;
        
        // use a dictionary to lookups at run time for performance
        static Dictionary<string, FastTravelAlias> aliasLookup;

        void InitializeLookups () {
            if (aliasLookup == null) {
                aliasLookup = new Dictionary<string, FastTravelAlias>();
                for (int i = 0; i < spawnAliases.Length; i++) {
                    aliasLookup.Add(spawnAliases[i].key, spawnAliases[i]);
                }
            }
        }

        SpawnLocation _GetFastTravelAlias (string name) {
            // during editor, just search by for loop
            if (!Application.isPlaying) {
                for (int i = 0; i < spawnAliases.Length; i++) {
                    if (spawnAliases[i].key == name) {
                        return spawnAliases[i].location;
                    }   
                }   
            }
            else {
                InitializeLookups ();
                FastTravelAlias alias;
                if (aliasLookup.TryGetValue(name, out alias)) {
                    return alias.location;
                }
            }
            Debug.LogError("Couldnt find Fast Travel Alias: " + name);
            return null;
        }

        List<string> _GetKeys () {
            // during editor, just search by for loop
            if (!Application.isPlaying) {
                List<string> r = new List<string>();
                for (int i = 0; i < spawnAliases.Length; i++) {
                    r.Add(spawnAliases[i].key);   
                }   
                return r;
            }
            else {
                InitializeLookups ();
                return aliasLookup.Keys.ToList();
            }
        }

        public static SpawnLocation GetFastTravelAlias(string name) {
            if (instance == null)
                return null;
            return instance._GetFastTravelAlias(name);
        }
        public static List<string> GetKeys() {
            if (instance == null)
                return null;
            return instance._GetKeys();
        }

    }
}
