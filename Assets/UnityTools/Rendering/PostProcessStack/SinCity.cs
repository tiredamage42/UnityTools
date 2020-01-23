using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(SinCityRenderer), PostProcessEvent.AfterStack, "Custom/SinCity")]
    public sealed class SinCity : PostProcessEffectSettings
    {
        public ColorParameter selectedColor = new ColorParameter () { value = Color.red };
		public ColorParameter replacementColor = new ColorParameter () { value = Color.white };
		public FloatParameter desaturation = new FloatParameter () { value = 0.5f};
		public FloatParameter tolerance = new FloatParameter () { value = 0.5f};

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class SinCityRenderer : PostProcessEffectRenderer<SinCity>
    {
        static Shader _shader; 
		static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/SinCity", ref _shader); } }

        static readonly int _Params = Shader.PropertyToID("_Params");
        static readonly int _SelectedColor = Shader.PropertyToID("_SelectedColor");
        static readonly int _ReplacedColor = Shader.PropertyToID("_ReplacedColor");
        
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetColor(_SelectedColor, settings.selectedColor);
			sheet.properties.SetColor(_ReplacedColor, settings.replacementColor);
            sheet.properties.SetVector(_Params, new Vector2(settings.desaturation, settings.tolerance));
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}