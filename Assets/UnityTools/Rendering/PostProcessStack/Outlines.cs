using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor;

using UnityTools.EditorTools;
using UnityTools.DevConsole;

namespace UnityTools.Rendering {
    
    [System.Serializable]
    [UnityEngine.Rendering.PostProcessing.PostProcess(typeof(OutlinesRenderer), PostProcessEvent.BeforeStack, "Custom/Outlines")]
    public sealed class Outlines : PostProcessEffectSettings
    {
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return enabled.value && Outlines.OutlinesActive();
        }


        static RenderingSettings settings { get { return RenderingSettings.instance; } }
        static Dictionary<string, OutlineCollection> collections = new Dictionary<string, OutlineCollection>();
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void _OnLoadGame() {
            InitializeGroups();
        }

        static void InitializeGroups () {
            collections.Clear();
            for (int i = 0; i < settings.outlineCollections.Length; i++) 
                collections.Add(settings.outlineCollections[i].name, new OutlineCollection(settings.outlineCollections[i]));
        }

        
        [Command("outlineobject", "visually outline object (by dynamic object key [#/@])", "Rendering", true)]
        public static void HighlightRenderers (string doKey, BasicColor color, bool overlay, string collection) {
            HighlightRenderers(DynamicObjectManager.GetDynamicObjectFromKey(doKey), color, overlay, collection);
        }
        public static void HighlightRenderers (DynamicObject obj, BasicColor color, bool overlay, string collection) {
            if (obj == null)
                return;
            HighlightRenderers(obj.renderers, color, overlay, collection);
        }


        static bool TryGetCollection (string collectionName, out OutlineCollection collection) {
            if (collections.TryGetValue(collectionName, out collection)) 
                return true;
            Debug.LogError("Outline Collection name not found: " + collectionName);
            return false;
        }

        public static void HighlightRenderers(Renderer[] renderers, BasicColor color, bool overlay, string collectionName) {
            if (TryGetCollection(collectionName, out OutlineCollection collection)) 
                collection.HighlightRenderers(renderers, color, overlay);
        }
        public static void HighlightRenderer(Renderer renderer, BasicColor color, bool overlay, string collectionName) {
            if (TryGetCollection(collectionName, out OutlineCollection collection)) 
                collection.HighlightRenderer(renderer, color, overlay);
        }
        public static void UnHighlightRenderers(Renderer[] renderers, string collectionName) {
            if (TryGetCollection(collectionName, out OutlineCollection collection)) 
                collection.UnHighlightRenderers(renderers);
        }
        public static void UnHighlightRenderer(Renderer renderer, string collectionName) {
            if (TryGetCollection(collectionName, out OutlineCollection collection)) 
                collection.UnHighlightRenderer(renderer);
        }

        const int maskOutPass = 0;
        const int finalPass = 1;

        static readonly int _MaxDistance = Shader.PropertyToID("_MaxDistance");
        static readonly int _Color = Shader.PropertyToID("_OutColor");
        static readonly int _MaskOut = Shader.PropertyToID("_MaskOut");
        static readonly int _AddOverlay = Shader.PropertyToID("_OverlayHighlights");
        static readonly int _AddHighlight = Shader.PropertyToID("_DepthHighlights");
        static readonly int _MaskAlphaSubtractMult = Shader.PropertyToID("_MaskAlphaSubtractMult");
        static readonly int _OverlayMask = Shader.PropertyToID("_OverlayMask");
        static readonly int _Intensity_Heaviness_OverlayAlphaHelper = Shader.PropertyToID("_Intensity_Heaviness_OverlayAlphaHelper");
        static readonly int _Intensity_Heaviness = Shader.PropertyToID("_Intensity_Heaviness");


        
        static bool HasAny(OutlineCollection collection, out bool needsDepth, out bool needsOverlay) {
            needsDepth = collection.overlays.Contains(false);
            needsOverlay = collection.overlays.Contains(true);
            return needsDepth || needsOverlay;
        }

        static int collectionsDrawnCount = -1;
        public static bool OutlinesActive () {
            collectionsDrawnCount = 0;
            foreach (var collection in collections.Values) {
                collection.Clean();
                if (collection.renderers.Count > 0)
                    collectionsDrawnCount++;
            }
            return collectionsDrawnCount > 0;
        }

        static Shader _fxShader;
        static Shader fxShader {
            get {
                if (_fxShader == null) 
                    _fxShader = Shader.Find("Hidden/Outlines");
                return _fxShader;
            }
        }


        static Material _drawSimpleMaterial;
        static Material drawSimpleMaterial { get { return RenderUtils.CreateMaterialIfNull ("Hidden/Outlined", ref _drawSimpleMaterial); } }

        static int[] GetRTIDs () {
            int[] r = new int[6];
            for (int i = 0; i < r.Length; i++) {
                r[i] = Shader.PropertyToID("_OutlineRT" + i);
            }
            return r;
        }
        static readonly int[] rtIDs = GetRTIDs();
        static int depthTarget { get { return rtIDs[0]; } }
        static int overlayTarget { get { return rtIDs[1]; } }
        static int blurTarget { get { return rtIDs[2]; } }
        static int drawTarget { get { return rtIDs[3]; } }
        static int t0 { get { return rtIDs[4]; } }
        static int t1 { get { return rtIDs[5]; } }
        

        static void BuildTemporaryIDs (CommandBuffer command) {
            for (int i = 0; i < 6; i++)
                command.GetTemporaryRT(rtIDs[i], -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.Default);
        }
        static void ReleaseTempIDs (CommandBuffer command) {
            for (int i = 0; i < 6; i++)
                command.ReleaseTemporaryRT(rtIDs[i]);
        }
            

        /*
            already checked if we have any collections drawn above
        */
        public static void Render(PostProcessRenderContext context, RenderTargetIdentifier source, RenderTargetIdentifier destination)
        {
            PropertySheet sheet = context.propertySheets.Get(fxShader);
            BuildTemporaryIDs(context.command);
            int i = 0;
            int l = collectionsDrawnCount - 1;
            foreach (var c in collections.Values) {
                if (HasAny(c, out bool needsDepth, out bool needsOverlay)) {
                    bool e = i % 2 == 0;
                    RenderOutlines (context, sheet, i == 0 ? source : (e ? t0 : t1), i == l ? destination : (e ? t1 : t0), c, needsDepth, needsOverlay);
                    i++;
                }
            }
            ReleaseTempIDs(context.command);
        }
            
        static void MaskOutInsides (CommandBuffer command, PropertySheet sheet, int target, float maskAlphaSubtractMult) {
            command.SetGlobalTexture(_MaskOut, drawTarget);
            command.SetGlobalFloat(_MaskAlphaSubtractMult, maskAlphaSubtractMult);
            command.BlitFullscreenTriangle(blurTarget, target, sheet, maskOutPass);
        }

        static void RenderLoop (CommandBuffer command, OutlineCollection group, bool overlay, bool depthTested) {        
            command.SetRenderTarget(drawTarget, (depthTested ? (RenderTargetIdentifier)BuiltinRenderTextureType.Depth : drawTarget));
            command.ClearRenderTarget(false, true, Color.clear, 1.0f);
            DrawRenderGroup(command, group, overlay);
        }

        static void RenderOutlines(PostProcessRenderContext context, PropertySheet sheet, RenderTargetIdentifier source, RenderTargetIdentifier destination, OutlineCollection collection, bool needsDepth, bool needsOverlay)
        {

            context.command.SetGlobalFloat(_MaxDistance, collection.def.maxRenderDistance);
            
            if (needsDepth) {
                // render only the highlited objects that are depth tested
                RenderLoop(context.command, collection, false, true);
                
                // blur the render texture we've been drawing to with the tempCamera
                Blur.BlurImage(context, drawTarget, context.screenWidth, context.screenHeight, RenderTextureFormat.Default, blurTarget, collection.def.blur);
                
                /*
                    alternative if blurred outlines against occludign non outlined geometry is bugging you...
                    
                    this remakes the mask to take out the insides of the outlines, except is uses a non depth tested render.
                    this comes with it's own artifacts, specifically when two outlined objects are seperated by an occulder
                    e.g.
                        camera ->  OutlinedA    Wall    OutlinedB
                    in the above situation if outlinedB is larger than OutlinedA, OutlinedA's outline will not be visible
                */
                if (settings.alternateDepthOutlines)
                    RenderLoop(context.command, collection, false, false);
                    
                // now mask out the insides (as well as the alpha)
                MaskOutInsides (context.command, sheet, depthTarget, 1);
            }

            //do overlay highlights
            if (needsOverlay) {
                RenderLoop(context.command, collection, true, false);
                
                Blur.BlurImage(context, drawTarget, context.screenWidth, context.screenHeight, RenderTextureFormat.Default, blurTarget, collection.def.blur);
                
                // mask out the insides, except for alpha (maskAlphaSubtractMult == 0)
                // we need the alpha of the overlay inside to cancel out any 
                // highlight within it that's been drawn by the depth tested passes
                MaskOutInsides (context.command, sheet, overlayTarget, 0);
            }

            // the final pass
            context.command.SetGlobalTexture(_AddHighlight, needsDepth ? depthTarget : (RenderTargetIdentifier)RenderUtils.blackTexture);
            context.command.SetGlobalTexture(_AddOverlay, needsOverlay ? overlayTarget : (RenderTargetIdentifier)RenderUtils.blackTexture);
            
            // we need to pass in the original overlay mask to cancel out the alpha
            // after it cancels out any overlapped depth tested highlights
            // the mask is contained in the camera target
            context.command.SetGlobalTexture(_OverlayMask, needsOverlay ? drawTarget : (RenderTargetIdentifier)RenderUtils.blackTexture);            
            
            context.command.SetGlobalVector(_Intensity_Heaviness_OverlayAlphaHelper, new Vector3(collection.def.intensity, collection.def.heaviness, settings.outlineOverlayAlphaHelper));

            context.command.BlitFullscreenTriangle(source, destination, sheet, finalPass);
        }
        
        static void DrawRenderGroup (CommandBuffer command, OutlineCollection group, bool overlay) {
        
            for (int r = 0; r < group.renderers.Count; r++) {
                if (group.overlays[r] != overlay)
                    continue;
                // maybe account for submeshes here....
                command.SetGlobalColor(_Color,settings.outlineColors.GetColor(group.colors[r])); 
                command.DrawRenderer(group.renderers[r], drawSimpleMaterial, 0, 0);                
            }
        }
            
        public static void RenderOptimized(PostProcessRenderContext context, RenderTargetIdentifier source, RenderTargetIdentifier destination)
        {
            PropertySheet sheet = context.propertySheets.Get(fxShader);
            BuildTemporaryIDs(context.command);
            int i = 0;
            int l = collectionsDrawnCount - 1;
            foreach (var c in collections.Values) {
                if (c.renderers.Count > 0) {
                    bool e = i % 2 == 0;
                    RenderOutline_Optimized (context, sheet, i == 0 ? source : (e ? t0 : t1), i == l ? destination : (e ? t1 : t0), c);
                    i++;
                }
            }
            ReleaseTempIDs(context.command);
        }
        static void RenderOutline_Optimized(PostProcessRenderContext context, PropertySheet sheet, RenderTargetIdentifier source, RenderTargetIdentifier destination, OutlineCollection collection)
        {
            context.command.SetGlobalFloat(_MaxDistance, collection.def.maxRenderDistance);
            
            //render objects
            context.command.SetRenderTarget(drawTarget, (RenderTargetIdentifier)BuiltinRenderTextureType.Depth);
            context.command.ClearRenderTarget(false, true, Color.clear, 1.0f);
            DrawRenderGroup(context.command, collection, false);
            
            //render overlayed
            context.command.SetRenderTarget(drawTarget, drawTarget);
            DrawRenderGroup(context.command, collection, true);

            //blur
            Blur.BlurImage(context, drawTarget, context.screenWidth, context.screenHeight, RenderTextureFormat.Default, blurTarget, collection.def.blur);
            
            // mask out insides
            context.command.SetGlobalTexture(_MaskOut, drawTarget);
            context.command.BlitFullscreenTriangle(blurTarget, depthTarget, sheet, maskOutPass+2);

            // add to final
            context.command.SetGlobalTexture(_AddOverlay, depthTarget);
            context.command.SetGlobalVector(_Intensity_Heaviness, new Vector2(collection.def.intensity, collection.def.heaviness));
            context.command.BlitFullscreenTriangle(source, destination, sheet, finalPass+2);
        }
    }

    public sealed class OutlinesRenderer : PostProcessEffectRenderer<Outlines>
    {
        public override void Render(PostProcessRenderContext context)
        {
            if (RenderingSettings.instance.optimizedOutlines)
                Outlines.RenderOptimized(context, context.source, context.destination);
            else
                Outlines.Render(context, context.source, context.destination);
        }
    }

    [System.Serializable] public class OutlineCollectionDefArray : NeatArrayWrapper<OutlineCollectionDef> { }
    [System.Serializable] public class OutlineCollectionDef {
        public string name;
        [Tooltip("Default: 25")] public float maxRenderDistance = 25;
        [Tooltip("Default: 2")] [Range(0,25)] public float intensity = 2;
        [Tooltip("Default: 1")] [Range(0,5)] public float heaviness = 1;
        public BlurParameters blur;
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(OutlineCollectionDef))]
    class OutlineCollectionDefDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            position.y = GUITools.PropertyFieldAndHeightChange (position, property, "name");
            position.y = GUITools.PropertyFieldAndHeightChange (position, property, "maxRenderDistance");
            position.y = GUITools.PropertyFieldAndHeightChange (position, property, "intensity");
            position.y = GUITools.PropertyFieldAndHeightChange (position, property, "heaviness");
            position.y = GUITools.PropertyFieldAndHeightChange (position, property, "blur");
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight * 4 + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("blur"), true);
        }
    }
    #endif
    
    public class OutlineCollection {
        public OutlineCollectionDef def;
        public List<Renderer> renderers = new List<Renderer>();
        public List<BasicColor> colors = new List<BasicColor>();
        public List<bool> overlays = new List<bool>();
        
        public OutlineCollection (OutlineCollectionDef def) {
            this.def = def;
        }
    
        void Remove (int atIndex) {
            renderers.RemoveAt(atIndex);
            colors.RemoveAt(atIndex);
            overlays.RemoveAt(atIndex);
        }
        public void Clean () {
            for (int i = renderers.Count -1; i >= 0; i--) 
                if (renderers[i] == null || !renderers[i].gameObject.activeInHierarchy) 
                    Remove(i);
        }
    
        public void UnHighlightRenderers(Renderer[] renderers) {
            for (int i = 0; i < renderers.Length; i++) 
                UnHighlightRenderer(renderers[i]);
        }
        public void UnHighlightRenderer(Renderer renderer) {
            if (renderers.Contains(renderer))
                Remove(renderers.IndexOf(renderer));
        }
        public void HighlightRenderers(Renderer[] renderers, BasicColor color, bool overlay) {
            for (int i = 0; i < renderers.Length; i++) 
                HighlightRenderer(renderers[i], color, overlay);
        }
        public void HighlightRenderer(Renderer renderer, BasicColor color, bool overlay) {
            if (!renderers.Contains(renderer)) {
                renderers.Add(renderer);
                colors.Add(color);
                overlays.Add(overlay);
            }
            else {
                int i = renderers.IndexOf(renderer);
                colors[i] = color;
                overlays[i] = overlay;
            }
        }
    }   
}