using UnityEngine;
using UnityTools.GameSettingsSystem;

using UnityTools.EditorTools;
namespace UnityTools.Internal {
    
    [CreateAssetMenu(menuName="Unity Tools/Internal/Game Manager Settings", fileName="GameManagerSettings")]
    public class GameManagerSettings : GameSettingsObject {
        public float pauseRoutineDelay = .1f;


        public string mainMenuScene;
        [NeatArray] public NeatStringArray nonSaveScenes;
    }
}