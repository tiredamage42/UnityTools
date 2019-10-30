using UnityEngine;
using UnityTools.GameSettingsSystem;

using UnityTools.EditorTools;
namespace UnityTools.Internal {
    
    [CreateAssetMenu(menuName="Unity Tools/Internal/Game Manager Settings", fileName="GameManagerSettings")]
    public class GameManagerSettings : GameSettingsObject {
        public float pauseRoutineDelay = .1f;
        public string newGameScene;
        public GameObject playerPrefab;
        public int maxSaveSlots = 6;
        public ActionsInterfaceController actionsController;

    }
}