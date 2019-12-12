using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.GameSettingsSystem;
namespace UnityTools.Rendering {

    public class Tagging : InitializationSingleTon<Tagging>
    {



		// const float rimPower = 1.25f;
        
		static Shader _tagShaderSeeThru; 
		static Shader tagShaderSeeThru {
			get {
				if (_tagShaderSeeThru == null) _tagShaderSeeThru = Shader.Find("Hidden/TaggedSeeThru");
				return _tagShaderSeeThru;
			}
		}
        static Shader _tagShader; 
		static Shader tagShader {
			get {
				if (_tagShader == null) _tagShader = Shader.Find("Hidden/Tagged");
				return _tagShader;
			}
		}

        static Dictionary<string, Material> color2Mat = new Dictionary<string, Material>();
        static List<Material> flashMaterials = new List<Material>();


        static readonly int _Color = Shader.PropertyToID("_Color");
        static readonly int _FlashRimPower = Shader.PropertyToID("_FlashRimPower");
        

        void Update () {
            UpdateFlashMaterials();
        }
        

        static void UpdateFlashMaterials () {
            
            Vector2 flashRimpower = Vector2.zero;
                
            flashRimpower.x = Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Time.time * TagSettings.instance.flashSpeed)), TagSettings.instance.flashSteepness);
            flashRimpower.y = Mathf.Lerp(TagSettings.instance.flashRimPowerRange.x, TagSettings.instance.flashRimPowerRange.y, flashRimpower.x);
            for (int i = 0; i < flashMaterials.Count; i++) {
                flashMaterials[i].SetVector(_FlashRimPower, flashRimpower);
            }
        }


        static Material GetMaterialForColor (BasicColor color, bool seeThru, bool flash) {
            
            string key = color.ToString() + seeThru.ToString() + flash.ToString();
            Material mat;
            if (!color2Mat.TryGetValue(key, out mat)) {
                mat = GetNewMaterial( TagSettings.instance.GetColor(color), seeThru );
                if (flash) 
                    flashMaterials.Add(mat);
                
                color2Mat[key] = mat;
            }
            return mat;
        }

		static Material GetNewMaterial(Color color, bool seeThru) {
			Material _mat = new Material(seeThru ? tagShaderSeeThru : tagShader);
			_mat.SetVector(_FlashRimPower, new Vector2(1, TagSettings.instance.defaultRimPower));
			_mat.SetColor(_Color, color);

			_mat.enableInstancing = true;
            _mat.hideFlags = HideFlags.HideAndDontSave;
			return _mat;
		}

        static bool MaterialIsTag (Material mat) {
            return mat.shader == tagShader || mat.shader == tagShaderSeeThru;
        }

		public static void UntagRenderer (Renderer r) {
			Material[] materials = r.sharedMaterials;
			int lastIndex = materials.Length - 1;

            if (MaterialIsTag(materials[lastIndex])) {
            	System.Array.Resize(ref materials, lastIndex);
				r.sharedMaterials = materials;
			}
		}
		public static void TagRenderer (Renderer r, BasicColor color, bool seeThru, bool flash) {
			Material[] materials = r.sharedMaterials;
			
			int lastIndex = materials.Length - 1;

            if (!MaterialIsTag(materials[lastIndex])) {
				System.Array.Resize(ref materials, materials.Length+1);
			}
   
            materials[materials.Length - 1] = GetMaterialForColor(color, seeThru, flash);
            r.sharedMaterials = materials;
            
		}

        public static void TagRenderers (Renderer[] renderers, BasicColor color, bool seeThru, bool flash) {
            for (int i = 0; i < renderers.Length; i++) {
                TagRenderer(renderers[i], color, seeThru, flash);
            }
        }
        public static void UntagRenderers (Renderer[] renderers) {
            for (int i = 0; i < renderers.Length; i++) {
                UntagRenderer(renderers[i]);
            }
        }
    }
}

