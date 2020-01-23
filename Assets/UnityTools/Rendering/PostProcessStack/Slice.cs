using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace UnityTools.Rendering
{
    [System.Serializable]
    [PostProcess(typeof(SliceRenderer), PostProcessEvent.AfterStack, "Custom/Slice")]
    public sealed class Slice : PostProcessEffectSettings
    {
        public FloatParameter rowCount = new FloatParameter() { value = 30 };
        [Range(-90,90)] public FloatParameter angle = new FloatParameter();
        [Range(-1,1)] public FloatParameter displacement = new FloatParameter();
        public IntParameter randomSeed = new IntParameter();

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && displacement.value != 0;
        }
    }

    public sealed class SliceRenderer : PostProcessEffectRenderer<Slice>
    {
        static Shader _shader;
        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Slice", ref _shader); } }
         
		static readonly int _Parameters = Shader.PropertyToID("_Parameters");
        static readonly int _Seed = Shader.PropertyToID("_Seed");

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            
            var rad = settings.angle.value * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            var seed = (uint)settings.randomSeed.value;
            seed = (seed << 16) | (seed >> 16);

            sheet.properties.SetVector(_Parameters, new Vector4(dir.x, dir.y, settings.displacement, settings.rowCount));
            sheet.properties.SetInt(_Seed, (int)seed);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }

    }
}
