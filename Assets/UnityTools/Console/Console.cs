using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UnityTools.DevConsole {
    public class Console : InitializationSingleTon<Console>
    {
        [Command("systeminfo", "Prints system information.", "Console", false)]
        public static string GetSystemInfo()
        {
            int padding = 23;
            StringBuilder text = new StringBuilder();
            text.AppendLine("<b>Device</b>");
            text.AppendLine("\t<b>OS</b>".PadRight(padding) + SystemInfo.operatingSystem + "(" + SystemInfo.operatingSystemFamily + ")");
            text.AppendLine("\t<b>RAM</b>".PadRight(padding) + (SystemInfo.systemMemorySize / 1024f) + " GiB");
            text.AppendLine("\t<b>Name</b>".PadRight(padding) + SystemInfo.deviceName);
            text.AppendLine("\t<b>Model</b>".PadRight(padding) + SystemInfo.deviceModel);
            text.AppendLine("\t<b>Type</b>".PadRight(padding) + SystemInfo.deviceType);
            text.AppendLine("\t<b>Unique ID</b>".PadRight(padding) + SystemInfo.deviceUniqueIdentifier);

            text.AppendLine("<b>CPU</b>");
            text.AppendLine("\t<b>Name</b>".PadRight(padding) + SystemInfo.processorType);
            text.AppendLine("\t<b>Processors</b>".PadRight(padding) + SystemInfo.processorCount);
            text.AppendLine("\t<b>Frequency</b>".PadRight(padding) + SystemInfo.processorFrequency);

            text.AppendLine("<b>GPU</b>");
            text.AppendLine("\t<b>Type</b>".PadRight(padding) + SystemInfo.graphicsDeviceType);
            text.AppendLine("\t<b>Name</b>".PadRight(padding) + SystemInfo.graphicsDeviceName);
            text.AppendLine("\t<b>Vendor</b>".PadRight(padding) + SystemInfo.graphicsDeviceVendor);
            text.AppendLine("\t<b>Memory</b>".PadRight(padding) + (SystemInfo.graphicsMemorySize / 1024f) + " GiB");
            return text.ToString();
        }
        [Command("appinfo", "Prints application information.", "Console", false)]
        public static string GetAppInfo()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("<b>Data</b>");
            text.AppendLine(Application.dataPath);
            text.AppendLine("<b>Streaming Assets</b>");
            text.AppendLine(Application.streamingAssetsPath);
            text.AppendLine("<b>Persistent Data</b>");
            text.AppendLine(Application.persistentDataPath);
            text.AppendLine("<b>Console Log</b>");
            text.AppendLine(Application.consoleLogPath);
            return text.ToString();
        }

        [Command("clear", "Clears the console window.", "Console", false)]
        public static void ClearConsole()
        {
            instance.Clear();
            Debug.ClearDeveloperConsole();
        }

        [Command("help", "Outputs a list of all commands", "Console", false)]
        public static string Help()
        {
            bool isInMainMenuScene = GameManager.isInMainMenuScene;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("All commands registered:");
            builder.AppendLine("Instance Method Legend:");
            builder.AppendLine("\t[\t! : Search Name,\t@ : Dynamic Object Alias,\t# : Dynamic Object ID\t]");
            
            foreach (var category in Command.commandsLibrary.Keys) {
                builder.AppendLine("\t" + category);
                foreach (var command in Command.commandsLibrary[category])
                    if (!command.inGameOnly || !isInMainMenuScene)
                        builder.AppendLine("\t\t" + command.GetHint());
            }
            return builder.ToString();
        }

        [Command("echo", "", "Console", false)]
        public static string Echo(string text)
        {
            return text;
        }

        const int HistorySize = 400;
        const string ConsoleControlName = "ControlField";
        const string PrintColor = "white";
        const string WarningColor = "orange";
        const string ErrorColor = "red";
        const string UserColor = "lime";

        protected override void Awake()
        {
            base.Awake();
            if (!thisInstanceErrored) {
                //adds a listener for debug prints in editor
                Application.logMessageReceived += HandleLog;
                Run("echo \"console initialized\"");
            }
        }

        int Scroll
        {
            get
            {
                return scroll;
            }
            set
            {
                if (value < 0) value = 0;

                int h = Mathf.Min(Mathf.Max(1, (text.Count - maxLinesOnScreen) + 2), HistorySize);
                if (value >= h) value = h - 1;
                scroll = value;
            }
        }
        int maxLinesOnScreen
        {
            get
            {
                int lines = Mathf.RoundToInt(Screen.height * 0.45f / 16);
                return Mathf.Clamp(lines, 4, 32);
            }
        }


        string input, linesString, originalTypedText;
        bool open, moveToEnd, typedSomething;
        int scroll, lastMaxLines, searchIndex;
        List<string> text = new List<string>(), searchResults = new List<string>(), inputsHistory = new List<string>();
        
        void HandleLog(string message, string stack, LogType logType)
        {
            WriteLine(message, logType);
        }

        static string GetStringFromObject(object message)
        {
            if (message == null)
                return null;
            if (message is string)
                return message as string;
            return message.ToString();
        }

        void Print(object message) {
            Add(GetStringFromObject(message), PrintColor);
        }
        void Warn(object message) {
            Add(GetStringFromObject(message), WarningColor);
        }
        void Error(object message) {
            Add(GetStringFromObject(message), ErrorColor);
        }

        void WriteLine(object text, LogType type = LogType.Log)
        {
            if (type == LogType.Log)
                Print(text);
            else if (type == LogType.Warning)
                Warn(text);
            else if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
                Error(text);
        }

        async void Run(string command)
        {
            //run
            object result = await Parser.Run(command);
            if (result == null)
                return;
            
            if (result is Exception exception)
            {
                Exception inner = exception.InnerException;
                if (inner != null)
                    Error(exception.Message + "\n" + exception.Source + "\n" + inner.Message);
                else
                    Error(exception.Message);
            }
            else
                Print(result.ToString());
        }

        //adds text to console text
        void Add(string input, string color)
        {
            if (input == null)
                return;

            List<string> lines = new List<string>();
            
            string str = input.ToString();

            if (str.Contains("\n"))
                lines.AddRange(str.Split('\n'));
            else
                lines.Add(str);
            
            for (int i = 0; i < lines.Count; i++)
            {
                text.Add("<color=" + color + ">" + lines[i] + "</color>");
                if (text.Count > HistorySize)
                {
                    text.RemoveAt(0);
                }
            }

            int newScroll = (text.Count - maxLinesOnScreen) + 1;
            if (newScroll > Scroll)
            {
                //set scroll to bottom
                Scroll = newScroll;
            }

            //update the lines string
            UpdateText();
        }

        //creates a single text to use when display the console

        void UpdateText()
        {
            string[] lines = new string[maxLinesOnScreen];
            int lineIndex = 0;
            for (int i = 0; i < text.Count; i++)
            {
                int index = i + Scroll;
                if (index < 0) continue;
                else if (index >= text.Count) continue;
                else if (string.IsNullOrEmpty(text[index])) break;
                
                lines[lineIndex] = (text[index]);

                //replace all \t with 4 spaces
                lines[lineIndex] = lines[lineIndex].Replace("\t", "    ");

                lineIndex++;
                if (lineIndex == maxLinesOnScreen) 
                    break;
            }
            linesString = string.Join("\n", lines);
        }

        void HandleClickObjects () {
            if (Input.GetMouseButtonDown(0)) {
                if (GameManager.playerExists) {
                    Ray ray = GameManager.playerCamera.ScreenPointToRay(Input.mousePosition);
                    
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, ConsoleSettings.instance.clickMask, QueryTriggerInteraction.Ignore)) {
                    
                        GameObject hitObject = hit.transform.gameObject;
                        DynamicObject dynamicObject = hitObject.GetComponentInParent<DynamicObject>();

                        if (dynamicObject != null) {
                            string id = dynamicObject.GetID();
                            string msg = "Clicked Object: '" + hitObject.name + "', Dynamic Object ID: " + id;
                            if (DynamicObjectManager.IDHasAlias(id, out string alias)) 
                                msg += ", Alias: '" + alias + "'";
                            
                            Warn(msg);
                            GUIUtility.systemCopyBuffer = id;
                        }
                        else {
                            Warn("Clicked Object: '" + hitObject.name + "'");
                            GUIUtility.systemCopyBuffer = hitObject.name;
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (!open)
                return;

            //max lines amount changed
            if (lastMaxLines != maxLinesOnScreen) {
                lastMaxLines = maxLinesOnScreen;
                UpdateText();
            }
            HandleClickObjects();            
        }

        static bool IsConsoleKey(KeyCode key) {
            return key == KeyCode.BackQuote || key == KeyCode.Tilde;
        }
        static bool IsConsoleChar(char character) {
            return character == '`' || character == '~' || character == '§';
        }

        //search through all commands
        void SearchForHints(string text)
        {
            searchResults.Clear();

            if (string.IsNullOrEmpty(text))
                return;
            
            if (Parser.StartsWithInstanceKey(text)) {
                if (text.Contains(" ")) 
                    text = text.Replace(text.Substring(0, text.IndexOf(' ')) + " ", "");
                else 
                    text = "";
            }
                
            if (string.IsNullOrEmpty(text))
                return;

            foreach (var command in Command.allCommands) 
                if (command.Name.StartsWith(text) || text.StartsWith(command.Name))
                    searchResults.Add(command.GetHint());
        }

        

        


        void SetInput () {
            if (typedSomething) 
                SetInputAsHint(searchResults);
            else 
                SetInputAsHistory (inputsHistory);
        }

        void SetInputAsHistory (List<string> historyOrResults) {
            if (searchIndex >= 0 && searchIndex < historyOrResults.Count)
            {
                input = historyOrResults[searchIndex];
                moveToEnd = true;
            }
        }
        void SetInputAsHint (List<string> historyOrResults) {
            if (searchIndex >= 0 && searchIndex < historyOrResults.Count)
            {
                string typedID = "";
                if (Parser.StartsWithInstanceKey(input)) {
                    typedID = input.Substring(0, input.IndexOf(' ') + 1);
                }
                string targ = historyOrResults[searchIndex].Replace("[I] ", "");
                input = typedID + targ.Substring(0, targ.IndexOf(' '));
                moveToEnd = true;
            }
        }
        
        void ResetInputAndSearchIndex (int newSearchIndex) {
            searchIndex = newSearchIndex;
            input = "";
            moveToEnd = true;
        }

        void OnEnterPressed () {
            if (!string.IsNullOrEmpty(input)) {
                Add(input, UserColor);
                inputsHistory.Add(input);
                searchIndex = inputsHistory.Count;
                searchResults.Clear();
                Run(input);
                ClearInput();
                Event.current.Use();
                typedSomething = false;
            }
        }

        void ClearInput () {
            input = "";
            originalTypedText = "";    
        }

        void Clear()
        {
            Scroll = 0;
            ClearInput();
            text.Clear();
            inputsHistory.Clear();
            UpdateText();
        }

        
        void HandleScrolling () {
            int scrollDirection = (int)Mathf.Sign(Event.current.delta.y) * 3;
            Scroll += scrollDirection;
            UpdateText();
        }

        void HandleAutoCompleteAndHistoryRepeat () {
            if (Event.current.keyCode == KeyCode.UpArrow)
            {
                if (searchIndex < 0) 
                    originalTypedText = input;
                
                searchIndex--;
                if (searchIndex <= -1) {
                    ResetInputAndSearchIndex(-1);
                    input = originalTypedText;
                }
                else {
                    SetInput();
                }
            }
            else if (Event.current.keyCode == KeyCode.DownArrow)
            {
                searchIndex++;
                if (searchIndex == 0) 
                    originalTypedText = input;
                
                if (searchIndex >= (typedSomething ? searchResults : inputsHistory).Count)
                    searchIndex = 0;

                SetInput();
            }
        }

        void ToggleConsoleOpen () {
            ClearInput();
            open = !open;

            if (open)
            {
                typedSomething = false;
                searchIndex = inputsHistory.Count;                    
                searchResults.Clear();
                GameManager.PauseGame(this);
            }
            else {
                GameManager.UnpauseGame(this);
            }
        }

        void MoveToEnd()
        {
            TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (te != null)
                te.MoveCursorToPosition(new Vector2(int.MaxValue, int.MaxValue));
        }

        GUIContent gUIContent;

        void OnGUI()
        {
            if (gUIContent == null)
                gUIContent = new GUIContent("");

            moveToEnd = false;

            if (Event.current.type == EventType.KeyDown)
            {
                if (IsConsoleKey(Event.current.keyCode))
                {
                    ToggleConsoleOpen();
                    Event.current.Use();
                }

                if (IsConsoleChar(Event.current.character))
                    return;
            }

            //dont show the console if it shouldnt be open
            if (!open)
                return;
            
            //view scrolling
            if (Event.current.type == EventType.ScrollWheel)
                HandleScrolling();
            
            //history scolling
            if (Event.current.type == EventType.KeyDown)
                HandleAutoCompleteAndHistoryRepeat();
            
            bool pasted = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.V && Event.current.modifiers == EventModifiers.Control;
            
            //draw elements
            
            GUI.depth = -5;
            

            gUIContent.text = linesString;
            GUILayout.Box(gUIContent, DebugUI.DebugStyle(TextAnchor.UpperLeft, ConsoleSettings.instance.fontSize), GUILayout.Width(Screen.width));
            
            Rect lastControl = GUILayoutUtility.GetLastRect();


            Rect r = new Rect(0, lastControl.y + lastControl.height, Screen.width, 2);

            
            //draw the typing field
            GUI.Box(r, GUIContent.none, DebugUI.DebugStyle(TextAnchor.MiddleCenter, ConsoleSettings.instance.fontSize, false));

            GUI.SetNextControlName(ConsoleControlName);

            int lineHeight = ConsoleSettings.instance.fontSize + 4;

            r.y += r.height;
            r.height = lineHeight * 1.25f;
            
            string typedText = GUI.TextField(r, input, DebugUI.DebugStyle(TextAnchor.MiddleLeft, ConsoleSettings.instance.fontSize));
            
            GUI.FocusControl(ConsoleControlName);

            if (pasted) {
                typedText += GUIUtility.systemCopyBuffer;
                moveToEnd = true;
                pasted = false;
            }

            if (moveToEnd)
                MoveToEnd();

            //text changed, search
            if (input != typedText)
            {
                if (!typedSomething)
                {
                    typedSomething = true;
                    searchIndex = -1;
                }

                if (typedSomething && string.IsNullOrEmpty(typedText))
                {
                    typedSomething = false;
                    searchIndex = inputsHistory.Count;
                    originalTypedText = "";
                }

                input = typedText;
                SearchForHints(input);
            }


            r.y += r.height;
            r.height = searchResults.Count * lineHeight;

            //display the search box
            gUIContent.text = string.Join("\n", searchResults);
            GUI.Box(r, gUIContent, DebugUI.DebugStyle(TextAnchor.MiddleLeft, ConsoleSettings.instance.fontSize));
            
            //pressing enter to run command
            if (Event.current.type == EventType.KeyDown && (Event.current.character == '\n' || Event.current.character == '\r'))
                OnEnterPressed();

        }
    }
}
