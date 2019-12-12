using UnityEngine;

namespace UnityTools {
    public class Layers {
        public static bool LayerExists (string layerName, out int layer, bool logError=true) {
            layer = LayerMask.NameToLayer(layerName);
            if (layer < 0) {
                if (logError) {
                    Debug.LogError("No Layer Named '" + layerName + "' specified in the project, add it to the project settings...");
                }
                return false;
            }
            return true;
        }
    }
}
