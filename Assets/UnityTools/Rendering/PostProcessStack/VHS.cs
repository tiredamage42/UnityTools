using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace UnityTools.Rendering
{

    [System.Serializable]
    [PostProcess(typeof(VHSRenderer), PostProcessEvent.AfterStack, "Custom/VHS")]
    public sealed class VHS : PostProcessEffectSettings
    {
        [Range(0,1)] public FloatParameter drift = new FloatParameter();
        [Range(0,1)] public FloatParameter jitter = new FloatParameter();
        [Range(0,1)] public FloatParameter jump = new FloatParameter();
        [Range(0,1)] public FloatParameter shake = new FloatParameter();

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && (drift.value > 0 || jitter.value > 0 || jump.value > 0 || shake.value > 0);
        }
    }

    public sealed class VHSRenderer : PostProcessEffectRenderer<VHS>
    {
        static Shader _shader; 

        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/VHS", ref _shader); } }
        
        class PerCamVariables {
            public float _prevTime, _jumpTime;
        }

        static Dictionary<Camera, PerCamVariables> variables = new Dictionary<Camera, PerCamVariables>();

        static PerCamVariables GetVariables (Camera cam) {
            if (variables.TryGetValue(cam, out PerCamVariables vars)) 
                return vars;
            variables[cam] = new PerCamVariables();
            return variables[cam];
        }

        static readonly int _Parameters0 = Shader.PropertyToID("_Parameters0");
        static readonly int _Parameters1 = Shader.PropertyToID("_Parameters1");
        
        public override void Render(PostProcessRenderContext context)
        {
            PerCamVariables vars = GetVariables(context.camera);

            // Update the time parameters.
            var time = Time.realtimeSinceStartup;

            var delta = time - vars._prevTime;
            vars._jumpTime += delta * settings.jump.value * 11.3f;
            vars._prevTime = time;

            // Drift parameters (time, displacement)
            var vdrift = new Vector2(
                time * 606.11f % (Mathf.PI * 2),
                settings.drift.value * 0.04f
            );

            // Jitter parameters (threshold, displacement)
            var jv = settings.jitter.value;
            var vjitter = new Vector3(
                Mathf.Max(0, 1.001f - jv * 1.2f),
                0.002f + jv * jv * jv * 0.05f
            );

            // Jump parameters (scroll, displacement)
            var vjump = new Vector2(vars._jumpTime, settings.jump.value);

            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetVector(_Parameters0, new Vector4((int)(time * 10000), settings.shake.value * 0.2f, vdrift.x, vdrift.y));
            sheet.properties.SetVector(_Parameters1, new Vector4(vjitter.x, vjitter.y, vjump.x, vjump.y));
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
