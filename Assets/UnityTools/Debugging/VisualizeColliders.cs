

using UnityEngine;
using System.Collections.Generic;
using UnityTools.DevConsole;
using UnityTools.Rendering;

namespace UnityTools.Debugging {

    public class ColliderDebug : MonoBehaviour
    {
        static Mesh Collider2Mesh (Collider collider) {
            if (collider as BoxCollider) 
                return RenderUtils.GetMesh(PrimitiveType.Cube);
            else if (collider as CapsuleCollider) 
                return RenderUtils.GetMesh(PrimitiveType.Capsule);
            else if (collider as SphereCollider) 
                return RenderUtils.GetMesh(PrimitiveType.Sphere);
            return null;
        }

        static Material _material;
        static Material material { get { return RenderUtils.CreateMaterialIfNull("Hidden/VisualizeColliders", ref _material); } }
        
        public static List<Collider> colliders = new List<Collider>();
        
        
        [Command("showcollider", "visualize object colliders (by dynamic object key [#/@])", "Rendering", true)]
        public static void VisualizeColliders (string doKey) {
            VisualizeColliders(DynamicObjectManager.GetDynamicObjectFromKey(doKey));
        }
        public static void VisualizeColliders (DynamicObject obj) {
            if (obj == null)
                return;
            VisualizeColliders(obj.colliders);
        }
        public static void VisualizeColliders(Collider[] colliders) {
            for (int i = 0; i < colliders.Length; i++) 
                VisualizeCollider(colliders[i]);
        }
        public static void VisualizeCollider(Collider collider) {
            if (collider as MeshCollider)
                return;
            
            if (!colliders.Contains(collider)) 
                colliders.Add(collider);
        }
        public static void UnvisualizeColliders(Collider[] colliders) {
            for (int i = 0; i < colliders.Length; i++) 
                UnvisualizeCollider(colliders[i]);
        }
        public static void UnvisualizeCollider(Collider collider) {
            if (collider as MeshCollider)
                return;
            if (colliders.Contains(collider))
                colliders.Remove(collider);
        }
        
        void Update()
        {
            for (int i = colliders.Count -1; i >= 0; i--) {
                if (colliders[i] == null || !colliders[i].gameObject.activeInHierarchy) {
                    colliders.RemoveAt(i);
                    continue;
                }
                Graphics.DrawMesh(Collider2Mesh(colliders[i]), colliders[i].transform.localToWorldMatrix, material, 0, null, 0, null, false, false, false);
            }
        }
    }
}