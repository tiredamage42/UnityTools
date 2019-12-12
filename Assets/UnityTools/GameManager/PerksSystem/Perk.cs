using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using UnityTools.EditorTools;
namespace UnityTools {
    [CreateAssetMenu(menuName="Unity Tools/Perk", fileName="New Perk")] 
    public class Perk : ScriptableObject
    {
        public bool isPublic = true;
        public bool playerEdit = true;
        public string displayName;
        [TextArea] public string description;
        [Header("Per Level")] [NeatArray] public NeatStringArray descriptions; 
        public int maxLevels = 1;
        // public int levels { get { return descriptions.list.Length; } }

        [NeatArray] public PerkBehaviorsArray perkBehaviors;
        [NeatArray] public PerkScriptsArray perkScripts;   
    }
}
