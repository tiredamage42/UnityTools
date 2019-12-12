
#if UNITY_EDITOR
using UnityEngine;


using UnityTools;
using UnityTools.EditorTools;
using UnityTools.Audio;
using UnityTools.Particles;
namespace UnityToolsDemo {
    /*
        used for testing
    */
    // [CreateAssetMenu(menuName="Unity Tools/Demo Sample Object")]
    public class SampleObject : ScriptableObject
    {
        // [NeatArray] public PreviewedAudioAssetArray audioClipsAssets;
        // [PreviewedAudio(false)] public AudioClip previewedAudio;
        // [PreviewedAudio(true)] public AudioClip previewedAudioSelector;

        // [CustomAssetSelection(typeof(SampleObject))] public SampleObject sampleObjectCustomSelector;

        // [NeatArray] public ExtendedAudioClipArray extendedClips;
        // public ExtendedAudioClip extendedClip0, extendedClip1;

        // public ParticlesFX particlesFX, particlesFX2;

        [NeatArray] public GameValueArray gameValues;
        [NeatArray] public GameValueModifierArray modifiers;
        // [NeatArray] public GameValueModifierArray2D modifiers2d;

        public Conditions conditions;

        public Messages messages;

        // public ONamepsac.BaseClassTest baseClassTest;

        

    }
}
#endif