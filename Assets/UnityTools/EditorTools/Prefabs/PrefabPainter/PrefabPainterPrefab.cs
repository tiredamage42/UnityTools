using UnityEngine;
using UnityEditor;
using System;
namespace UnityTools.EditorTools.Internal {
    [Serializable] public class PrefabPainterPrefabValues {
        public Vector3 posOffsetMin, posOffsetMax;
        public Vector3 rotOffsetMin, rotOffsetMax;
        public Vector2 scaleMultiplierRange = Vector2.one;
        
        public PrefabPainterPrefabValues() {
            scaleMultiplierRange = Vector2.one;
        }
        public PrefabPainterPrefabValues (PrefabPainterPrefabValues template) {
            posOffsetMin = template.posOffsetMin; 
            posOffsetMax = template.posOffsetMax;
            rotOffsetMin = template.rotOffsetMin;
            rotOffsetMax = template.rotOffsetMax;
            scaleMultiplierRange = template.scaleMultiplierRange;
        }

        public void AdjustTransform (out Vector3 position, out Vector3 rotation, out float scaleMultiplier) {
            position = Vectors.GetRandomRange(posOffsetMin, posOffsetMax);
            rotation = Vectors.GetRandomRange(rotOffsetMin, rotOffsetMax);
            scaleMultiplier = scaleMultiplierRange.RandomRange();        
        }
    }

    public class PrefabPainterPrefab : MonoBehaviour
    {
        public PrefabPainterPrefabValues painterValues = new PrefabPainterPrefabValues();
    }

    
    #if UNITY_EDITOR
    [CustomEditor(typeof(PrefabPainterPrefab))] class PrefabPainterPrefabEditor : Editor {
        public override void OnInspectorGUI() {
            // base.OnInspectorGUI();
        }
    }
    #endif

    
}

