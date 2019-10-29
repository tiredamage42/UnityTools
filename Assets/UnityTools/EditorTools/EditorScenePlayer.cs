using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
 
namespace UnityTools.EditorTools {
    public static class EditorScenePlayer
    {
        static string getMasterScene { get { return EditorBuildSettings.scenes[0].path; } }
        public static bool overrideLoadMasterOnPlay;
    
        public static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!LoadMasterOnPlay)
                return;
            
            if (state == PlayModeStateChange.EnteredEditMode) {
                if (overrideLoadMasterOnPlay) {
                    overrideLoadMasterOnPlay = false;
                    return;
                }
                // User pressed stop -- reload previous scene.
                try {
                    EditorSceneManager.OpenScene(PreviousScene);
                }
                catch {
                    Debug.LogError(string.Format("error: scene not found: {0}", PreviousScene));
                }
            }
            else if (state == PlayModeStateChange.ExitingEditMode) {
                if (overrideLoadMasterOnPlay)
                    return;
                
                // User pressed play -- autoload master scene.
                PreviousScene = EditorSceneManager.GetActiveScene().path;
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    try {
                        EditorSceneManager.OpenScene(getMasterScene);
                    }
                    catch {
                        Debug.LogError(string.Format("error: scene not found: {0}", getMasterScene));
                        EditorApplication.isPlaying = false;
                    }
                }
                else {
                    // User cancelled the save operation -- cancel play as well.
                    EditorApplication.isPlaying = false;
                }
            }
        }
    
        // Properties are remembered as editor preferences.
        private const string cEditorPrefLoadMasterOnPlay = "SceneAutoLoader.LoadMasterOnPlay";
        private const string cEditorPrefPreviousScene = "SceneAutoLoader.PreviousScene";
    
        public static bool LoadMasterOnPlay
        {
            get { return EditorPrefs.GetBool(cEditorPrefLoadMasterOnPlay, false); }
            set { EditorPrefs.SetBool(cEditorPrefLoadMasterOnPlay, value); }
        }
        private static string PreviousScene
        {
            get { return EditorPrefs.GetString(cEditorPrefPreviousScene, EditorSceneManager.GetActiveScene().path); }
            set { EditorPrefs.SetString(cEditorPrefPreviousScene, value); }
        }
    }

}