using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
 
namespace UnityTools.EditorTools {

    /*
        lets us load a main menu (or initialization) scene to keep our workflow "game-like"

        we can skip to a specified scene specified in the scene build window
    */
    public static class InitialSceneWorkflow
    {
        static string initialScene { get { return EditorBuildSettings.scenes[0].path; } }
        public static bool disableInitialSceneWorkflow;
    
        public static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!LoadInitialOnPlay)
                return;
            
            if (state == PlayModeStateChange.EnteredEditMode) {
                if (disableInitialSceneWorkflow) {
                    disableInitialSceneWorkflow = false;
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
                if (disableInitialSceneWorkflow)
                    return;
                
                // User pressed play -- autoload initials scene.
                PreviousScene = EditorSceneManager.GetActiveScene().path;
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    try {
                        EditorSceneManager.OpenScene(initialScene);
                    }
                    catch {
                        Debug.LogError(string.Format("error: scene not found: {0}", initialScene));
                        EditorApplication.isPlaying = false;
                    }
                }
                else {
                    // User cancelled the save operation -- cancel play as well.
                    EditorApplication.isPlaying = false;
                }
            }
        }
    
        const string cEditorPrefLoadInitialOnPlay = "InitialSceneWorkflow.LoadInitialOnPlay";
        const string cEditorPrefPreviousScene = "InitialSceneWorkflow.PreviousScene";
        const string cEditorPrefSkipToScene = "InitialSceneWorkflow.SkipToScene";
        
        public static string SkipToScene
        {
            get { return EditorPrefs.GetString(cEditorPrefSkipToScene, null); }
            set { EditorPrefs.SetString(cEditorPrefSkipToScene, value); }
        }
        public static bool LoadInitialOnPlay
        {
            get { return EditorPrefs.GetBool(cEditorPrefLoadInitialOnPlay, false); }
            set { EditorPrefs.SetBool(cEditorPrefLoadInitialOnPlay, value); }
        }
        static string PreviousScene
        {
            get { return EditorPrefs.GetString(cEditorPrefPreviousScene, EditorSceneManager.GetActiveScene().path); }
            set { EditorPrefs.SetString(cEditorPrefPreviousScene, value); }
        }
    }

}