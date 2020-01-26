using UnityEngine;

namespace UnityTools {

    public static class Vectors
    {
        public static float RandomRange (this Vector2 range) {
            if (range.x == range.y) return range.x;
            return Random.Range(range.x, range.y);
        }

        public static Vector3 GetRandomRange (Vector3 a, Vector3 b) {
            return new Vector3(
                Random.Range(a.x, b.x), 
                Random.Range(a.y, b.y), 
                Random.Range(a.z, b.z)
            );
        }
    }
}
