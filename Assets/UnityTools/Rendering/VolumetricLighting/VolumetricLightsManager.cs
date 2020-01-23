using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityTools.Rendering.VolumetricLighting {
    public static class VolumetricLightsManager 
    {

        public static List<VolumetricLight> allLights = new List<VolumetricLight>();
        
        public static void AddLight (VolumetricLight light) {
            if (!allLights.Contains(light))
                allLights.Add(light);
        }
        public static void RemoveLight (VolumetricLight light) {
            allLights.Remove(light);
        }


        public static bool PrepareLightsForRender (Camera cam, float maxDistance, float fadeRange) {
            bool anyRendering = false;
            Vector3 camPos = cam.transform.position;
            float inner = maxDistance - fadeRange;

            int c = allLights.Count;
            for (int i = 0; i < c; i++) {
                VolumetricLight light = allLights[i];
                if (light.lightC.intensity <= 0) {
                    light.finalIntensity = 0;
                    continue;
                }

                if (light.type == LightType.Directional) {
                    light.finalIntensity = light.maxIntensity;
                }
                else {
                    light.cam2Light = camPos - light.transform.position;
                    light.cam2LightSq = light.cam2Light.sqrMagnitude;
                    light.cam2LightDist = Mathf.Sqrt(light.cam2LightSq);

                    float t = 1.0f - Mathf.Clamp01((light.cam2LightDist - inner) / fadeRange);
                    light.finalIntensity = light.maxIntensity * t;
                }
                if (light.finalIntensity > 0) 
                    anyRendering = true;
            }

            return anyRendering;
        }
        public static bool IsDrawingDirectional () {
            for (int i = 0; i < allLights.Count; i++) 
                if (allLights[i].type == LightType.Directional && allLights[i].finalIntensity > 0)
                    return true;
            return false;
        }

    }
}
