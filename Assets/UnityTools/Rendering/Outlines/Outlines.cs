using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.GameSettingsSystem;

namespace UnityTools.Rendering {
    


    public enum OutlineSortType { Overlay = 1, DepthFilter = 2 }


    public class OutlineGroup {

        
        public OutlineGroup (BasicColor color, OutlineSortType sortingType) {
            this.color = color;
            this.sortingType = sortingType;
        }

        public BasicColor color;
        public OutlineSortType sortingType;
        public List<Renderer> renderers = new List<Renderer>();
        public List<int> originalLayers = new List<int>();
        

        public void Remove (int atIndex) {
            renderers.RemoveAt(atIndex);
            originalLayers.RemoveAt(atIndex);
        }
        public void Clean () {
            for (int i = renderers.Count -1; i >= 0; i--) {
                if (renderers[i] == null) {
                    Remove(i);
                }
            }
        }
        public void Remove (Renderer renderer) {
            if (renderers.Contains(renderer))
                Remove(renderers.IndexOf(renderer));
        }
        public void Add (Renderer renderer) {
            if (!renderers.Contains(renderer)) {
                renderers.Add(renderer);
                originalLayers.Add(renderer.gameObject.layer);
            }
        }
        public void Stage (int stagedLayer) {
            for (int i = 0; i < renderers.Count; i++) {
                originalLayers[i] = renderers[i].gameObject.layer;
                renderers[i].gameObject.layer = stagedLayer;
            }
        }
        public void Unstage () {
            for (int i = 0; i < renderers.Count; i++) {
                renderers[i].gameObject.layer = originalLayers[i];
            }
        }
    }


        

    // [ExecuteInEditMode]
    public static class Outlines 
    {
        const string outlineLayer = "Outline";
        const int maskOutPass = 0;
        const int finalPass = 1;


        static readonly int _Color = Shader.PropertyToID("_OutColor");
        static readonly int _MaskOut = Shader.PropertyToID("_MaskOut");
        static readonly int _AddOverlay = Shader.PropertyToID("_OverlayHighlights");
        static readonly int _AddHighlight = Shader.PropertyToID("_DepthHighlights");
        static readonly int _MaskAlphaSubtractMult = Shader.PropertyToID("_MaskAlphaSubtractMult");
        static readonly int _OverlayMask = Shader.PropertyToID("_OverlayMask");
        static readonly int _Intensity_Heaviness_OverlayAlphaHelper = Shader.PropertyToID("_Intensity_Heaviness_OverlayAlphaHelper");


        static Shader _drawSimpleShader;
        static Shader drawSimpleShader {
            get {
                if (_drawSimpleShader == null)
                    _drawSimpleShader = Shader.Find("Hidden/OutlinesDrawSimple");
                return _drawSimpleShader;
            }
        }

        static Material _fxMaterial;
        static Material fxMaterial {
            get {
                if (_fxMaterial == null) {
                    _fxMaterial = new Material(Shader.Find("Hidden/Outlines"));
                    _fxMaterial.hideFlags = HideFlags.HideAndDontSave;                
                }
                return _fxMaterial;
            }
        }
        static Texture2D _blackTexture;
        static Texture blackTexture {
            get {
                if (_blackTexture == null) {
                    // Debug.Log("rebuilding black texture");
                    //might need to adjust alphas on this...
                    _blackTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false, false);
                    _blackTexture.SetPixels( new Color[] { Color.clear } );
                    _blackTexture.Apply();
                    _blackTexture.hideFlags = HideFlags.HideAndDontSave;
                }
                return _blackTexture;
            }
        }

        static Camera _tempCam;
        static Camera tempCamera {
            get {
                if (_tempCam == null) {
                    _tempCam = new GameObject().AddComponent<Camera>();
                    _tempCam.enabled = false;
                    _tempCam.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }
                return _tempCam;
            }
        }

        static void InitializeTemporaryCamera (int stagedLayer, Camera fromCamera, float maxDistance) {
            //set up a temporary camera
            tempCamera.CopyFrom(fromCamera);
            tempCamera.renderingPath = RenderingPath.VertexLit;
            tempCamera.allowDynamicResolution = false;
            tempCamera.allowHDR = false;
            tempCamera.allowMSAA = false;        
            tempCamera.farClipPlane = maxDistance;
            tempCamera.nearClipPlane = .1f;
            tempCamera.useOcclusionCulling = true; 
            tempCamera.backgroundColor = Color.clear;

            //cull eveyrthing but outlined renderers
            tempCamera.cullingMask = 1<<stagedLayer;
        }
    





        // we need this for the depth mask
        static void RenderSceneWithoutOutlines(int stagedLayer, List<OutlineGroup> groups) {

            //clear before we start renedering
            tempCamera.clearFlags = CameraClearFlags.Color;  
            
            //cull all highlighted renderers
            tempCamera.cullingMask = ~(1<<stagedLayer);

            Shader.SetGlobalColor(_Color, Color.clear);
            
            //stage all
            for (int i = 0; i <groups.Count; i++)
                groups[i].Stage(stagedLayer);
            
            tempCamera.RenderWithShader(drawSimpleShader, "");

            // unstage all
            for (int i = 0; i < groups.Count; i++) 
                groups[i].Unstage();
            
            //cull eveyrthing but outlined renderers
            tempCamera.cullingMask = 1<<stagedLayer;
        }

        static ColorsDefenition _outlineColors;
        static ColorsDefenition outlineColors {
            get {
                if (_outlineColors == null) {
                    _outlineColors = GameSettings.GetSettings<ColorsDefenition>("OutlineColors");
                }
                return _outlineColors;
            }
        }

        static Color GetColor (BasicColor basicColor) {
            return outlineColors.GetColor(basicColor);

        }

        static void RenderLoop (int stagedLayer, List<OutlineGroup> groups, OutlineSortType sortType, bool clear, bool useGroupColor, Color overrideColor) {
            if (!useGroupColor) {
                Shader.SetGlobalColor(_Color, overrideColor);
            }
            
            bool clearedCamera = false;
                
            for (int i = 0; i < groups.Count; i++) {
                OutlineGroup group = groups[i];
                if (group.sortingType == sortType) {
                    
                    tempCamera.clearFlags = clearedCamera || !clear ? CameraClearFlags.Nothing : CameraClearFlags.Color;
                    
                    clearedCamera = true;
                        
                    if (useGroupColor)
                        Shader.SetGlobalColor(_Color, GetColor(group.color));
                    
                    group.Stage(stagedLayer);
                    tempCamera.RenderWithShader(drawSimpleShader, "");
                    group.Unstage();
                }
            }
        }

        static void MaskOutInsides (RenderTexture image, RenderTexture mask, RenderTexture target, float maskAlphaSubtractMult) {
            fxMaterial.SetTexture(_MaskOut, mask);
            fxMaterial.SetFloat(_MaskAlphaSubtractMult, maskAlphaSubtractMult);
            Graphics.Blit(image, target, fxMaterial, maskOutPass);
        }
        

        

        public static void RenderOutlines(RenderTexture source, RenderTexture destination, List<OutlineGroup> outlineGroups, bool needsDepth, bool needsOverlay, Camera renderCam, float maxDistance, float intensity, float heaviness, float overlayAlphaHelper, BlurParameters blur)
        {

            int stagedLayer;
            if (!Layers.LayerExists(outlineLayer, out stagedLayer)) {
                Graphics.Blit(source, destination);
                return;
            }

            int w = source.width;
            int h = source.height;

            InitializeTemporaryCamera(stagedLayer, renderCam, maxDistance);
            //make the temporary rendertexture
            RenderTexture cameraTarget = RenderTexture.GetTemporary(w, h, needsDepth ? 16 : 0, RenderTextureFormat.Default);
            //set the camera's target texture when rendering
            tempCamera.targetTexture = cameraTarget;
        
            
            RenderTexture blurredTarget = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default);
            
            RenderTexture finalDepthTestedHighlighted = null;
            if (needsDepth) {
                finalDepthTestedHighlighted = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default);
                
                // draw the scene without highlighted objects to mask out the depth tested
                // highlights we draw later
                RenderSceneWithoutOutlines(stagedLayer, outlineGroups);
                
                // render only the highlited objects that are depth tested
                // no camera clearint yet, we need the depth buffer from the last pass
                RenderLoop(stagedLayer, outlineGroups, OutlineSortType.DepthFilter, clear: false, true, Color.magenta);
                
                // blur the render texture we've been drawing to with the tempCamera
                Blur.BlurImage(cameraTarget, blurredTarget, blur);
            
                // make a mask to remove the inside to make it an outline
                // render all outlined objects again wihtout any occlusion (this render loop clears the camera)
                // to filter out all possible glow on overlapping objects
                RenderLoop(stagedLayer, outlineGroups, OutlineSortType.DepthFilter, clear: true, false, Color.white);
                
                // Graphics.Blit (blurredTarget, finalDepthTestedHighlighted);
                // now mask out the insides (as well as the alpha)
                MaskOutInsides (blurredTarget, cameraTarget, finalDepthTestedHighlighted, 1);
            }

            //do overlay highlights
            RenderTexture finalOverlayedHighLighted = null;
            if (needsOverlay) {
                finalOverlayedHighLighted = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.Default);
                
                RenderLoop(stagedLayer, outlineGroups, OutlineSortType.Overlay, clear: true, true, Color.magenta);
                
                Blur.BlurImage(cameraTarget, blurredTarget, blur);
                
                // mask out the insides, except for alpha (maskAlphaSubtractMult == 0)
                // we need the alpha of the overlay inside to cancel out any 
                // highlight within it that's been drawn by the depth tested passes
                MaskOutInsides (blurredTarget, cameraTarget, finalOverlayedHighLighted, 0);
            }

            // the final pass
            fxMaterial.SetTexture(_AddHighlight, needsDepth ? finalDepthTestedHighlighted : blackTexture);
            // fxMaterial.SetTexture(_AddHighlight, blackTexture);
            
            fxMaterial.SetTexture(_AddOverlay, needsOverlay ? finalOverlayedHighLighted : blackTexture);
            
            // we need to pass in the original overlay mask to cancel out the alpha
            // after it cancels out any overlapped depth tested highlights
            // the mask is contained in the camera target
            fxMaterial.SetTexture(_OverlayMask, needsOverlay ? cameraTarget : blackTexture);
            
            fxMaterial.SetVector(_Intensity_Heaviness_OverlayAlphaHelper, new Vector3(intensity, heaviness, overlayAlphaHelper));
            
            Graphics.Blit(source, destination, fxMaterial, finalPass);

            // release memory
            RenderTexture.ReleaseTemporary(cameraTarget);
            RenderTexture.ReleaseTemporary(blurredTarget);    
            
            if (needsDepth) RenderTexture.ReleaseTemporary(finalDepthTestedHighlighted);
            if (needsOverlay) RenderTexture.ReleaseTemporary(finalOverlayedHighLighted);
        }
    }
}

