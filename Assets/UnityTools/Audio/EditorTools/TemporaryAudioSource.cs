using UnityEngine;
using UnityEditor;


namespace UnityTools.Audio.Internal {
    /*
        instantiate a temprorary game object with an audio source to play sounds in editor
    */
    #if UNITY_EDITOR
    [ExecuteInEditMode] class TemporaryAudioSource : MonoBehaviour
    {    
        AudioSource _source;
        public AudioSource source { get { return gameObject.GetOrAddComponent<AudioSource>(ref _source, true); } }

        Object targetObject;

        public void PlayClip (AudioClip clip, bool loop, Object targetObject) {
            source.clip = clip;
            source.volume = 1;
            source.spatialBlend = 0;
            source.loop = loop;
            source.Play();
            EditorApplication.update += UpdateEditor;
            this.targetObject = targetObject;
        }

        void UpdateEditor () {
            EditorApplication.QueuePlayerLoopUpdate();
        }
        void Update () {
            if (targetObject != null) EditorUtility.SetDirty( targetObject );
            if (!source.isPlaying) {
                EditorApplication.update -= UpdateEditor;
                DestroyImmediate(gameObject);
            }
        }
    }
    #endif
}
