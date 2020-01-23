using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(LightWaveRenderer), PostProcessEvent.AfterStack, "Custom/LightWave")]
    public sealed class LightWave : PostProcessEffectSettings
    {

        [Range(0,1)] public FloatParameter intensity = new FloatParameter() { value = 1 };
        public Vector2Parameter red = new Vector2Parameter () { value = new Vector2(4f, 4f) };
		public Vector2Parameter green = new Vector2Parameter () { value = new Vector2(0, 4f) };
		public Vector2Parameter blue = new Vector2Parameter () { value = new Vector2(4f, 0) };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && intensity.value > 0;
        }
    }

    public sealed class LightWaveRenderer : PostProcessEffectRenderer<LightWave>
    {
        static Shader _shader; 
		static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/LightWave", ref _shader); } }
        
        static readonly int _RG = Shader.PropertyToID("_RG");
        static readonly int _BI = Shader.PropertyToID("_BI");
            
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetVector(_RG, new Vector4(settings.red.value.x, settings.red.value.y, settings.green.value.x, settings.green.value.y));
			sheet.properties.SetVector(_BI, new Vector4(settings.blue.value.x, settings.blue.value.y, settings.intensity, 0));
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}