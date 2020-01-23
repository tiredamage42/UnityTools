using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using System;

namespace UnityTools.Rendering {
    
    [Serializable]
    [PostProcess(typeof(EdgeDetectRenderer), PostProcessEvent.AfterStack, "Custom/EdgeDetect")]
    public sealed class EdgeDetect : PostProcessEffectSettings
    {
        public FloatParameter sensitivityDepth = new FloatParameter() { value = 1.0f };
        public FloatParameter sensitivityNormals = new FloatParameter() { value = 1.0f };
        public FloatParameter sampleDist = new FloatParameter() { value = 1.0f };
        [Range(0f,1f)]
        public FloatParameter edgesOnly = new FloatParameter() { value = 0.0f };
        public ColorParameter edgesOnlyBgColor = new ColorParameter() { value =  Color.white };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class EdgeDetectRenderer : PostProcessEffectRenderer<EdgeDetect>
    {
        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.DepthNormals;
        }
        static Shader _shader; 
		
        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/EdgeDetect", ref _shader); } }
        

        static readonly int _Params = Shader.PropertyToID("_Params");
        static readonly int _BgColor = Shader.PropertyToID("_BgColor");

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetVector(_Params, new Vector4(settings.sensitivityDepth, settings.sensitivityNormals, settings.sampleDist, settings.edgesOnly));
            sheet.properties.SetVector(_BgColor, settings.edgesOnlyBgColor.value);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}

