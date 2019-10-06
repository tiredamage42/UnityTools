using UnityEngine;
using System.Collections.Generic;

using UnityTools.Audio.Internal;
namespace UnityTools.Audio.Editor {

    #if UNITY_EDITOR
    public class EditorAudioTools 
    {
        static List<TemporaryAudioSource> tempSources = new List<TemporaryAudioSource>();

        public static void PlayClip(AudioClip clip, bool loop, Object targetObject) {
             if (clip == null)
                return;
            
            GameObject g = new GameObject("__TemporaryAudioSource__");
            g.hideFlags = HideFlags.DontSave;
            tempSources.Add(g.AddComponent<TemporaryAudioSource>());
            tempSources[tempSources.Count - 1].PlayClip(clip, loop, targetObject);
        } 

        public static void StopAllClips() {
            for (int i = tempSources.Count - 1; i >= 0; i--) {
                if (tempSources[i] == null) {
                    tempSources.RemoveAt(i);
                }
                else {
                    tempSources[i].source.Stop();
                }
            }
        }
        public static bool IsClipPlaying(AudioClip clip) {
            return IsClipPlaying(clip, out _);
        }
        public static bool IsClipPlaying(AudioClip clip, out AudioSource source) {
            source = null;
            if (clip == null) {
                return false;
            }
            for (int i = tempSources.Count - 1; i >= 0; i--) {
                if (tempSources[i] == null) {
                    tempSources.RemoveAt(i);
                }
                else {
                    if (tempSources[i].source.clip == clip) {
                        source = tempSources[i].source;
                        return true;
                    }   
                }
            }
            return false;
        }

        public static void StopClip (AudioClip clip) {
            AudioSource source;
            if (IsClipPlaying (clip, out source)) {
                source.Stop();
            }
        }
    }
    #endif
}
