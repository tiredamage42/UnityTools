using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityTools {
    public class PerkHolder {
        public Perk perk;
        public int level;
        public List<PerkScript> perkScripts = new List<PerkScript>();
        public PerkHolder (PerksContainer container, Perk perk) {
            this.perk = perk;
            level = 1;
            OnPerkGiven(container);
        }

        public bool IsAtMaxLevel () {
            return level >= perk.maxLevels;
        }

        void OnPerkGiven (PerksContainer container) {
            for (int i =0; i < perk.perkBehaviors.Length; i++) 
                perk.perkBehaviors[i].OnPerkGiven(container, perk);
            
            for (int i = 0; i < perk.perkScripts.Length; i++) {
                PerkScript scriptInstance = perk.perkScripts[i].MakeCopyOnGameObject(container.gameObject);
                scriptInstance.OnPerkGiven(perk);
                perkScripts.Add(scriptInstance);
            }
        }

        public void OnPerkRemoved (PerksContainer container) {
            for (int i =0; i < perk.perkBehaviors.Length; i++) {
                perk.perkBehaviors[i].OnPerkRemoved(container, perk, level);
            }
            for (int i =0; i < perkScripts.Count; i++) {
                perkScripts[i].OnPerkRemoved(perk, level);
                MonoBehaviour.Destroy(perkScripts[i]);
            }
            perkScripts.Clear();
        }        
        public void SetLevel(PerksContainer container, int newLevel) {
            bool clamped = false;
            if (newLevel > perk.maxLevels) {
                clamped = true;
                newLevel = perk.maxLevels;
            }
            if (level != newLevel) {
                if (clamped) {
                    Debug.LogWarning(perk.name + ", capping level to: " + perk.maxLevels);
                }
                int oldLevel = level;
                level = newLevel;
                OnPerkLevelChange(container, oldLevel);
            }
        }

        void OnPerkLevelChange (PerksContainer container, int oldLevel) {
            // tell the static behaviors
            for (int i =0; i < perk.perkBehaviors.Length; i++) 
                perk.perkBehaviors[i].OnPerkLevelChange(container, perk, oldLevel, level);
            // tell the runtime scripts
            for (int i =0; i < perkScripts.Count; i++) 
                perkScripts[i].OnPerkLevelChange(perk, oldLevel, level);
        }

        public void OnPerkUpdate (PerksContainer container, float deltaTime) {
            if (level > 0) {
                for (int i =0; i < perk.perkBehaviors.Length; i++) 
                    perk.perkBehaviors[i].OnPerkUpdate(container, perk, level, deltaTime);
            
                // update the runtime scripts attached to this perk
                for (int i =0; i < perkScripts.Count; i++) 
                    perkScripts[i].OnPerkUpdate(perk, level, deltaTime);
            }
        }
    }

    [Serializable] public class PerksContainerState : ObjectAttachmentState {
        public List<string> perksSaved;
            
        public PerksContainerState (PerksContainer instance) {
            perksSaved = new List<string>();
            for (int i = 0; i < instance.allPerks.Count; i++) {
                perksSaved.Add(instance.allPerks[i].perk.name + PerksContainer.paramsKey + instance.allPerks[i].level.ToString());
            }
        }
    }

    public class PerksContainer : MonoBehaviour, IObjectAttachment {

        public int InitializationPhase () {
            return -1;
        }

        public void InitializeDefault () {
            // build perks from template
           
        }

        public void Strip () {
            RemoveAllPerks();
        }

        public const string paramsKey = "@@";
        static readonly string[] splitKey = new string[] { paramsKey };

        public ObjectAttachmentState GetState () {
            return new PerksContainerState (this);
        }

        public void LoadState (ObjectAttachmentState state) {
            PerksContainerState perksState = state as PerksContainerState; 
            
            // RemoveAllPerks ();
            for (int i = 0; i < perksState.perksSaved.Count; i++) {
                string[] split = perksState.perksSaved[i].Split(splitKey, StringSplitOptions.RemoveEmptyEntries);
                AddPerk(PerksCollection.GetPerk(split[0]), int.Parse(split[1]));
            }
        }

        // void OnEnable () {
            // remove all perks then
            // build perks from template...
        // }

        public List<PerkHolder> allPerks = new List<PerkHolder>();

        void Update () {
            if (GameManager.isPaused)
                return;

            UpdatePerkScripts(Time.deltaTime);
        }

        void UpdatePerkScripts (float deltaTime) {
            for (int i = 0; i < allPerks.Count; i++)
                allPerks[i].OnPerkUpdate (this, deltaTime);
        }

        bool HasPerk (Perk perk, out int index) {
            for (int i = 0; i < allPerks.Count; i++) {
                if (allPerks[i].perk == perk) {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        public bool PerkMaxedOut (Perk perk) {
            int index;
            if (HasPerk(perk, out index))
                return allPerks[index].IsAtMaxLevel();
            return false;
        }
        public bool PerkMaxedOut (string perk) {
            return PerkMaxedOut(PerksCollection.GetPerk(perk));
        }

        public int GetPerkLevel (Perk perk) {
            int index;
            if (HasPerk(perk, out index))
                return allPerks[index].level;
            return 0;
        }
        public int GetPerkLevel (string perk) {
            return GetPerkLevel(PerksCollection.GetPerk(perk));
        }
        public bool HasPerk (Perk perk) {
            return GetPerkLevel(perk) > 0;
        }
        public bool HasPerk (string perk) {
            return GetPerkLevel(perk) > 0;
        }

        PerkHolder GetOrAddPerkHolder (Perk perk) {
            int index;
            if (HasPerk(perk, out index)) {
                return allPerks[index];
            } 
            allPerks.Add(new PerkHolder(this, perk));
            return allPerks[allPerks.Count - 1];
        }

        public PerkHolder AddToPerkLevel (Perk perk, int amount) {
            PerkHolder perkHolder = GetOrAddPerkHolder(perk);
            perkHolder.SetLevel(this, perkHolder.level + amount);
            return perkHolder;
        }

        public PerkHolder AddToPerkLevel (string perk, int amount) {
            return AddToPerkLevel(PerksCollection.GetPerk(perk), amount);
        }

        public PerkHolder SetPerkLevel (Perk perk, int level) {
            PerkHolder perkHolder = GetOrAddPerkHolder(perk);
            perkHolder.SetLevel(this, level);
            return perkHolder;
        }
        public PerkHolder SetPerkLevel (string perk, int level) {
            return SetPerkLevel(PerksCollection.GetPerk(perk), level);
        } 
        public PerkHolder AddPerk (Perk perk, int setLevel) {
            return SetPerkLevel(perk, setLevel);
        }
        public PerkHolder AddPerk (string perk, int setLevel) {
            return SetPerkLevel(perk, setLevel);
        }
        public PerkHolder AddPerk (Perk perk) {
            return GetOrAddPerkHolder(perk);
        }
        public PerkHolder AddPerk (string perk) {
            return AddPerk(PerksCollection.GetPerk(perk));
        }
        public void RemovePerk (Perk perk) {
            int index;
            if (HasPerk(perk, out index)) {
                allPerks[index].OnPerkRemoved(this);
                allPerks.Remove(allPerks[index]);
            } 
        }
        public void RemovePerk (string perk) {
            RemovePerk(PerksCollection.GetPerk(perk));
        }
        public void RemoveAllPerks () {
            for (int i = 0; i < allPerks.Count; i++) {
                allPerks[i].OnPerkRemoved(this);
            }
            allPerks.Clear();
        }
    }    
}