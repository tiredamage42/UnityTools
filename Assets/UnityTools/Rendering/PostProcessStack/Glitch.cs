using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(GlitchRenderer), PostProcessEvent.AfterStack, "Custom/Glitch")]
    public sealed class Glitch : PostProcessEffectSettings
    {

        // vector that keeps 4 values. 
        // the x and y as the displacement amount for the stripes (in each axis), 
        // the z and w as the displacement amount for the wavy displacement effect (again in each axis)
        public Vector4Parameter displacementAmount = new Vector4Parameter() { value = new Vector4(0,0,0,0) };
        
        public Vector2Parameter chromaticAberration = new Vector2Parameter();
        
        //“Amount” is basically the frequency, while the “Fill” is how many stripes will be visible
        public FloatParameter rightStripesAmount = new FloatParameter() { value = 1 };
        [Range(0,1)] public FloatParameter rightStripesFill = new FloatParameter() { value = .7f };
        public FloatParameter leftStripesAmount = new FloatParameter() { value = 1 };
        [Range(0,1)] public FloatParameter leftStripesFill = new FloatParameter() { value = .7f };

        // the frequency of our wavy mask
        public FloatParameter wavesFrequency = new FloatParameter() { value = 10 };        
        public FloatParameter glitchSpeed = new FloatParameter() { value = 0 };
        
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class GlitchRenderer : PostProcessEffectRenderer<Glitch>
    {

        static Shader _shader; 
        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/Glitch", ref _shader); } }
        
        static int _DisplacementAmount = Shader.PropertyToID("_DisplacementAmount");
        static int _Params0 = Shader.PropertyToID("_Params0");
        static int _Params1 = Shader.PropertyToID("_Params1");
        
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            
            sheet.properties.SetVector(_DisplacementAmount, settings.displacementAmount);
            
            sheet.properties.SetVector(_Params0, new Vector4(
                settings.chromaticAberration.value.x,
                settings.chromaticAberration.value.y,
                settings.rightStripesAmount,
                settings.leftStripesAmount
            ));
            sheet.properties.SetVector(_Params1, new Vector4(
                settings.rightStripesFill,
                settings.leftStripesFill,
                settings.wavesFrequency,
                settings.glitchSpeed * Time.realtimeSinceStartup
            ));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}