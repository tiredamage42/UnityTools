using UnityEngine;
using UnityTools.EditorTools;
using UnityTools.GameSettingsSystem;
namespace UnityTools {
    [CreateAssetMenu(menuName="Unity Tools/Initial Game Prefabs", fileName="InitialGamePrefabs")]
    public class InitialGamePrefabs : GameSettingsObject {
        [NeatArray] public NeatGameObjectArray prefabs;
        public override bool ShowInGameSettingsWindow() {
            return false;
        }
    }
}
