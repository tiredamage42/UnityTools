
#if UNITY_EDITOR
using UnityEngine;

using UnityTools.EditorTools;
using UnityTools.Audio;

namespace UnityToolsDemo {
    /*
        used for testing
    */
    [CreateAssetMenu(menuName="Custom Editor Tools/Sample Object")]
    public class SampleObject : ScriptableObject
    {
        [NeatArray] public PreviewedAudioAssetArray audioClipsAssets;
        [NeatArray] public PreviewedAudioClipArray audioClipsClips;
        
        [PreviewedAudio(false)] public AudioClip previewedAudio;
        [PreviewedAudio(true)] public AudioClip previewedAudioSelector;

        [CustomAssetSelection(typeof(SampleObject))] public SampleObject sampleObjectCustomSelector;

        [NeatArray] public ExtendedAudioClipArray extendedClips;
        public ExtendedAudioClip extendedClip0, extendedClip1;

    }
}
#endif