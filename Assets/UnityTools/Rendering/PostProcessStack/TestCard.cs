using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace UnityTools.Rendering
{
    [System.Serializable]
    [PostProcess(typeof(TestCardRenderer), PostProcessEvent.AfterStack, "Custom/TestCard")]
    public sealed class TestCard : PostProcessEffectSettings
    {
        [Range(0,1)] public FloatParameter opacity = new FloatParameter();
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && opacity.value != 0;
        }
    }

    public sealed class TestCardRenderer : PostProcessEffectRenderer<TestCard>
    {
        static Shader _shader; 

        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/TestCard", ref _shader); } }

        static readonly int _Parameters = Shader.PropertyToID("_Parameters");        
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            // Size of inner area
            Vector2 area = new Vector2(Mathf.Floor(6.5f * ((float)context.screenWidth / context.screenHeight)) * 2 + 1, 13);
            float scale = 27f / context.screenHeight; // Grid scale
            sheet.properties.SetVector(_Parameters, new Vector4(settings.opacity, scale, area.x, area.y));
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}