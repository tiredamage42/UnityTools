using UnityEngine;
using UnityEditor;
using UnityTools.Spawning;
namespace UnityTools.RandomEncounters.Internal {
    public class RandomEncounterSpawn : MonoBehaviour {
        public PrefabChoiceOrReference prefab;
        public SpawnOptions spawnOptions;

        #if UNITY_EDITOR
        void OnDrawGizmos () {
            Gizmos.color = new Color (0, 1, 0, .5f);
            Gizmos.DrawCube(transform.position + Vector3.up * .5f, new Vector3(.2f, 1, .2f));
        }
        #endif
    }

    #if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RandomEncounterSpawn))] 
    class RandomEncounterSpawnEditor : Editor {
        public override void OnInspectorGUI() {
            EditorGUILayout.HelpBox("Spawn Name Must Be Unique Within Encounter", MessageType.Info);
            base.OnInspectorGUI();
        }
    }
    #endif
}
