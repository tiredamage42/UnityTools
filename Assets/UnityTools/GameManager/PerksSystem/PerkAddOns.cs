using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityTools.EditorTools;
namespace UnityTools {

    /*
        perk behaviors that dont need to be instantiated into the scene to keep track of values during runtime...

        such as buffs that are only given or taken away during perk level changes
    */
    [System.Serializable] public class PerkBehaviorsArray : NeatArrayWrapper<PerkBehavior> { }
    public abstract class PerkBehavior : ScriptableObject {
        public abstract void OnPerkLevelChange (PerksContainer perksContainer, Perk perk, int oldLevel, int newLevel);
        public abstract void OnPerkGiven(PerksContainer perksContainer, Perk perk);
        public abstract void OnPerkRemoved(PerksContainer perksContainer, Perk perk, int level);
        public abstract void OnPerkUpdate (PerksContainer perksContainer, Perk perk, int level, float deltaTime);
    }


    /*
        copies of each perk script object are instantiated and attached to the object they're given to
        on perk update is only called if the perk level > 0

        use as a skill tree system
        or mod script....
    */

    [System.Serializable] public class PerkScriptsArray : NeatArrayWrapper<PerkScript> { }

    public abstract class PerkScript : MonoBehaviour {
        PerksContainer _perksContainer;
        protected PerksContainer perksContainer { get { return this.GetComponentIfNull<PerksContainer>(ref _perksContainer, false); } }
        
        public PerkScript MakeCopyOnGameObject (GameObject target) {
            PerkScript script = target.AddComponent(GetType()) as PerkScript;
            CopyFieldsTo(script);
            return script;
        }
        protected abstract void CopyFieldsTo (PerkScript target);
        public abstract void OnPerkLevelChange (Perk perk, int oldLevel, int newLevel);
        public abstract void OnPerkGiven(Perk perk);
        public abstract void OnPerkRemoved(Perk perk, int level);
        public abstract void OnPerkUpdate (Perk perk, int level, float deltaTime);
    }

}
