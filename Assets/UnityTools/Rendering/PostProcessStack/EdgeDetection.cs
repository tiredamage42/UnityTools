using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(EdgeDetectionRenderer), PostProcessEvent.AfterStack, "Custom/EdgeDetection")]
    public sealed class EdgeDetection : PostProcessEffectSettings
    {
        [Tooltip("Number of pixels between samples that are tested for an edge. When this value is 1, tested samples are adjacent.")]
        public IntParameter scale = new IntParameter { value = 1 };
        [Tooltip("Difference between depth values, scaled by the current depth, required to draw an edge.")]
        public FloatParameter depthThreshold = new FloatParameter { value = 1.5f };
        [Range(0, 1), Tooltip("The value at which the dot product between the surface normal and the view direction will affect " +
            "the depth threshold. This ensures that surfaces at right angles to the camera require a larger depth threshold to draw " +
            "an edge, avoiding edges being drawn along slopes.")]
        public FloatParameter depthNormalThreshold = new FloatParameter { value = 0.5f };
        [Tooltip("Scale the strength of how much the depthNormalThreshold affects the depth threshold.")]
        public FloatParameter depthNormalThresholdScale = new FloatParameter { value = 7 };
        [Range(0, 1), Tooltip("Larger values will require the difference between normals to be greater to draw an edge.")]
        public FloatParameter normalThreshold = new FloatParameter { value = 0.4f };   


        [Space]

        public ColorParameter edgeColor = new ColorParameter() { value = Color.black };
        public ColorParameter backGroundColor = new ColorParameter() { value = Color.white };

        [Range(.1f, 1f)] public FloatParameter threshold = new FloatParameter() { value = .1f };
        
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value;
        }
    }

    public sealed class EdgeDetectionRenderer : PostProcessEffectRenderer<EdgeDetection>
    {

        public override DepthTextureMode GetCameraFlags() {
            return DepthTextureMode.DepthNormals;    
        }

        static Shader _shader; 
        static Shader shader { get { return RenderUtils.GetShaderIfNull("Hidden/EdgeDetection", ref _shader); } }
        
        static int _Parameters = Shader.PropertyToID("_Parameters");
        static int _BackgroundColor = Shader.PropertyToID("_BackgroundColor");
        static int _EdgeColor = Shader.PropertyToID("_EdgeColor");
        
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetFloat("_Scale", settings.scale);
            sheet.properties.SetFloat("_DepthThreshold", settings.depthThreshold);
            sheet.properties.SetFloat("_DepthNormalThreshold", settings.depthNormalThreshold);
            sheet.properties.SetFloat("_DepthNormalThresholdScale", settings.depthNormalThresholdScale);
            sheet.properties.SetFloat("_NormalThreshold", settings.normalThreshold);
            
            sheet.properties.SetMatrix("_ClipToView", GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, true).inverse);

            sheet.properties.SetVector(_Parameters, new Vector4(settings.threshold, 0, 0, 0));            
            sheet.properties.SetColor(_EdgeColor, settings.edgeColor);
            sheet.properties.SetColor(_BackgroundColor, settings.backGroundColor);
            
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}