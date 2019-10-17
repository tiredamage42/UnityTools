using UnityEngine;
using UnityTools.GameSettingsSystem;
namespace UnityTools.Internal {
    
    [CreateAssetMenu(menuName="Unity Tools/Internal/Game Time Settings", fileName ="GameTimeSettings")]
    public class GameTimeSettings : GameSettingsObject {
        [Range(0,2)] public float baseTimeDilation = 1.0f;
        public float fixedTimeStepFrequencyMultiplier = 1.0f;
        [Header("Unity Default: .02")] public float fixedTimeStep = .02f;
        [Header("Unit Default: .1")] public float maxTimeStep = .1f;
        public float actualFixedTimeStep { get { return fixedTimeStep / fixedTimeStepFrequencyMultiplier; } }
    }
}
