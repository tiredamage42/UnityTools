// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
namespace UnityTools {
    public enum UIColorScheme { Normal = 0, Warning = 1, Invalid = 2, Black = 3, White = 4, };
    public interface IUIHandler {
        void ShowMessage (int messageIndex, string message, bool immediate, UIColorScheme scheme, bool bulleted);
        void ShowSubtitles (string speaker, string bark, float fadeIn, float duration, float fadeOut);
        void ShowActionPrompt (int promptIndex, string msg, List<int> actions, List<string> hints, float fadeIn = .1f);
        void HideActionPrompt (int promptIndex, float fadeOut = .1f);
        void ShowSelectionPopup(string msg, string[] options, Action<bool, int> returnValue);
        void ShowConfirmationPopup(string msg, Action onConfirmation);
        void ShowIntSliderPopup(string title, int initialValue, int minValue, int maxValue, Action<bool, int> returnValue, Action<int> onValueChange);
    }

    public class UIEvents 
    {
        public static IUIHandler handler;
        public static bool isInitialized { get { return handler != null; } }

        public static void InitializeEvents (IUIHandler handler) {
            UIEvents.handler = handler;
        }
            
        static bool CheckInitialized () {
            if (!isInitialized) {
                Debug.LogError("UIEvents Not Initialized With a UIHandler");
                return false;
            }
            return true;
        }
        public static void ShowMessage (int messageIndex, string message, bool immediate, UIColorScheme scheme, bool bulleted) {
            if (!CheckInitialized())
                return;
            handler.ShowMessage ( messageIndex, message, immediate, scheme, bulleted);
        }
        public static void ShowSubtitles (string speaker, string bark, float fadeIn, float duration, float fadeOut) {
            if (!CheckInitialized())
                return;
            handler.ShowSubtitles (speaker, bark, fadeIn, duration, fadeOut);
        }
        public static void ShowActionPrompt (int promptIndex, string msg, List<int> actions, List<string> hints, float fadeIn = .1f) {
            if (!CheckInitialized())
                return;
            handler.ShowActionPrompt ( promptIndex, msg, actions, hints, fadeIn );
        }
        public static void HideActionPrompt (int promptIndex, float fadeOut = .1f) {
            if (!CheckInitialized())
                return;
            handler.HideActionPrompt ( promptIndex, fadeOut );
        }
        public static void ShowSelectionPopup(string msg, string[] options, Action<bool, int> returnValue) {
            if (!CheckInitialized())
                return;
            handler.ShowSelectionPopup( msg, options, returnValue);
        }
        public static void ShowConfirmationPopup(string msg, Action onConfirmation) {
            if (!CheckInitialized())
                return;
            handler.ShowConfirmationPopup( msg, onConfirmation);
        }
        public static void ShowIntSliderPopup(string title, int initialValue, int minValue, int maxValue, Action<bool, int> returnValue, Action<int> onValueChange) {
            if (!CheckInitialized())
                return;   
            handler.ShowIntSliderPopup( title, initialValue, minValue, maxValue, returnValue, onValueChange);
        }
    }
}
