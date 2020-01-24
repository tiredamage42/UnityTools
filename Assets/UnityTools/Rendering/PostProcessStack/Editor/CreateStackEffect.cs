using UnityEngine;
using UnityEditor;
using System.IO;

namespace UnityTools.EditorTools {
    public static class CreateStackEffect
    {
        [MenuItem("Assets/Create/Stack Effect", priority = 81)]
        static void CreateStackEffectFromScript()
        {
            BuildEffect(Selection.activeObject);
            AssetDatabase.Refresh();
        }
        [MenuItem("Assets/Create/Stack Effect", priority = 81, validate = true)]
        static bool ValidateCreateStackEffectFromScript()
        {
            var script = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(script);
            if (script.GetType() != typeof(MonoScript))
                return false;
            if (!path.EndsWith(".cs"))
                return false;
            return true;
        }
        static void BuildEffect(Object obj)
        {
            MonoScript monoScript = obj as MonoScript;
            string assetPath = AssetDatabase.GetAssetPath(obj);
            string effectName = Path.GetFileNameWithoutExtension(assetPath);
            string directory = Path.GetDirectoryName(assetPath);
            var resources = directory + "/Resources";
            
            if (!Directory.Exists(resources))
                Directory.CreateDirectory(resources);
            else {
                if (File.Exists(resources + "/" + effectName + ".shader")) {
                    Debug.Log("ERROR: " + effectName + ".shader already exists.");
                    return;
                }
            }

            File.WriteAllText(directory + "/" + effectName + ".cs", string.Format(scripttemplate, effectName));
            File.WriteAllText(resources + "/" + effectName + ".shader", string.Format(shaderTemplate, effectName));
            EditorUtility.SetDirty(monoScript);
        }
            
        
        static readonly string scripttemplate = @"
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
namespace UnityTools.Rendering {{
    
    [Serializable]
    [PostProcess(typeof({0}Renderer), PostProcessEvent.AfterStack, ""Custom/{0}"")]
    public sealed class {0} : PostProcessEffectSettings
    {{

        // [Range(0,1)] public FloatParameter param = new FloatParameter() {{ value = 0 }};
        
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {{
            return enabled.value;
        }}
    }}

    public sealed class {0}Renderer : PostProcessEffectRenderer<{0}>
    {{
        static Shader _shader; 
		static Shader shader {{ get {{ return RenderUtils.GetShaderIfNull(""Hidden/{0}"", ref _shader); }} }}
        
        //static readonly int _Params = Shader.PropertyToID(""_Params"");
        
        public override void Render(PostProcessRenderContext context)
        {{
            var sheet = context.propertySheets.Get(shader);
            
            // sheet.properties.SetVector(_Params, new Vector4( x, y, z, w ));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }}
    }}
}}
";

        static readonly string shaderTemplate = @"
Shader ""Hidden/{0}""
{{
    SubShader
    {{
        Cull Off ZWrite Off ZTest Off Blend Off
        Pass
        {{
            HLSLPROGRAM
            #pragma vertex ImgFXVertex
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include ""ImgFX.hlsl""
            sampler2D _MainTex;
            fixed4 frag (v2f i) : SV_Target {{
                fixed4 main = tex2D(_MainTex, i.uv);
                return fixed4(main.r, main.b, main.g, 1);
            }}
            ENDHLSL
        }}
    }}
}}
";

    }

}