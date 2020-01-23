using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityTools.EditorTools;
using UnityEngine.Rendering.PostProcessing;
namespace UnityTools.Rendering {
    [System.Serializable] public class ImageSpaceMod {
        public PostProcessProfile profile;
        public float duration = 2f;
        public float fadeIn = .1f;
        public float fadeOut = .1f;
        public AnimationCurve anim = AnimationCurve.Constant(0, 1, 1);
        public float animCycle;
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ImageSpaceMod))]
    class ImageSpaceModDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            position.y = GUITools.PropertyFieldAndHeightChange (position, property, "profile");
            position.y = GUITools.PropertyFieldAndHeightChange (position, property, "duration");
            position.y = GUITools.PropertyFieldAndHeightChange (position, property, "fadeIn");
            position.y = GUITools.PropertyFieldAndHeightChange (position, property, "fadeOut");
            position.y = GUITools.PropertyFieldAndHeightChange (position, property, "anim");
            position.y = GUITools.PropertyFieldAndHeightChange (position, property, "animCycle");
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight * 6;
        }
    }

    #endif
        


    public class ImageSpaceModifier : MonoBehaviour
    {
        static ComponentPool<ImageSpaceModifier> pool = new ComponentPool<ImageSpaceModifier>();
        static List<ImageSpaceModifier> activeModifiers = new List<ImageSpaceModifier>();
        
        static bool ProfileIsActive (PostProcessProfile profile, out ImageSpaceModifier mod) {
            for (int i = 0; i < activeModifiers.Count; i++) {
                if (activeModifiers[i].profile == profile) {
                    mod = activeModifiers[i];
                    return true;
                }
            }
            mod = null;
            return false;
        }

        public static bool AddImageSpaceModifier (PostProcessProfile profile, float fadeIn, float duration, float fadeOut, AnimationCurve anim, float animCycle) {
            if (profile == null) {
                Debug.LogWarning("Add: Post Prcess profile is null");
                return false;
            }

            if (ProfileIsActive(profile, out _)) {
                Debug.LogWarning("Add: Post Prcess profile: " + profile.name + " is already active in an image space modifier");
                return false;
            }


            ImageSpaceModifier mod = pool.GetAvailable(null, true);
            activeModifiers.Add(mod);
            mod.InitializeModifier(profile, fadeIn, duration, fadeOut, anim, animCycle, activeModifiers.Count);
            // Debug.Log("Adding profile: " + profile.name);
            return true;
        }

        public static void RemoveImageSpaceModifier (PostProcessProfile profile, float fadeOut) {
            if (profile == null) {
                Debug.LogWarning("Remove: Post Prcess profile is null");
                return;
            }

            if (!ProfileIsActive(profile, out ImageSpaceModifier mod)) {
                Debug.LogWarning("Remove: Post Prcess profile: " + profile.name + " is not active in an image space modifier");
                return;
            }

            // Debug.Log("Removing profile: " + profile.name);

            mod.StartRemoveModifier(fadeOut);
        }

        void InitializeModifier (PostProcessProfile profile, float fadeIn, float duration, float fadeOut, AnimationCurve anim, float animCycle, float priority) {
            this.fadeIn = fadeIn;
            this.duration = duration;
            this.fadeOut = fadeOut;
            this.anim = anim;
            this.animCycle = animCycle;
            this.profile = profile;

            m_Volume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("PostProcessing"), priority, profile.settings.ToArray());
            
            durationTimer = 0;
            weight = 0;
            isRemoving = false;
            m_Volume.weight = 0;
        }
            
        bool isRemoving;
        // float weight, t, animCycle, durationTimer;
        float weight, animCycle, durationTimer;
        
        PostProcessProfile profile;
        float fadeIn = .1f;
        float duration = 2f;
        float fadeOut = .1f;
        AnimationCurve anim;// = AnimationCurve.Constant(0, 1, 1);
        PostProcessVolume m_Volume;


        void StartRemoveModifier(float fadeOut) {
            if (isRemoving) 
                return;
            
            this.fadeOut = fadeOut;
            isRemoving = true;
            weight = 1;
        }
        
        void Update () {
            if (isRemoving) {
                UpdateFade(0, fadeOut, Time.deltaTime);
                if (weight == 0) {
                    // Debug.Log("Removing modifier " + profile.name);
                    gameObject.SetActive(false);
                    return;
                }
            }
            else {
                if (weight != 1.0f) 
                    UpdateFade(1, fadeIn, Time.deltaTime);
                
            }

            durationTimer += Time.deltaTime;
            
            m_Volume.weight = weight * CalculateAnimMultiplier();

            if (duration > 0) {
                
                if (durationTimer >= duration) {
                    StartRemoveModifier(fadeOut);
                }
            }
        }

        float CalculateAnimMultiplier () {
            return Mathf.Clamp01(anim.Evaluate((durationTimer % animCycle) / animCycle));
        }

        void UpdateFade (float target, float time, float deltaTime) {
            if (time > 0) {
                weight += deltaTime * (1f/time) * (target == 1 ? 1 : -1);
                weight = Mathf.Clamp01(weight);
            }
            else  
                weight = target;
        }
        
        void OnDisable () {
            profile = null;
            RuntimeUtilities.DestroyVolume(m_Volume, false, true);
            activeModifiers.Remove(this);
        }        
    }
}
