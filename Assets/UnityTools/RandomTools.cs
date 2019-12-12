using UnityEngine;

namespace UnityTools {

    public static class RandomTools 
    {
        public static float RandomSign (float value, float mask) {
            return (mask == 0 || value == 0 || Random.value < .5f) ? value : -value;
        }
        public static float RandomSign (float value) {
            return RandomSign(value, 1);
        }
        public static Vector3 RandomSign (Vector3 v, Vector3 mask) {
            return new Vector3 (RandomSign(v.x, mask.x), RandomSign(v.y, mask.y), RandomSign(v.z, mask.z));
        }
        public static Vector3 RandomSign (Vector3 v) {
            return RandomSign(v, Vector3.one);
        }   
        public static float Percent () {
            return Random.Range(0f, 100f);
        }
        public static Vector3 RandomPoint (this Bounds b) {
            return b.center + Vector3.Scale(b.extents, new Vector3 (Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
        }
    }
}
