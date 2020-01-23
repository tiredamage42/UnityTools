using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(SphericalRenderer), PostProcessEvent.AfterStack, "Custom/Spherical")]
    public sealed class Spherical : PostProcessEffectSettings
    {
        public FloatParameter radius = new FloatParameter () { value = 1f};

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class SphericalRenderer : PostProcessEffectRenderer<Spherical>
    {
        static Shader _shader; 
        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Spherical", ref _shader); } }
        
        static readonly int _Radius = Shader.PropertyToID("_Radius");
        
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetFloat(_Radius, settings.radius);            
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}