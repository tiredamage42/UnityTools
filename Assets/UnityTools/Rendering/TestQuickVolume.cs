using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering.PostProcessing;
public class TestQuickVolume : MonoBehaviour
{

    public PostProcessProfile overrideProfile;

    PostProcessVolume m_Volume;
    // Vignette m_Vignette;

    void Start()
    {
        // m_Vignette = ScriptableObject.CreateInstance<Vignette>();
        // m_Vignette.enabled.Override(true);
        // m_Vignette.intensity.Override(1f);
        // m_Vignette.color.Override(Color.black);
        // m_Vignette.roundness.Override(1f);

        m_Volume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("PostProcessing"), 100f, 
        overrideProfile.settings.ToArray()
        // m_Vignette
        );
    }

    void Update()
    {

        m_Volume.weight = Mathf.Sin(Time.realtimeSinceStartup);
        // m_Vignette.intensity.value = Mathf.Sin(Time.realtimeSinceStartup);
    }

    void OnDestroy()
    {
        RuntimeUtilities.DestroyVolume(m_Volume, false, true);
    }
}
