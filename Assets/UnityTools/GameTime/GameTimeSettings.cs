using UnityEngine;
using UnityTools.GameSettingsSystem;
namespace UnityTools.Internal {
    
    // [CreateAssetMenu()]
    public class GameTimeSettings : GameSettingsObjectSingleton<GameTimeSettings> {
        [Header("Engine")]
        public float fixedTimeStepFrequencyMultiplier = 1.0f;
        [Tooltip("Unity Default: .02")] public float fixedTimeStep = .02f;
        [Tooltip("Unit Default: .1")] public float maxTimeStep = .1f;
        
        [Header("Dilation")]
        [Range(0.1f, 2)] public float baseTimeDilation = 1.0f;

        [Header("Time Of Day")]
        [Tooltip("2 = twice as fast as real life")]
        [Range(1, 50)] public float defaultTimeScale = 10;
        public GameDate defaultGameDate;
    }
}
