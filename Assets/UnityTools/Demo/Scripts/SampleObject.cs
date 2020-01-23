
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

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
        [NeatArray] public PreviewedAudioAssetArray audioClipsAssets;
        // [PreviewedAudio(false)] public AudioClip previewedAudio;
        [PreviewedAudio(true)] public AudioClip previewedAudioSelector;

        // [CustomAssetSelection(typeof(SampleObject))] public SampleObject sampleObjectCustomSelector;

        [NeatArray] public ExtendedAudioClipArray extendedClips;
        public ExtendedAudioClip extendedClip0, extendedClip1;

        public ParticlesFX particlesFX, particlesFX2;

        [NeatArray] public GameValueArray gameValues;
        // [NeatArray] public GameValueModifierArray modifiers;
        // [NeatArray] public GameValueModifierArray2D modifiers2d;

        [NeatArray("Subjects: 'Owner', 'Target'")] public Conditions conditions;

        [NeatArray] public Messages messages;

        // public ONamepsac.BaseClassTest baseClassTest;

        

    }


    // [CustomEditor(typeof(SampleObject))]
    // public class SampleObjectEditor : Editor {

    //     CurveDrawer curveDrawer = new CurveDrawer();

    //     AnimationCurve curve;// = AnimationCurve.Linear(0,0,1,1);


    //     AnimationCurve BuildCurve () {
    //         AnimationCurve c = new AnimationCurve();
            
    //         for (int i = 0; i < 50; i++) {
    //             c.AddKey(i, Mathf.Pow(i, 2) - Mathf.Pow(Mathf.Max(0,i-1), 2));
    //         }

    //         return c;
    //     }

    //     void OnEnable () {
    //         curve = BuildCurve();
    //         curveDrawer.OnEnable();
    //     }
    //     void OnDisable () {
    //         curveDrawer.OnDisable();
    //     }
    //     public override void OnInspectorGUI () {
    //         base.OnInspectorGUI ();
    //         curveDrawer.OnGUI (curve, 250, Color.green);
    //     }
    // }
}


#endif