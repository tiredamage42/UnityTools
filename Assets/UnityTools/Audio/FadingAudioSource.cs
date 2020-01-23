using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Audio.Internal {
    /*
        used to fade audiosources in / out
    */
    class FadingAudioSource {
        static List<FadingAudioSource> fadingAudioSources = new List<FadingAudioSource>();
        static Dictionary<int, FadingAudioSource> sourceToFadingAudioSource = new Dictionary<int, FadingAudioSource>();
        static bool isUpdating;

        static bool SourceIsFading (AudioSource source, out FadingAudioSource fas) {
            return sourceToFadingAudioSource.TryGetValue(source.GetInstanceID(), out fas);
        }

        static void InitializeAudioSourceExtensions () {
            if (!isUpdating) {
                UpdateManager.update += UpdateFadingSources;
                isUpdating = true;
            }
        }

        static void UpdateFadingSources (float deltaTime) {
            for (int i = fadingAudioSources.Count - 1; i >= 0; i--) {
                FadingAudioSource fas = fadingAudioSources[i];
                if (fas.UpdateSource(deltaTime)) {
                    fadingAudioSources.Remove(fas);
                    sourceToFadingAudioSource.Remove(fas.source.GetInstanceID());
                }
            }
        }

        static void FadeSource (AudioSource source, float fadeDuration, bool fadeIn) {
            InitializeAudioSourceExtensions();

            FadingAudioSource fas;
            if (SourceIsFading ( source, out fas )) {
                // check if in opposite fade state
                if (fas.fadeIn != fadeIn) {
                    fas.Reset(fadeDuration, fadeIn);
                }
                return;
            }
            
            new FadingAudioSource(source, fadeDuration, fadeIn);
        }
            
        public static void FadeInSource (AudioSource source, float fadeDuration) {
            FadeSource ( source, fadeDuration, true );
        }
            
        public static void FadeOutSource (AudioSource source, float fadeDuration) {
            if (!source.isPlaying) 
                return;
        
            if (fadeDuration <= 0) {
                source.Stop();
                return;
            }

            FadeSource ( source, fadeDuration, false );
        }
            

        AudioSource source;
        bool fadeIn;
        float targetVolume, fadeDuration, volumeT;

        public void Reset (float fadeDuration, bool fadeIn) {
            this.fadeIn = fadeIn;
            this.fadeDuration = 1f/fadeDuration;
            this.targetVolume = source.volume;
            volumeT = 0;
        }
        
        FadingAudioSource (AudioSource source, float fadeDuration, bool fadeIn) {
            fadingAudioSources.Add(this);
            sourceToFadingAudioSource.Add(source.GetInstanceID(), this);
            this.source = source;    
            Reset(fadeDuration, fadeIn);
        }
        
        bool UpdateSource (float deltaTime) {
            if (source == null || !source.isPlaying) 
                return true;

            volumeT = Mathf.Clamp01(volumeT + deltaTime * fadeDuration);
            source.volume = (fadeIn ? volumeT : 1 - volumeT) * targetVolume;

            bool done = volumeT == 1.0f;
            if (!fadeIn && done) 
                source.Stop();
            return done;   
        }
    }
}