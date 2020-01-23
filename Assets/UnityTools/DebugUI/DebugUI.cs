using UnityEngine;
using System.Collections.Generic;
using System;
namespace UnityTools {
    public class DebugUI : InitializationSingleTon<DebugUI>, IUIHandler
    {
        bool isUsed;
        void Start () {

            if (!UIEvents.isInitialized) {
                UIEvents.InitializeEvents(this);
                isUsed = true;
                Debug.Log("Using backup debug ui for main ui events... consider using a custom one as this is not performance friendly or complete!");
            }
        }
             
        static Texture2D GetTexture (Color color) {
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, color);
            t.Apply();
            return t;
        }
        
        static Texture2D _darkTexture;
        static Texture2D darkTexture {
            get {
                if (_darkTexture == null) 
                    _darkTexture = GetTexture(new Color(0f, 0f, 0f, 0.8f));
                return _darkTexture;
            }
        }
        static Texture2D _whiteTexture;
        static Texture2D whiteTexture {
            get {
                if (_whiteTexture == null) 
                    _whiteTexture = GetTexture(Color.white);
                return _whiteTexture;
            }
        }
        
        static GUIStyle CreateStyle()
        {
            GUIStyle s = new GUIStyle {
                richText = true,
                font = Resources.Load<Font>("Hack-Regular")
            };
            // s.normal.textColor = Color.white;
            // s.normal.background = darkTexture;
            return s;
        }

        static GUIStyle _debugStyle;
        static GUIStyle debugStyle {
            get {
                if (_debugStyle == null)
                    _debugStyle = CreateStyle();
                return _debugStyle;
            }
        }

        public static GUIStyle DebugStyle (TextAnchor textAnchor = TextAnchor.UpperLeft, int fontSize = 12, bool dark = true, bool useHover = false) {
            GUIStyle style = debugStyle;
            style.alignment = textAnchor;
            style.fontSize = fontSize;
            
            style.normal.textColor = dark ? Color.white : Color.black;
            style.normal.background = dark ? darkTexture : whiteTexture;
            
            // bool darkNess = useHover ? !dark : dark;
            style.hover.textColor = !useHover ? Color.white : Color.black;
            style.hover.background = !useHover ? darkTexture : whiteTexture;
            return style;
        }


        Queue<string> messagesQ = new Queue<string>();
        List<string> shownMsgs = new List<string>();

        void DrawMessages () {
            GUIContent g = new GUIContent(string.Join("\n", shownMsgs));
            GUIStyle style = DebugStyle(TextAnchor.MiddleRight);
            Vector2 size = style.CalcSize(g);
            GUI.Box(new Rect(Screen.width - size.x, Screen.height * .5f, size.x, size.y), g, style);
        }

        const float msgTime = 5;
        const int maxShownMessages = 5;

        float msgTimer;
        void UpdateMessages () {
            if (shownMsgs.Count > 0)
            {
                msgTimer += Time.unscaledDeltaTime;
                if (msgTimer > msgTime) {
                    shownMsgs.RemoveAt(0);
                    msgTimer = 0;
                    if (messagesQ.Count > 0) {
                        shownMsgs.Add(messagesQ.Dequeue());
                    }
                }
            }
        }

        void Update () {
            if (!isUsed)
                return;
            UpdateMessages();
        }

        void OnGUI()
        {
            if (!isUsed)
                return;
            
            if (shownMsgs.Count > 0)
                DrawMessages();

            if (!string.IsNullOrEmpty(showPrompt))
                DrawPrompt ();

            if (selectionOpen)
                DrawSelectionPopup();
        }
         
        void DrawPrompt () {
            GUIContent g = new GUIContent(showPrompt);
            GUIStyle style = DebugStyle(TextAnchor.MiddleCenter, 14);
            Vector2 size = style.CalcSize(g);
            GUI.Box(new Rect(Screen.width * .5f - size.x * .5f, Screen.height * .5f, size.x, size.y), g, style);
        }

        string showPrompt;

        public void ShowMessage (int messageIndex, string message, bool immediate, UIColorScheme scheme, bool bulleted) {
            if (shownMsgs.Count >= maxShownMessages) 
                messagesQ.Enqueue(message);
            else 
                shownMsgs.Add(message);
        }
        public void ShowSubtitles (string speaker, string bark, float fadeIn, float duration, float fadeOut) {

        }

        public void ShowActionPrompt (int promptIndex, string msg, List<int> actions, List<string> hints, float fadeIn = .1f) {
            string prompt = msg + " [";
            for (int i = 0; i < actions.Count; i++) {
                prompt += ActionsInterface.Action2String(actions[i]) + " : " + hints[i] + (i == actions.Count - 1 ? "]" : ", ");
            }
            showPrompt = prompt;
        }

        public void HideActionPrompt (int promptIndex, float fadeOut = .1f) {
            showPrompt = null;
        }
        public void ShowSelectionPopup(string msg, string[] options, Action<bool, int> returnValue) {
            popupMsg = msg;
            buttonsOptions = options;
            onSelection = returnValue;
        }

        Action onConfirmation;
        void OnConfirmation (bool used, int optionChosen) {
            if (optionChosen == 0) {
                if (onConfirmation != null)
                    onConfirmation();
                onConfirmation = null;
            }
        }

        public void ShowConfirmationPopup(string msg, Action onConfirmation) {
            this.onConfirmation = onConfirmation;
            ShowSelectionPopup(msg, new string[] { "OK", "Cancel" }, OnConfirmation);
        }

        public void ShowIntSliderPopup(string title, int initialValue, int minValue, int maxValue, Action<bool, int> returnValue, Action<int> onValueChange) {
            Debug.LogError("ShowIntSliderPopup not available in " + GetType().Name);
        }


        bool selectionOpen { get { return onSelection != null; } }
        string[] buttonsOptions;
        string popupMsg;
        const float buttonHeight = 32;
        Action<bool, int> onSelection;
        void DrawSelectionPopup () {

            float boxWidth = Screen.width * .5f;
            
            float buttonsHeight = buttonHeight * buttonsOptions.Length;
            GUIContent popupMsgGUI = new GUIContent(popupMsg);

            GUIStyle style = DebugStyle(TextAnchor.UpperCenter, 16, true); 

            float msgHeight = style.CalcHeight(popupMsgGUI, boxWidth);

            float boxHeight = msgHeight + buttonsHeight;
            Rect boxRect = new Rect(Screen.width * .25f, Screen.height * .5f - (boxHeight * .5f), boxWidth, boxHeight);
            GUI.Box(boxRect, popupMsgGUI, style);
                
            Rect buttonRect = new Rect(Screen.width * .25f, Screen.height * .5f - (boxHeight * .5f) + msgHeight, boxWidth, buttonHeight);
            
            GUIStyle buttonStyle = DebugStyle(TextAnchor.MiddleCenter, 14, true, true);
            for (int i = 0; i < buttonsOptions.Length; i++) {
                if (GUI.Button(buttonRect, buttonsOptions[i], buttonStyle)) {
                    onSelection(true, i);
                    onSelection = null;
                }
                buttonRect.y += buttonHeight;
            }
        }        
    }
}
