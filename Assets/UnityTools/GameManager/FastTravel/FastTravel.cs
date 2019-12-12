using UnityEngine;
using UnityEngine.SceneManagement;
using UnityTools.Spawning;

using System.Collections.Generic;
using UnityEditor;
using UnityTools.EditorTools;
using System.IO;
using UnityEditor.SceneManagement;

namespace UnityTools.FastTravelling {

    public static class FastTravel 
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void _InitFastTravel()
        {
            Debug.Log("FastTravel: Initializing Fast Travel...");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        static void FinishFastTravel (string scene){
            fastTravelling = true;
            SceneLoading.LoadSceneAsync(scene, null, null, LoadSceneMode.Single, false);
        }


        public static void FastTravelTo (string alias) {
            FastTravelTo(FastTravelSettings.GetFastTravelAlias(alias));    
        }

        public static void FastTravelTo (FastTravelLocation location) {

            if (location.useKey) {
                FastTravelTo(location.key);
                return;
            }
            FastTravelTo(location.location);
        }
        public static void FastTravelTo (SpawnLocation location) {
            if (location != null)
                FastTravelTo(location.scene, location.spawnName);
        }

        public static void FastTravelTo (string scene, string spawnName){
            SpawnPoint spawnPoint = SpawnPoint.GetSpawnPoint (scene, spawnName);
            if (spawnPoint == null)
                return;
            target = spawnPoint.GetTransform();
            FinishFastTravel(scene);
        }

        public static void FastTravelTo (string scene, Vector3 targetPosition){
            if (string.IsNullOrEmpty(scene)) {
                Debug.LogError("FastTravel: No Scene Specified");
                return;
            }
            target = new MiniTransform (targetPosition, GameManager.player.transform.rotation);
            FinishFastTravel(scene);
        }

        static bool fastTravelling;
        static MiniTransform target;
        

        static void OnSceneLoaded (Scene scene, LoadSceneMode mode) {
            if (fastTravelling) {
                if (GameManager.playerExists) {

                    SpawnOptions spawn = FastTravelSettings.instance.fastTravelSpawnOptions;

                    Transform player = GameManager.player.transform;
                    Vector3 up;
                    
                    target.position = GameManager.GroundPosition(target.position, spawn.ground, spawn.navigate, out up);
                            
                    player.WarpTo(target.position, Quaternion.Euler(target.rotation));
                        
                    if (spawn.uncollide)
                        GameManager.UnIntersectTransform (player, up);
                    
                }
                fastTravelling = false;
            }
        }
    }

    [System.Serializable] public class FastTravelLocation {
        public SpawnLocation location;
        public bool useKey;
        public string key;
    }

    [System.Serializable] public class FastTravelAliasArray : NeatArrayWrapper<FastTravelAlias> { }
    [System.Serializable] public class FastTravelAlias {
        public string key;
        public SpawnLocation location;
    }


    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FastTravelAlias))] class FastTravelAliasDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            
            EditorGUI.PropertyField(pos, prop.FindPropertyRelative("key"), true);
            SerializedProperty location = prop.FindPropertyRelative("location");
            EditorGUI.PropertyField(pos, location, true);
            if (GUITools.Button(pos.x, pos.y + GUITools.singleLineHeight * 2, pos.width, GUITools.singleLineHeight, new GUIContent("Fast Travel"), GUITools.button)) {
                string chosenScene = location.FindPropertyRelative("scene").stringValue;
                string chosenSpawn = location.FindPropertyRelative("spawnName").stringValue;
                FastTravelLocationEditor.FastTravelToSpawnLocation (chosenScene, chosenSpawn);
            }
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return GUITools.singleLineHeight * 4;
        }
        
    }

    
    [CustomPropertyDrawer(typeof(FastTravelLocation))] class FastTravelLocationDrawer : PropertyDrawer
    {

        float CalcHeight (SerializedProperty useKey) {
            return GUITools.singleLineHeight * (useKey.boolValue ? 3 : 4);
        }
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {


            SerializedProperty useKey = prop.FindPropertyRelative("useKey");
            GUITools.Box(new Rect(pos.x, pos.y, pos.width, CalcHeight(useKey)), new Color32(0, 0, 0, 32));

            pos.height = GUITools.singleLineHeight;
            GUITools.Label(pos, label, GUITools.black, GUITools.boldLabel);

            GUITools.DrawToggleButton(useKey, new GUIContent("K", "Use Key"), pos.x + (pos.width - GUITools.iconButtonWidth), pos.y );


            pos.y += GUITools.singleLineHeight;
                        
            if (useKey.boolValue) {
                SerializedProperty keyProp = prop.FindPropertyRelative("key");
                DrawKeySelect(pos, keyProp);
                GUI.enabled = !string.IsNullOrEmpty(keyProp.stringValue);
                if (GUITools.Button(pos.x, pos.y + GUITools.singleLineHeight, pos.width, GUITools.singleLineHeight, new GUIContent("Fast Travel"), GUITools.button)) {
                    SpawnLocation location = FastTravelSettings.GetFastTravelAlias(keyProp.stringValue);
                    if (location != null) {
                        FastTravelLocationEditor.FastTravelToSpawnLocation (location.scene, location.spawnName);
                    }
                }
                GUI.enabled = true;
            }
            else {
                SerializedProperty location = prop.FindPropertyRelative("location");
                EditorGUI.PropertyField(pos, location, true);
                if (GUITools.Button(pos.x, pos.y + GUITools.singleLineHeight * 2, pos.width, GUITools.singleLineHeight, new GUIContent("Fast Travel"), GUITools.button)) {
                    string chosenScene = location.FindPropertyRelative("scene").stringValue;
                    string chosenSpawn = location.FindPropertyRelative("spawnName").stringValue;
                    FastTravelLocationEditor.FastTravelToSpawnLocation (chosenScene, chosenSpawn);
                }
            }
            
        }
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return CalcHeight(prop.FindPropertyRelative("useKey"));
        }
    
    

        static void DrawKeySelect (Rect pos, SerializedProperty prop) {
            
            List<string> allKeys = FastTravelSettings.GetKeys();

            if (!allKeys.Contains(prop.stringValue)) {
                prop.stringValue = string.Empty;
            }
            
            EditorGUI.LabelField(new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth, pos.height), new GUIContent(prop.displayName));
            if (GUITools.Button(pos.x + EditorGUIUtility.labelWidth, pos.y, pos.width - (EditorGUIUtility.labelWidth), pos.height, new GUIContent(!string.IsNullOrEmpty(prop.stringValue) ? prop.stringValue : "[ Null ]"), GUITools.popup)) {
                
                GenericMenu menu = new GenericMenu();
                
                for (int i = 0; i < allKeys.Count; i++) {
                    string e = allKeys[i];
                    menu.AddItem (new GUIContent(e), e == prop.stringValue, 
                        () => {
                            prop.stringValue = e;
                            prop.serializedObject.ApplyModifiedProperties();
                        }
                    );
                }
                menu.ShowAsContext();  
            }
        }
        
    }






















    public class FastTravelLocationEditor {

        static readonly string[] overrideProps = new string[] {
            "location.scene",
            "location.spawnName",
            "key",
            "useKey"
        };

        public static void AdjustLocationValues (SerializedProperty prop) {
            for (int i = 0; i < 3; i++) {
                SerializedProperty p = prop.FindPropertyRelative(overrideProps[i]);
                p.stringValue = p.stringValue + "!";
            }
            prop.FindPropertyRelative("useKey").boolValue = !prop.FindPropertyRelative("useKey").boolValue;
            prop.serializedObject.ApplyModifiedPropertiesWithoutUndo();       
        }

        public static void RestoreLocationValues (SerializedProperty prop) {
            for (int i = 0; i < 3; i++) {
                SerializedProperty p = prop.FindPropertyRelative(overrideProps[i]);
                p.stringValue = p.stringValue.Remove(p.stringValue.Length - 1);
            }
            prop.FindPropertyRelative("useKey").boolValue = !prop.FindPropertyRelative("useKey").boolValue;
            prop.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        public static bool LocationIsPrefabOverride (SerializedProperty prop) {
            for (int i = 0; i < overrideProps.Length; i++){
                if (!prop.FindPropertyRelative(overrideProps[i]).prefabOverride)
                    return false; 
            }
            return true;
        }


        // public static void FastTravelToSpawnLocation (SerializedProperty location) {
        public static void FastTravelToSpawnLocation (string chosenScene, string chosenSpawn) {
        
                        
            if (Application.isPlaying) {
                FastTravel.FastTravelTo (chosenScene, chosenSpawn);
            }
            else {
                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                string scenePath = null;
                for (int i = 0; i < scenes.Length; i++) {
                    if (Path.GetFileNameWithoutExtension(scenes[i].path) == chosenScene) {
                        scenePath = scenes[i].path;
                    }
                }
                if (scenePath != null) {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                        SpawnPoint chosenSpawnPoint = SpawnPoint.GetSpawnPoint(chosenScene, chosenSpawn);

                        if (chosenSpawnPoint != null)
                            SceneView.lastActiveSceneView.Frame(chosenSpawnPoint.GetBounds(), false);
                        else 
                            Debug.LogWarning("No Spawn Found: " + chosenSpawn);
                    }
                }
                else 
                    Debug.LogWarning("No Scene Found: " + chosenScene);
            }
        }
    }


#endif

        
}
