// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;


namespace UnityTools.MeshTools {

    public static class MeshUtils 
    {
        public static Mesh FinishBuild(this Mesh mesh) {
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}
