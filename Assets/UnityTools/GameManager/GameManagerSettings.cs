using UnityEngine;
using UnityTools.GameSettingsSystem;
namespace UnityTools.Internal {
    
    [CreateAssetMenu(menuName="Unity Tools/Internal/Game Manager Settings", fileName="GameManagerSettings")]
    public class GameManagerSettings : GameSettingsObject {
        public float pauseRoutineDelay = .1f;
    }
}