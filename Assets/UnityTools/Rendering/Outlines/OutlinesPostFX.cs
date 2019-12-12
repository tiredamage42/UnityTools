using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityTools.Rendering {
    [ExecuteInEditMode] public class OutlinesPostFX : MonoBehaviour {
        public Renderer debugRenderer;
        public BasicColor debugColor = BasicColor.Red;
        public OutlineSortType debugSortType = OutlineSortType.Overlay;
        public bool resetDebug;


        public float maxRenderDistance = 25;
        [Range(0,25)] public float intensity = 2;
        [Range(0,5)] public float heaviness = 1;
        
        [Tooltip("raise this if depth tested highlight colors bleed into overlay areas")]
        [Range(0.1f, 5)] public float overlayAlphaHelper = 1;
        public BlurParameters blur;

        Camera _attachedCamera;
        Camera attachedCamera { get { return this.GetComponentIfNull<Camera>(ref _attachedCamera, false); } }
        
        Dictionary<string, OutlineGroup> outlineGroupsD = new Dictionary<string, OutlineGroup>();
        List<OutlineGroup> outlineGroupsL = new List<OutlineGroup>();


        
        public void UnHighlightRenderers(List<Renderer> renderers) {
            for (int i = 0; i < renderers.Count; i++) UnHighlightRenderer(renderers[i]);
        }
        public void HighlightRenderers(List<Renderer> renderers, BasicColor color, OutlineSortType sortType) {
            for (int i = 0; i < renderers.Count; i++) HighlightRenderer(renderers[i], color, sortType);
            enabled = true;
        }
        public void UnHighlightRenderers(Renderer[] renderers) {
            for (int i = 0; i < renderers.Length; i++) UnHighlightRenderer(renderers[i]);
        }
        public void HighlightRenderers(Renderer[] renderers, BasicColor color, OutlineSortType sortType) {
            for (int i = 0; i < renderers.Length; i++) HighlightRenderer(renderers[i], color, sortType);
            enabled = true;
        }

        public void HighlightRenderer(Renderer renderer, BasicColor color, OutlineSortType sortType) {

            
            string key = color.ToString() + ((int)sortType).ToString();


            OutlineGroup group;

            if (!outlineGroupsD.TryGetValue(key, out group)) {
                group = new OutlineGroup(color, sortType);
                outlineGroupsD[key] = group;
                outlineGroupsL.Add(group);
            }
            group.Add(renderer);

            for (int i = 0; i < outlineGroupsL.Count; i++) {
                if (outlineGroupsL[i] != group) {
                    outlineGroupsL[i].Remove(renderer);
                }
            }
            
            // if (!enabled)
            //     enabled = true;
        }

        public void UnHighlightRenderer(Renderer renderer) {
            for (int i = 0; i < outlineGroupsL.Count; i++)
                outlineGroupsL[i].Remove(renderer);
            
            
            // if (enabled) {
            //     bool renderingHighlighted = HasAny(out _, out _);
            //     if (!renderingHighlighted)
            //         enabled = false;         
            // }

        }

        void OnEnable () {
            ResetDebug();
            // enabled = HasAny(out _, out _);
        }

        bool HasAny(out bool needsDepth, out bool needsOverlay) {
            needsDepth = needsOverlay = false;
            
            for (int i = 0; i < outlineGroupsL.Count; i++) {
                OutlineGroup group = outlineGroupsL[i];
                group.Clean();
                if (group.renderers.Count > 0) {
                    if (group.sortingType == OutlineSortType.DepthFilter) 
                        needsDepth = true;
                    else 
                        needsOverlay = true;

                    if (needsDepth && needsOverlay)
                        return true;
                }
            }
            return needsDepth || needsOverlay;
        }

        void Update () {
            HandleDebug();
        }

        void HandleDebug () {
            if (resetDebug) {
                ResetDebug();
                resetDebug = false;
            }
        }

        void ResetDebug () {
            if (debugRenderer != null) {
                HighlightRenderer(debugRenderer, debugColor, debugSortType);
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {

            bool needsDepth, needsOverlay;
            bool renderingHighlighted = HasAny(out needsDepth, out needsOverlay);

            if (!renderingHighlighted) {
                
                Graphics.Blit(source, destination);
                // enabled = false;
                return;
            }

            Outlines.RenderOutlines (source, destination, outlineGroupsL, needsDepth, needsOverlay, attachedCamera, maxRenderDistance, intensity, heaviness, overlayAlphaHelper, blur);
        }
    }
}