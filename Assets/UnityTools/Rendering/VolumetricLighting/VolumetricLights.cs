using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;

namespace UnityTools.Rendering.VolumetricLighting {
    
    [Serializable]
    [PostProcess(typeof(VolumetricLightsRenderer), PostProcessEvent.BeforeStack, "Custom/VolumetricLights")]
    public sealed class VolumetricLights : PostProcessEffectSettings
    {
        public FloatParameter maxDistance = new FloatParameter() { value = 100f };
        public FloatParameter fadeRange = new FloatParameter() { value = 10f };
        
        public FloatParameter noiseScale = new FloatParameter() { value = .5f };
        public FloatParameter dirNoiseScale = new FloatParameter() { value = .1f };
        [Range(-1f, 1f)] public FloatParameter noiseOffset = new FloatParameter() { value = 0 };    
        [Range(1f, 5f)] public FloatParameter noiseSteepness = new FloatParameter() { value = 2 };
        public Vector3Parameter noiseVelocity = new Vector3Parameter() { value = new Vector3(.025f, .025f, .025f) };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && VolumetricLightsManager.PrepareLightsForRender (context.camera, maxDistance, fadeRange);
        }
    }

    public sealed class VolumetricLightsRenderer : PostProcessEffectRenderer<VolumetricLights>
    {
        
        public override DepthTextureMode GetCameraFlags() {
            return DepthTextureMode.Depth;
        }

        static Shader _blitAdd;
        static Shader blitAdd { get { return RenderUtils.GetShaderIfNull("Hidden/BlitAdd", ref _blitAdd); } }
        static Shader _blur;
        static Shader blur { get { return RenderUtils.GetShaderIfNull("Hidden/BilateralBlur", ref _blur); } }
            
        
        static Vector4[] frustumCorners = new Vector4[4];

        static void CalcFrustumCorners (CommandBuffer cb, Camera cam) {
            float farClip = cam.farClipPlane;
            // setup frustum corners for world position reconstruction
            frustumCorners[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, farClip));
            frustumCorners[2] = cam.ViewportToWorldPoint(new Vector3(0, 1, farClip));
            frustumCorners[3] = cam.ViewportToWorldPoint(new Vector3(1, 1, farClip));
            frustumCorners[1] = cam.ViewportToWorldPoint(new Vector3(1, 0, farClip));
            cb.SetGlobalVectorArray(_FrustumCorners, frustumCorners);
        }

        public override void Render(PostProcessRenderContext context)
        {
            CommandBuffer cb = context.command;
            Camera cam = context.camera;

            cb.GetTemporaryRT(_RenderTarget, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
            cb.SetRenderTarget(_RenderTarget);
            cb.ClearRenderTarget(false, true, new Color(0, 0, 0, 1));

            cb.SetGlobalTexture(_DitherTexture, VolumetricLightUtils.ditherTexture);

            cb.SetGlobalTexture(_NoiseTexture, RenderUtils.noiseTexture3d);
            cb.SetGlobalVector(_NoiseData, new Vector4(settings.noiseScale, settings.dirNoiseScale, settings.noiseOffset, settings.noiseSteepness));
            cb.SetGlobalVector(_NoiseVelocity, settings.noiseVelocity.value * Time.realtimeSinceStartup);
            
            if (VolumetricLightsManager.IsDrawingDirectional())
                CalcFrustumCorners(cb, cam);
            
            cb.SetGlobalVector(_CameraForward, cam.transform.forward);

            for (int i = 0; i < VolumetricLightsManager.allLights.Count; i++) 
                if (VolumetricLightsManager.allLights[i].finalIntensity > 0)
                    DrawLight(VolumetricLightsManager.allLights[i], cb);
                         
            // BLUR
            var sheet = context.propertySheets.Get(blur);
            cb.GetTemporaryRT(_BlurTemp, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
            cb.BlitFullscreenTriangle(_RenderTarget, _BlurTemp, sheet, 0);
            cb.BlitFullscreenTriangle(_BlurTemp, _RenderTarget, sheet, 1);
            cb.ReleaseTemporaryRT(_BlurTemp);
            
            // COMPOSITE
            cb.SetGlobalTexture(_Source, _RenderTarget);
            sheet = context.propertySheets.Get(blitAdd);
            cb.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
            
            cb.ReleaseTemporaryRT(_RenderTarget);
        }

        static void DrawLight( VolumetricLight light, CommandBuffer cb)
        {
            bool forceShadowsOff = light.type != LightType.Directional && light.cam2LightDist >= QualitySettings.shadowDistance;
            bool drawShadows = light.lightC.shadows != LightShadows.None && light.lightC.shadowStrength > 0 && !forceShadowsOff;
            
            cb.EnableKeyword("USE_SHADOWS", drawShadows);
            light.UpdateShadowCommandBuffers(drawShadows);
            
            if (drawShadows)
                cb.SetGlobalTexture(_CachedShadowMap, light.shadowmapCache.shadowMap);
            
            cb.SetGlobalVector(_LightColor, new Vector4(light.lightC.color.r, light.lightC.color.g, light.lightC.color.b, light.finalIntensity));

            if (light.type == LightType.Point) 
                SetupPointLight(light, cb, light.transform.position, drawShadows);
            else if (light.type == LightType.Spot)
                SetupSpotLight(light, cb, light.transform.position, drawShadows);
            else if (light.type == LightType.Directional) 
                SetupDirectionalLight(light, cb, drawShadows);
        }

        static void SetNonDirectionalParams (VolumetricLight light, CommandBuffer cb, Vector3 lightPos, ref Matrix4x4 world, bool drawShadows, Vector2 extraParams) {
            float innerLimit = light.range * light.attenuationStart;
            cb.SetGlobalVector(_AttenuationParams, new Vector4(innerLimit, light.range - innerLimit, extraParams.x, extraParams.y));
            cb.SetGlobalVector(_VolumetricLight, new Vector4(1 - (light.mieG * light.mieG), 1 + (light.mieG * light.mieG), 2 * light.mieG, light.range));
            cb.SetGlobalVector(_LightPos, lightPos);
            cb.EnableKeyword("USE_COOKIE", light.cookie != null);
            if (light.cookie != null)
                cb.SetGlobalTexture(_LightTexture0, light.cookie);

            if (drawShadows) 
                light.shadowmapForcer.ForceShadows(ref world);
        }
        
        static void SetupPointLight(VolumetricLight light, CommandBuffer cb, Vector3 lightPos, bool drawShadows) {
            
            Matrix4x4 world = Matrix4x4.TRS(lightPos, Quaternion.identity, Vector3.one * light.range * 2.0f);
            SetNonDirectionalParams ( light, cb, lightPos, ref world, drawShadows, Vector2.zero);

            //view
            if (light.cookie != null)
                cb.SetGlobalMatrix(_MyLightMatrix0, Matrix4x4.TRS(lightPos, light.transform.rotation, Vector3.one).inverse);                
            
            cb.DrawMesh(RenderUtils.GetMesh(PrimitiveType.Sphere), world, VolumetricLightUtils.GetMaterial(LightType.Point), 0, IsCameraInPointLightBounds(light) ? 0 : 1);   
        }


        static readonly Matrix4x4 spotClip = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, Vector3.one * .5f);
        static readonly Matrix4x4 spotClipNeg = Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0.0f), Quaternion.identity, new Vector3(-0.5f, -0.5f, 1.0f));
        
        static void SetupSpotLight(VolumetricLight light, CommandBuffer cb, Vector3 lightPos, bool drawShadows)
        {

            float angleScale = Mathf.Tan((light.spotAngle + 1) * 0.5f * Mathf.Deg2Rad) * light.range;

            Quaternion rotation = light.transform.rotation;
            Matrix4x4 world = Matrix4x4.TRS(lightPos, rotation, new Vector3(angleScale, angleScale, light.range));            
            Matrix4x4 view = Matrix4x4.TRS(lightPos, rotation, Vector3.one).inverse;

            Vector3 fwd = light.transform.forward;
            // cosAngle, planeD, 
            Vector2 extraParams = new Vector2(Mathf.Cos((light.spotAngle + 1) * 0.5f * Mathf.Deg2Rad), -Vector3.Dot((lightPos + fwd * light.lightC.range), fwd));
            cb.SetGlobalVector(_ConeAxis, fwd);
            
            SetNonDirectionalParams ( light, cb, lightPos, ref world, drawShadows, extraParams);
            
            if (light.cookie != null) {
                Matrix4x4 proj = Matrix4x4.Perspective(light.spotAngle, 1, 0, 1);
                cb.SetGlobalMatrix(_MyLightMatrix0, spotClipNeg * proj * view);
            }

            if (drawShadows) {
                Matrix4x4 proj = Matrix4x4.Perspective(light.spotAngle, 1, SystemInfo.usesReversedZBuffer ? light.range : light.lightC.shadowNearPlane, SystemInfo.usesReversedZBuffer ? light.lightC.shadowNearPlane : light.range);
                Matrix4x4 m = spotClip * proj;
                m[0, 2] *= -1; m[1, 2] *= -1; m[2, 2] *= -1; m[3, 2] *= -1;
                cb.SetGlobalMatrix(_MyWorld2Shadow, m * view);    
            }
                
            cb.DrawMesh(VolumetricLightUtils.spotlightMesh, world, VolumetricLightUtils.GetMaterial(LightType.Spot), 0, IsCameraInSpotLightBounds( light, fwd) ? 0 : 1);            
        }
        static void SetupDirectionalLight(VolumetricLight light, CommandBuffer cb, bool drawShadows)
        {            
            if (drawShadows) 
                cb.SetGlobalTexture(_World2ShadowTex, light.world2ShadowCache.world2ShadowTex);
            
            cb.Blit(null as Texture, _RenderTarget, VolumetricLightUtils.GetMaterial(LightType.Directional), 0);    
        }

        static bool IsCameraInPointLightBounds(VolumetricLight light)
        {
            return light.cam2LightDist < light.range + 1;
        }

        static bool IsCameraInSpotLightBounds(VolumetricLight light, Vector3 fwd)
        {
            if (!IsCameraInPointLightBounds(light))
                return false;

            Vector3 cam2LightNorm = light.cam2Light / light.cam2LightDist;
            if((Mathf.Acos(Vector3.Dot(fwd, cam2LightNorm)) * Mathf.Rad2Deg) > (light.spotAngle + 3) * 0.5f)
                return false;

            return true;
        }


        static readonly int _RenderTarget = Shader.PropertyToID("_RenderTarget");
        static readonly int _BlurTemp = Shader.PropertyToID("_BlurTemp");

        static readonly int _DitherTexture = Shader.PropertyToID ("_DitherTexture");
        static readonly int _NoiseTexture = Shader.PropertyToID ("_NoiseTexture");
        static readonly int _NoiseData = Shader.PropertyToID ("_NoiseData");
        static readonly int _NoiseVelocity = Shader.PropertyToID ("_NoiseVelocity");
        static readonly int _CameraForward = Shader.PropertyToID ("_CameraForward");
        static readonly int _FrustumCorners = Shader.PropertyToID ("_FrustumCorners");
        static readonly int _Source = Shader.PropertyToID ("_Source");
        static readonly int _CachedShadowMap = Shader.PropertyToID ("_CachedShadowMap");
        static readonly int _VolumetricLight = Shader.PropertyToID ("_VolumetricLight");
        static readonly int _LightColor = Shader.PropertyToID ("_LightColor");
        static readonly int _MyLightMatrix0 = Shader.PropertyToID ("_MyLightMatrix0");
        static readonly int _LightPos = Shader.PropertyToID ("_LightPos");
        static readonly int _ConeAxis = Shader.PropertyToID ("_ConeAxis");
        static readonly int _LightTexture0 = Shader.PropertyToID ("_LightTexture0");
        static readonly int _AttenuationParams = Shader.PropertyToID ("_AttenuationParams");
        static readonly int _MyWorld2Shadow = Shader.PropertyToID ("_MyWorld2Shadow");
        static readonly int _World2ShadowTex = Shader.PropertyToID ("_World2ShadowTex");        
    }
}