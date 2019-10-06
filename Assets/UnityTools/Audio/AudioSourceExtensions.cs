using UnityEngine;
using UnityTools.Audio.Internal;

namespace UnityTools.Audio {

    static class AudioSourceExtensions 
    {

        public static void Stop (this AudioSource source, float fadeOut) {
            // just stops if fade out is <= 0
            FadingAudioSource.FadeOutSource (source, fadeOut);
        }

        public static bool Play (this AudioSource source, ExtendedAudioClip clip, float fadeIn) {
            AudioClip audioClip = clip.clips.GetRandom(null);
            if (audioClip == null) {
                Debug.LogWarning("No AudioClip Found");
                return false;
            }

            source.pitch = (clip.pitchRange.x == clip.pitchRange.y) ? clip.pitchRange.x : UnityEngine.Random.Range(clip.pitchRange.x, clip.pitchRange.y);
            
            if (clip.is2D) {
                source.spatialBlend = 0;
            }
            else {
                source.spatialBlend = 1;
                source.rolloffMode = clip.useLinearRolloff ? AudioRolloffMode.Linear : AudioRolloffMode.Logarithmic;
                source.minDistance = clip.minMaxDistance.x;
                source.maxDistance = clip.minMaxDistance.y;

                source.spread = clip.spread;
                source.dopplerLevel = clip.doppler;
            }

            source.priority = clip.priority;
            source.panStereo = clip.stereoPan;

            if (clip.oneShot) {
                source.loop = false;
                source.PlayOneShot(audioClip, clip.volume);
            }
            else {
                source.Stop(0);
                source.volume = clip.volume;
                source.loop = clip.loop;
                source.clip = audioClip;
                source.Play();
            }   

            if (fadeIn > 0) {
                FadingAudioSource.FadeInSource(source, fadeIn);
            }

            return true;
        }
    }
}
