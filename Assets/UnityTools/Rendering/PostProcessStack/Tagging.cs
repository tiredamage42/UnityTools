using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using System.Collections.Generic;


using UnityTools.DevConsole;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [PostProcess(typeof(TaggingRenderer), PostProcessEvent.BeforeStack, "Custom/Tagging")]
    public sealed class Tagging : PostProcessEffectSettings
    {
        
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && TagsActive();
        }

        public static List<Renderer> renderers = new List<Renderer>();
        public static List<BasicColor> colors = new List<BasicColor>();
        public static List<bool> overlays = new List<bool>(), flashes = new List<bool>();

        static void Remove (int atIndex) {
            renderers.RemoveAt(atIndex);
            colors.RemoveAt(atIndex);
            overlays.RemoveAt(atIndex);
            flashes.RemoveAt(atIndex);
        }
        
        [Command("tagobject", "visually tag object (by dynamic object key [#/@])", "Rendering", true)]
        public static void TagRenderers (string doKey, BasicColor color, bool overlay, bool flash) {
            TagRenderers(DynamicObjectManager.GetDynamicObjectFromKey(doKey), color, overlay, flash);
        }
        public static void TagRenderers (DynamicObject obj, BasicColor color, bool overlay, bool flash) {
            if (obj == null)
                return;
            TagRenderers(obj.renderers, color, overlay, flash);
        }
        public static void TagRenderers(Renderer[] renderers, BasicColor color, bool overlay, bool flash) {
            for (int i = 0; i < renderers.Length; i++) 
                TagRenderer(renderers[i], color, overlay, flash);
        }
        public static void TagRenderer(Renderer renderer, BasicColor color, bool overlay, bool flash) {
            if (!renderers.Contains(renderer)) {
                renderers.Add(renderer);
                colors.Add(color);
                overlays.Add(overlay);
                flashes.Add(flash);
            }
            else {
                int i = renderers.IndexOf(renderer);
                colors[i] = color;
                overlays[i] = overlay;
                flashes[i] = flash;
            }
        }
        public static void UntagRenderers(Renderer[] renderers) {
            for (int i = 0; i < renderers.Length; i++) 
                UntagRenderer(renderers[i]);
        }
        public static void UntagRenderer(Renderer renderer) {
            if (renderers.Contains(renderer))
                Remove(renderers.IndexOf(renderer));
        }
        static void Clean () {
            for (int i = renderers.Count -1; i >= 0; i--)
                if (renderers[i] == null || !renderers[i].gameObject.activeInHierarchy) 
                    Remove(i);
        }        
        static bool TagsActive () {
            Clean();
            return renderers.Count > 0;
        }
    }

    public sealed class TaggingRenderer : PostProcessEffectRenderer<Tagging>
    {
        static readonly int _MaxDistance = Shader.PropertyToID("_MaxDistance");
        static readonly int _OverlayMask = Shader.PropertyToID("_OverlayMask");
        static readonly int _Color = Shader.PropertyToID("_Color");
        static readonly int _FlashParams = Shader.PropertyToID("_FlashParams");
        static readonly int _RimPower = Shader.PropertyToID("_RimPower");
        
        static int drawTarget = Shader.PropertyToID("_TagsRT");
        
        static RenderingSettings renderSettings { get { return RenderingSettings.instance; } }
        
        static Shader _fxShader;
        static Shader fxShader { get { return RenderUtils.GetShaderIfNull("Hidden/Tagging", ref _fxShader); } }

        static Material _drawTaggedMaterial;
        static Material drawTaggedMaterial { get { return RenderUtils.CreateMaterialIfNull("Hidden/Tagged", ref _drawTaggedMaterial); } }


        public override void Render(PostProcessRenderContext context)
        {
            context.command.SetGlobalFloat(_RimPower, renderSettings.tagDefaultRimPower);
            context.command.SetGlobalVector(_FlashParams, new Vector4(renderSettings.tagFlashSpeed, renderSettings.tagFlashSteepness, renderSettings.tagFlashRimPowerRange.x, renderSettings.tagFlashRimPowerRange.y));
            context.command.SetGlobalFloat(_MaxDistance, renderSettings.tagsMaxRenderDistance);

            context.command.GetTemporaryRT(drawTarget, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.Default);
            context.command.SetRenderTarget(drawTarget, (RenderTargetIdentifier)BuiltinRenderTextureType.Depth);
            context.command.ClearRenderTarget(false, true, Color.clear, 1.0f);
            DrawRenderGroup(context.command, false);
            context.command.SetRenderTarget(drawTarget, drawTarget);
            DrawRenderGroup(context.command, true);
            context.command.SetGlobalTexture(_OverlayMask, drawTarget);            
            context.command.BlitFullscreenTriangle(context.source, context.destination, context.propertySheets.Get(fxShader), 0);
            context.command.ReleaseTemporaryRT(drawTarget);
        }
        static void DrawRenderGroup (CommandBuffer command, bool overlay) {
            for (int r = 0; r < Tagging.renderers.Count; r++) {
                if (Tagging.overlays[r] != overlay)
                    continue;
                // maybe account for submeshes here....
                command.SetGlobalColor(_Color, renderSettings.tagColors.GetColor(Tagging.colors[r])); 
                command.DrawRenderer(Tagging.renderers[r], drawTaggedMaterial, 0, Tagging.flashes[r] ? 1 : 0);
            }
        }  
    }
}