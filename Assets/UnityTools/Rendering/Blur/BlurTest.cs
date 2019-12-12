using UnityEngine;
namespace UnityTools.Rendering {
    [ExecuteInEditMode]
    public class BlurTest : MonoBehaviour
    {
        public BlurParameters blur;

        void OnRenderImage (RenderTexture source, RenderTexture destination) {
            Blur.BlurImage(source, destination, blur);
        }
    }
}
