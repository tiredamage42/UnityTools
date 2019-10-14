
#if UNITY_EDITOR
using UnityEngine;

using UnityTools.EditorTools;
using UnityTools.Audio;
using UnityTools.Particles;
namespace UnityToolsDemo {
    /*
        used for testing
    */
    [CreateAssetMenu(menuName="Unity Tools/Demo Sample Object")]
    public class SampleObject : ScriptableObject
    {
        [NeatArray] public PreviewedAudioAssetArray audioClipsAssets;
        [PreviewedAudio(false)] public AudioClip previewedAudio;
        [PreviewedAudio(true)] public AudioClip previewedAudioSelector;

        [CustomAssetSelection(typeof(SampleObject))] public SampleObject sampleObjectCustomSelector;

        [NeatArray] public ExtendedAudioClipArray extendedClips;
        public ExtendedAudioClip extendedClip0, extendedClip1;

        public ParticlesFX particlesFX, particlesFX2;

    }
}
#endif