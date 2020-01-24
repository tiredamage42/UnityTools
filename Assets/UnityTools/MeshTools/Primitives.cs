

using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;


namespace UnityTools.MeshTools {
    public static class Primitives
    {
        // less vertices than cylinder, uv's messed up at bottom though...
        public static Mesh BuildCone(int subdivisions = 10, float radius = 1f, float height = 1f) {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[subdivisions + 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[(subdivisions * 2) * 3];

            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(0.5f, 0f);
            for (int i = 0, n = subdivisions - 1; i < subdivisions; i++) {
                float ratio = (float)i / n;
                float r = ratio * (Mathf.PI * 2f);
                vertices[i + 1] = new Vector3(Mathf.Cos(r), 0f, Mathf.Sin(r)) * radius;
                uv[i + 1] = new Vector2(ratio, 0f);
            }
            vertices[subdivisions + 1] = new Vector3(0f, height, 0f);
            uv[subdivisions + 1] = new Vector2(0.5f, 1f);

            // construct bottom
            for (int i = 0, n = subdivisions - 1; i < n; i++) {
                int offset = i * 3;
                triangles[offset] = 0;
                triangles[offset + 1] = i + 1;
                triangles[offset + 2] = i + 2;
            }
            // construct sides
            int bottomOffset = subdivisions * 3;
            for (int i = 0, n = subdivisions - 1; i < n; i++) {
                int offset = i * 3 + bottomOffset;
                triangles[offset] = i + 1;
                triangles[offset + 1] = subdivisions + 1;
                triangles[offset + 2] = i + 2;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            return mesh.FinishBuild();
        }

        

    // Note that cylinders (bottomRadius == topRadius) and pyramids (4 sides, topRadius == 0) are types of cones, and can be created with this script.
    public static Mesh BuildCylinder (float height = 1, float bottomRadius = .25f, float topRadius = .05f, int sides = 18) {

        Mesh mesh = new Mesh();
        
        int nbHeightSeg = 1; // Not implemented yet
        
        int nbVerticesCap = sides + 1;
        #region Vertices
        
        // bottom + top + sides
        Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap + sides * nbHeightSeg * 2 + 2];
        int vert = 0;
        float _2pi = Mathf.PI * 2f;
        
        // Bottom cap
        vertices[vert++] = new Vector3(0f, 0f, 0f);
        while( vert <= sides )
        {
            float rad = (float)vert / sides * _2pi;
            vertices[vert] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0f, Mathf.Sin(rad) * bottomRadius);
            vert++;
        }
        
        // Top cap
        vertices[vert++] = new Vector3(0f, height, 0f);
        while (vert <= sides * 2 + 1)
        {
            float rad = (float)(vert - sides - 1)  / sides * _2pi;
            vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
            vert++;
        }
        
        // Sides
        int v = 0;
        while (vert <= vertices.Length - 4 )
        {
            float rad = (float)v / sides * _2pi;
            vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
            vertices[vert + 1] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius);
            vert+=2;
            v++;
        }
        vertices[vert] = vertices[ sides * 2 + 2 ];
        vertices[vert + 1] = vertices[sides * 2 + 3 ];
        #endregion
        
        #region Normales
        
        // bottom + top + sides
        Vector3[] normales = new Vector3[vertices.Length];
        vert = 0;
        
        // Bottom cap
        while( vert  <= sides )
            normales[vert++] = Vector3.down;
        
        // Top cap
        while( vert <= sides * 2 + 1 )
            normales[vert++] = Vector3.up;
        
        // Sides
        v = 0;
        while (vert <= vertices.Length - 4 )
        {			
            float rad = (float)v / sides * _2pi;
            normales[vert] = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
            normales[vert+1] = normales[vert];
            vert += 2;
            v++;
        }
        normales[vert] = normales[ sides * 2 + 2 ];
        normales[vert + 1] = normales[sides * 2 + 3 ];
        #endregion
        
        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        
        // Bottom cap
        int u = 0;
        uvs[u++] = new Vector2(0.5f, 0.5f);
        while (u <= sides)
        {
            float rad = (float)u / sides * _2pi;
            uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
            u++;
        }
        
        // Top cap
        uvs[u++] = new Vector2(0.5f, 0.5f);
        while (u <= sides * 2 + 1)
        {
            float rad = (float)u / sides * _2pi;
            uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
            u++;
        }
        
        // Sides
        int u_sides = 0;
        while (u <= uvs.Length - 4 )
        {
            float t = (float)u_sides / sides;
            uvs[u] = new Vector3(t, 1f);
            uvs[u + 1] = new Vector3(t, 0f);
            u += 2;
            u_sides++;
        }
        uvs[u] = new Vector2(1f, 1f);
        uvs[u + 1] = new Vector2(1f, 0f);
        #endregion 
        
        #region Triangles
        int nbTriangles = sides + sides + sides * 2;
        int[] triangles = new int[nbTriangles * 3 + 3];
        
        // Bottom cap
        int tri = 0;
        int i = 0;
        while (tri < sides - 1)
        {
            triangles[ i ] = 0;
            triangles[ i+1 ] = tri + 1;
            triangles[ i+2 ] = tri + 2;
            tri++;
            i += 3;
        }
        triangles[i] = 0;
        triangles[i + 1] = tri + 1;
        triangles[i + 2] = 1;
        tri++;
        i += 3;
        
        // Top cap
        //tri++;
        while (tri < sides * 2)
        {
            triangles[ i ] = tri + 2;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = nbVerticesCap;
            tri++;
            i += 3;
        }
        
        triangles[i] = nbVerticesCap + 1;
        triangles[i + 1] = tri + 1;
        triangles[i + 2] = nbVerticesCap;		
        tri++;
        i += 3;
        tri++;
        
        // Sides
        while( tri <= nbTriangles )
        {
            triangles[ i ] = tri + 2;
            triangles[ i+1 ] = tri + 1;
            triangles[ i+2 ] = tri + 0;
            tri++;
            i += 3;
        
            triangles[ i ] = tri + 1;
            triangles[ i+1 ] = tri + 2;
            triangles[ i+2 ] = tri + 0;
            tri++;
            i += 3;
        }
        #endregion
        
        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        
        return mesh;
    }


    // Outter shell is at radius1 + radius2 / 2, inner shell at radius1 - radius2 / 2
    public static Mesh BuildTube (float height=1, float bottomRadius1 = .5f, float bottomRadius2 = .15f, float topRadius1 = .5f, float topRadius2 = .15f, int sides=24) {
        
        Mesh mesh = new Mesh();
        
        int nbVerticesCap = sides * 2 + 2;
        int nbVerticesSides = sides * 2 + 2;
        #region Vertices
        
        // bottom + top + sides
        Vector3[] vertices = new Vector3[nbVerticesCap * 2 + nbVerticesSides * 2];
        int vert = 0;
        float _2pi = Mathf.PI * 2f;
        
        // Bottom cap
        int sideCounter = 0;
        while( vert < nbVerticesCap )
        {
            sideCounter = sideCounter == sides ? 0 : sideCounter;
        
            float r1 = (float)(sideCounter++) / sides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
            vertices[vert] = new Vector3( cos * (bottomRadius1 - bottomRadius2 * .5f), 0f, sin * (bottomRadius1 - bottomRadius2 * .5f));
            vertices[vert+1] = new Vector3( cos * (bottomRadius1 + bottomRadius2 * .5f), 0f, sin * (bottomRadius1 + bottomRadius2 * .5f));
            vert += 2;
        }
        
        // Top cap
        sideCounter = 0;
        while( vert < nbVerticesCap * 2 )
        {
            sideCounter = sideCounter == sides ? 0 : sideCounter;
        
            float r1 = (float)(sideCounter++) / sides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
            vertices[vert] = new Vector3( cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
            vertices[vert+1] = new Vector3( cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
            vert += 2;
        }
        
        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides )
        {
            sideCounter = sideCounter == sides ? 0 : sideCounter;
        
            float r1 = (float)(sideCounter++) / sides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
        
            vertices[vert] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0, sin * (bottomRadius1 + bottomRadius2 * .5f));
            vert += 2;
        }
        
        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length )
        {
            sideCounter = sideCounter == sides ? 0 : sideCounter;
        
            float r1 = (float)(sideCounter++) / sides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
        
            vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0, sin * (bottomRadius1 - bottomRadius2 * .5f));
            vert += 2;
        }
        #endregion
        
        #region Normales
        
        // bottom + top + sides
        Vector3[] normales = new Vector3[vertices.Length];
        vert = 0;
        
        // Bottom cap
        while( vert < nbVerticesCap )
        {
            normales[vert++] = Vector3.down;
        }
        
        // Top cap
        while( vert < nbVerticesCap * 2 )
        {
            normales[vert++] = Vector3.up;
        }
        
        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides )
        {
            sideCounter = sideCounter == sides ? 0 : sideCounter;
        
            float r1 = (float)(sideCounter++) / sides * _2pi;
        
            normales[vert] = new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1));
            normales[vert+1] = normales[vert];
            vert+=2;
        }
        
        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length )
        {
            sideCounter = sideCounter == sides ? 0 : sideCounter;
        
            float r1 = (float)(sideCounter++) / sides * _2pi;
        
            normales[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
            normales[vert+1] = normales[vert];
            vert+=2;
        }
        #endregion
        
        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        
        vert = 0;
        // Bottom cap
        sideCounter = 0;
        while( vert < nbVerticesCap )
        {
            float t = (float)(sideCounter++) / sides;
            uvs[ vert++ ] = new Vector2( 0f, t );
            uvs[ vert++ ] = new Vector2( 1f, t );
        }
        
        // Top cap
        sideCounter = 0;
        while( vert < nbVerticesCap * 2 )
        {
            float t = (float)(sideCounter++) / sides;
            uvs[ vert++ ] = new Vector2( 0f, t );
            uvs[ vert++ ] = new Vector2( 1f, t );
        }
        
        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides )
        {
            float t = (float)(sideCounter++) / sides;
            uvs[ vert++ ] = new Vector2( t, 0f );
            uvs[ vert++ ] = new Vector2( t, 1f );
        }
        
        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length )
        {
            float t = (float)(sideCounter++) / sides;
            uvs[ vert++ ] = new Vector2( t, 0f );
            uvs[ vert++ ] = new Vector2( t, 1f );
        }
        #endregion
        
        #region Triangles
        int nbFace = sides * 4;
        int nbTriangles = nbFace * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];
        
        // Bottom cap
        int i = 0;
        sideCounter = 0;
        while (sideCounter < sides)
        {
            int current = sideCounter * 2;
            int next = sideCounter * 2 + 2;
        
            triangles[ i++ ] = next + 1;
            triangles[ i++ ] = next;
            triangles[ i++ ] = current;
        
            triangles[ i++ ] = current + 1;
            triangles[ i++ ] = next + 1;
            triangles[ i++ ] = current;
        
            sideCounter++;
        }
        
        // Top cap
        while (sideCounter < sides * 2)
        {
            int current = sideCounter * 2 + 2;
            int next = sideCounter * 2 + 4;
        
            triangles[ i++ ] = current;
            triangles[ i++ ] = next;
            triangles[ i++ ] = next + 1;
        
            triangles[ i++ ] = current;
            triangles[ i++ ] = next + 1;
            triangles[ i++ ] = current + 1;
        
            sideCounter++;
        }
        
        // Sides (out)
        while( sideCounter < sides * 3 )
        {
            int current = sideCounter * 2 + 4;
            int next = sideCounter * 2 + 6;
        
            triangles[ i++ ] = current;
            triangles[ i++ ] = next;
            triangles[ i++ ] = next + 1;
        
            triangles[ i++ ] = current;
            triangles[ i++ ] = next + 1;
            triangles[ i++ ] = current + 1;
        
            sideCounter++;
        }
        
        
        // Sides (in)
        while( sideCounter < sides * 4 )
        {
            int current = sideCounter * 2 + 6;
            int next = sideCounter * 2 + 8;
        
            triangles[ i++ ] = next + 1;
            triangles[ i++ ] = next;
            triangles[ i++ ] = current;
        
            triangles[ i++ ] = current + 1;
            triangles[ i++ ] = next + 1;
            triangles[ i++ ] = current;
        
            sideCounter++;
        }
        #endregion
        
        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }








        








        public static Mesh BuildCube (Vector3 size, Vector3Int resolutions) {
            resolutions.x = Mathf.Max(2, resolutions.x);
            resolutions.y = Mathf.Max(2, resolutions.y);
            resolutions.z = Mathf.Max(2, resolutions.z);
            
            var mesh = new Mesh();

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            var hw = size.x * 0.5f;
            var hh = size.y * 0.5f;
            var hd = size.z * 0.5f;

            // front
            CalculatePlane(vertices, uvs, triangles, Vector3.forward * -hd, Vector3.right * size.x, Vector3.up * size.y, Vector3.zero, null, resolutions.x, resolutions.y);
            // right
            CalculatePlane(vertices, uvs, triangles, Vector3.right * hw, Vector3.forward * size.z, Vector3.up * size.y, Vector3.zero, null, resolutions.z, resolutions.y);
            // back
            CalculatePlane(vertices, uvs, triangles, Vector3.forward * hd, Vector3.left * size.x, Vector3.up * size.y, Vector3.zero, null, resolutions.x, resolutions.y);
            // left
            CalculatePlane(vertices, uvs, triangles, Vector3.right * -hw, Vector3.back * size.z, Vector3.up * size.y, Vector3.zero, null, resolutions.z, resolutions.y);
            // top
            CalculatePlane(vertices, uvs, triangles, Vector3.up * hh, Vector3.right * size.x, Vector3.forward * size.z, Vector3.zero, null, resolutions.x, resolutions.z);
            // bottom
            CalculatePlane(vertices, uvs, triangles, Vector3.up * -hh, Vector3.right * size.x, Vector3.back * size.z, Vector3.zero, null, resolutions.x, resolutions.z);

            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.SetTriangles(triangles.ToArray(), 0);

            return mesh.FinishBuild();
        }

        public static Mesh BuildFrustum(Vector3 forward, Vector3 up, Matrix4x4 projectionMatrix) {
			var m00 = projectionMatrix.m00;
			var m11 = projectionMatrix.m11;
			var m22 = - projectionMatrix.m22;
			var m23 = - projectionMatrix.m23;
			var nearClip = (2f * m23) / (2f * m22 - 2f);
			var farClip = ((m22 - 1f) * nearClip) / (m22 + 1f);
			var fov = Mathf.Atan(1f / m11) * 2f * Mathf.Rad2Deg;
			var aspectRatio = (1f / m00) / (1f / m11);
			return BuildFrustum(forward, up, nearClip, farClip, fov, aspectRatio);
		}

        public static Mesh BuildFrustum(Vector3 forward, Vector3 up, float nearClip, float farClip, float fieldOfView = 60f, float aspectRatio = 1f) {
            var mesh = new Mesh();

            forward = forward.normalized;
            up = up.normalized;
            var left = Vector3.Cross(forward, up);

            var hfov = fieldOfView * 0.5f * Mathf.Deg2Rad;
            var near = forward * nearClip;
            var far = forward * farClip;

            var nearUp = up * Mathf.Tan(hfov) * nearClip;
            var nearLeft = left * Mathf.Tan(hfov) * nearClip * aspectRatio;

            var farUp = up * Mathf.Tan(hfov) * farClip;
            var farLeft = left * Mathf.Tan(hfov) * farClip * aspectRatio;

            mesh.vertices = new Vector3[] {
                near + nearUp + nearLeft, // near top left
                near + nearUp - nearLeft, // near top right
                near - nearUp - nearLeft, // near bottom right
                near - nearUp + nearLeft, // near bototm left

                far + farUp + farLeft, // far top left
                far + farUp - farLeft, // far top right
                far - farUp - farLeft, // far bottom right
                far - farUp + farLeft // far bottom left
            };

            mesh.uv = new Vector2[] {
                new Vector2(0, 1), // near top left
                new Vector2(1, 1), // near top right
                new Vector2(1, 0), // near bottom right
                new Vector2(0, 0), // near bototm left

                new Vector2(0, 1), // far top left
                new Vector2(1, 1), // far top right
                new Vector2(1, 0), // far bottom right
                new Vector2(0, 0) // far bottom left
            };

            mesh.triangles = new int[] {
                // front
                0, 1, 2, 2, 3, 0,
                // back
                4, 6, 5, 6, 4, 7,
                // top
                0, 5, 1, 4, 5, 0,
                // bottom
                3, 2, 7, 6, 7, 2,
                // left
                0, 3, 4, 3, 7, 4,
                // right
                1, 5, 6, 6, 2, 1,
            };

            return mesh.FinishBuild();
        }
















        
        static void CalculatePlane (List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, Vector3 origin, Vector3 right, Vector3 up, Vector3 fwd, Func<float, float, float> getHeight, int xRes = 2, int yRes = 2) {
            
            float rInv = 1f / (xRes - 1);
            float uInv = 1f / (yRes - 1);

            int triangleOffset = vertices.Count;

            for(int y = 0; y < yRes; y++) {
                float ru = y * uInv;
                for(int x = 0; x < xRes; x++) {
                    float rr = x * rInv;

                    if (getHeight == null) 
                        vertices.Add(origin + right * (rr - 0.5f) + up * (ru - 0.5f));
                    else 
                        vertices.Add(origin + right * (rr - 0.5f) + up * (ru - 0.5f) + fwd * getHeight(rr, ru));
                    
                    uvs.Add(new Vector2(rr, ru));
                }

                if(y < yRes - 1) {
                    var offset = y * xRes + triangleOffset;
                    for(int x = 0, n = xRes - 1; x < n; x++) {
                        triangles.Add(offset + x);
                        triangles.Add(offset + x + xRes);
                        triangles.Add(offset + x + 1);

                        triangles.Add(offset + x + 1);
                        triangles.Add(offset + x + xRes);
                        triangles.Add(offset + x + 1 + xRes);
                    }
                }
            }
        }


        public static Mesh BuildPlane (float width = 1f, float height = 1f, int xRes = 1, int yRes = 1) {
            return BuildPlane(null, width, height, xRes, yRes);
        }

        public static Mesh BuildPlane(ParametricPlane param, float width = 1f, float height = 1f, int xRes = 1, int yRes = 1) {
            xRes = Mathf.Max(1, xRes);
            yRes = Mathf.Max(1, yRes);

            var mesh = new Mesh();

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var uvs = new List<Vector2>();

            Func<float, float, float> getHeight = null;
            if (param != null) 
                getHeight = param.Height;
            CalculatePlane (vertices, uvs, triangles, Vector3.zero, Vector3.right * width, Vector3.forward * height, Vector3.up, getHeight, xRes, yRes);

            mesh.vertices = vertices.ToArray();
            mesh.SetTriangles(triangles.ToArray(), 0);
            mesh.uv = uvs.ToArray();

            return mesh.FinishBuild();
        }

        

        static Vector3[] SphereVerts (float radius, int lonSegments, int latSegments) {
            float pi2 = Mathf.PI * 2f;
            var vertices = new Vector3[(lonSegments + 1) * latSegments + 2];
            vertices[0] = Vector3.up * radius;
            for (int lat = 0; lat < latSegments; lat++) {
                float a1 = Mathf.PI * (float)(lat + 1) / (latSegments + 1);
                float sin = Mathf.Sin(a1);
                float cos = Mathf.Cos(a1);
                for (int lon = 0; lon <= lonSegments; lon++) {
                    float a2 = pi2 * (float)(lon == lonSegments ? 0 : lon) / lonSegments;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);
                    vertices[lon + lat * (lonSegments + 1) + 1] = new Vector3(sin * cos2, cos, sin * sin2) * radius;
                }
            }
            vertices[vertices.Length - 1] = Vector3.up * -radius;
            return vertices;
        }

        static Vector2[] SphereUVs (int length, int lonSegments, int latSegments) {
            Vector2[] uvs = new Vector2[length];
            uvs[0] = Vector2.up;
            uvs[uvs.Length - 1] = Vector2.zero;
            for (int lat = 0; lat < latSegments; lat++) 
                for (int lon = 0; lon <= lonSegments; lon++) 
                    uvs[lon + lat * (lonSegments + 1) + 1] = new Vector2((float)lon / lonSegments, 1f - (float)(lat + 1) / (latSegments + 1));
            return uvs;   
        }
        static int[] SphereTris (int length, int lonSegments, int latSegments) {
            int[] triangles = new int[length * 2 * 3];
            // top cap
            int acc = 0;
            for (int lon = 0; lon < lonSegments; lon++) {
                triangles[acc++] = lon + 2;
                triangles[acc++] = lon + 1;
                triangles[acc++] = 0;
            }
            // middle
            for (int lat = 0; lat < latSegments - 1; lat++) {
                for (int lon = 0; lon < lonSegments; lon++) {
                    int current = lon + lat * (lonSegments + 1) + 1;
                    int next = current + lonSegments + 1;
                    triangles[acc++] = current;
                    triangles[acc++] = current + 1;
                    triangles[acc++] = next + 1;
                    triangles[acc++] = current;
                    triangles[acc++] = next + 1;
                    triangles[acc++] = next;
                }
            }
            // bottom cap
            for (int lon = 0; lon < lonSegments; lon++) {
                triangles[acc++] = length - 1;
                triangles[acc++] = length - (lon + 2) - 1;
                triangles[acc++] = length - (lon + 1) - 1;
            }
            return triangles;
        }


        static Vector3[] SphereNormals (Vector3[] vertices) {
            Vector3[] normals = new Vector3[vertices.Length];
            for (int n = 0; n < vertices.Length; n++)
                normals[n] = vertices[n].normalized;
            return normals;
        }

        public static Mesh BuildSphere(float radius = 1f, int lonSegments = 24, int latSegments = 16) {
            var mesh = new Mesh();
            var vertices = SphereVerts (radius, lonSegments, latSegments);
            mesh.vertices = vertices;
            mesh.uv = SphereUVs (vertices.Length, lonSegments, latSegments);
            mesh.triangles = SphereTris (vertices.Length, lonSegments, latSegments);
            mesh.normals = SphereNormals(vertices);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }
         
        // return index of point in the middle of p1 and p2
        static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
        {
            // first check if we have it already
            bool firstIsSmaller = p1 < p2;
            long smallerIndex = firstIsSmaller ? p1 : p2;
            long greaterIndex = firstIsSmaller ? p2 : p1;
            long key = (smallerIndex << 32) + greaterIndex;
    
            int ret;
            if (cache.TryGetValue(key, out ret))
                return ret;
            
            // not in cache, calculate it
            Vector3 point1 = vertices[p1];
            Vector3 point2 = vertices[p2];
            Vector3 middle = (point1 + point2) * .5f;
            // add vertex makes sure point is on unit sphere
            int i = vertices.Count;
            vertices.Add( middle.normalized * radius ); 
            // store it, return index
            cache.Add(key, i);
            return i;
        }
    


        // recursion level = 0 makes good "debris"

        // This is a sphere without poles. Source : http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html
        // no uvs...
        public static Mesh CreateIcoSphere(float radius = 1f, int recursionLevel = 3)
        {
            Mesh mesh = new Mesh();
            List<Vector3> vertList = new List<Vector3>();
            Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
            
            // create 12 vertices of a icosahedron
            float t = (1f + Mathf.Sqrt(5f)) / 2f;
            vertList.Add(new Vector3(-1f,  t,  0f).normalized * radius);
            vertList.Add(new Vector3( 1f,  t,  0f).normalized * radius);
            vertList.Add(new Vector3(-1f, -t,  0f).normalized * radius);
            vertList.Add(new Vector3( 1f, -t,  0f).normalized * radius);
    
            vertList.Add(new Vector3( 0f, -1f,  t).normalized * radius);
            vertList.Add(new Vector3( 0f,  1f,  t).normalized * radius);
            vertList.Add(new Vector3( 0f, -1f, -t).normalized * radius);
            vertList.Add(new Vector3( 0f,  1f, -t).normalized * radius);
    
            vertList.Add(new Vector3( t,  0f, -1f).normalized * radius);
            vertList.Add(new Vector3( t,  0f,  1f).normalized * radius);
            vertList.Add(new Vector3(-t,  0f, -1f).normalized * radius);
            vertList.Add(new Vector3(-t,  0f,  1f).normalized * radius);
    
            // create 20 triangles of the icosahedron
            List<Vector3Int> faces = new List<Vector3Int>();
    
            // 5 faces around point 0
            faces.Add(new Vector3Int(0, 11, 5));
            faces.Add(new Vector3Int(0, 5, 1));
            faces.Add(new Vector3Int(0, 1, 7));
            faces.Add(new Vector3Int(0, 7, 10));
            faces.Add(new Vector3Int(0, 10, 11));
    
            // 5 adjacent faces 
            faces.Add(new Vector3Int(1, 5, 9));
            faces.Add(new Vector3Int(5, 11, 4));
            faces.Add(new Vector3Int(11, 10, 2));
            faces.Add(new Vector3Int(10, 7, 6));
            faces.Add(new Vector3Int(7, 1, 8));
    
            // 5 faces around point 3
            faces.Add(new Vector3Int(3, 9, 4));
            faces.Add(new Vector3Int(3, 4, 2));
            faces.Add(new Vector3Int(3, 2, 6));
            faces.Add(new Vector3Int(3, 6, 8));
            faces.Add(new Vector3Int(3, 8, 9));
    
            // 5 adjacent faces 
            faces.Add(new Vector3Int(4, 9, 5));
            faces.Add(new Vector3Int(2, 4, 11));
            faces.Add(new Vector3Int(6, 2, 10));
            faces.Add(new Vector3Int(8, 6, 7));
            faces.Add(new Vector3Int(9, 8, 1));
    
            // refine triangles
            for (int i = 0; i < recursionLevel; i++)
            {
                List<Vector3Int> faces2 = new List<Vector3Int>();
                foreach (var tri in faces)
                {
                    // replace triangle by 4 triangles
                    int a = getMiddlePoint(tri.x, tri.y, ref vertList, ref middlePointIndexCache, radius);
                    int b = getMiddlePoint(tri.y, tri.z, ref vertList, ref middlePointIndexCache, radius);
                    int c = getMiddlePoint(tri.z, tri.x, ref vertList, ref middlePointIndexCache, radius);
    
                    faces2.Add(new Vector3Int(tri.x, a, c));
                    faces2.Add(new Vector3Int(tri.y, b, a));
                    faces2.Add(new Vector3Int(tri.z, c, b));
                    faces2.Add(new Vector3Int(a, b, c));
                }
                faces = faces2;
            }
    
            mesh.vertices = vertList.ToArray();
    
            List< int > triList = new List<int>();
            for (int i = 0; i < faces.Count; i++)
            {
                triList.Add( faces[i].x );
                triList.Add( faces[i].y );
                triList.Add( faces[i].z );
            }		

            mesh.triangles = triList.ToArray();
            mesh.uv = new Vector2[ mesh.vertices.Length ];
            mesh.normals = SphereNormals(mesh.vertices);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        public static Mesh Torus (float radius1 = 1f, float radius2 = .3f, int nbRadSeg = 24, int nbSides = 18) {

            Mesh mesh = new Mesh();
            
            #region Vertices		
            Vector3[] vertices = new Vector3[(nbRadSeg+1) * (nbSides+1)];
            float _2pi = Mathf.PI * 2f;
            for( int seg = 0; seg <= nbRadSeg; seg++ )
            {
                int currSeg = seg  == nbRadSeg ? 0 : seg;
            
                float t1 = (float)currSeg / nbRadSeg * _2pi;
                Vector3 r1 = new Vector3( Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1 );
            
                for( int side = 0; side <= nbSides; side++ )
                {
                    int currSide = side == nbSides ? 0 : side;
            
                    Vector3 normale = Vector3.Cross( r1, Vector3.up );
                    float t2 = (float)currSide / nbSides * _2pi;
                    Vector3 r2 = Quaternion.AngleAxis( -t1 * Mathf.Rad2Deg, Vector3.up ) *new Vector3( Mathf.Sin(t2) * radius2, Mathf.Cos(t2) * radius2 );
            
                    vertices[side + seg * (nbSides+1)] = r1 + r2;
                }
            }
            #endregion
            
            #region Normales		
            Vector3[] normales = new Vector3[vertices.Length];
            for( int seg = 0; seg <= nbRadSeg; seg++ )
            {
                int currSeg = seg  == nbRadSeg ? 0 : seg;
            
                float t1 = (float)currSeg / nbRadSeg * _2pi;
                Vector3 r1 = new Vector3( Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1 );
            
                for( int side = 0; side <= nbSides; side++ )
                    normales[side + seg * (nbSides+1)] = (vertices[side + seg * (nbSides+1)] - r1).normalized;
                
            }
            #endregion
            
            #region UVs
            Vector2[] uvs = new Vector2[vertices.Length];
            for( int seg = 0; seg <= nbRadSeg; seg++ )
                for( int side = 0; side <= nbSides; side++ )
                    uvs[side + seg * (nbSides+1)] = new Vector2( (float)seg / nbRadSeg, (float)side / nbSides );
            #endregion
            
            #region Triangles
            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[ nbIndexes ];
            
            int i = 0;
            for( int seg = 0; seg <= nbRadSeg; seg++ )
            {			
                for( int side = 0; side <= nbSides - 1; side++ )
                {
                    int current = side + seg * (nbSides+1);
                    int next = side + (seg < (nbRadSeg) ?(seg+1) * (nbSides+1) : 0);
            
                    if( i < triangles.Length - 6 )
                    {
                        triangles[i++] = current;
                        triangles[i++] = next;
                        triangles[i++] = next+1;
            
                        triangles[i++] = current;
                        triangles[i++] = next+1;
                        triangles[i++] = current+1;
                    }
                }
            }
            #endregion
            
            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }
    }

    public abstract class ParametricPlane {
        public abstract float Height(float ux, float uy);
    }

    public class ParametricPlaneRandom : ParametricPlane {
        float height;
        public ParametricPlaneRandom (float height = 1f) {
            this.height = height;
        }
        public override float Height(float ux, float uy) {
            return Random.value * height;
        }
    }

    public class ParametricPlanePerlin : ParametricPlane {
        Vector2 offset, scale;
        float height;
        public ParametricPlanePerlin (Vector2 offset, Vector2 scale, float height = 1f) {
            this.offset = offset;
            this.scale = scale;
            this.height = height;
        }
        public override float Height(float ux, float uy) {
            return (Mathf.PerlinNoise(offset.x + ux * scale.x, offset.y + uy * scale.y) * 2 - 1) * height;
        }
    }

}






