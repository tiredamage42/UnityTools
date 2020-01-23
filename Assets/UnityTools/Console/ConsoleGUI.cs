using UnityEngine;

namespace UnityTools.DevConsole {

    public static class ConsoleGUI 
    {
        // public static Texture2D GetTexture (Color color) {
        //     Texture2D t = new Texture2D(1, 1);
        //     t.SetPixel(0, 0, color);
        //     t.Apply();
        //     return t;
        // }
        
        // static Texture2D _pixelTexture;
        // static Texture2D pixelTexture {
        //     get {
        //         if (_pixelTexture == null) 
        //             _pixelTexture = GetTexture(new Color(0f, 0f, 0f, 0.8f));
        //         return _pixelTexture;
        //     }
        // }
        
        // static GUIStyle CreateStyle()
        // {
        //     GUIStyle s = new GUIStyle {
        //         richText = true,
        //         font = ConsoleSettings.instance.consoleFont,
        //     };
        //     s.normal.textColor = Color.white;
        //     s.normal.background = pixelTexture;
        //     return s;
        // }

        // static GUIStyle _debugStyle;
        // static GUIStyle debugStyle {
        //     get {
        //         if (_debugStyle == null)
        //             _debugStyle = CreateStyle();
        //         return _debugStyle;
        //     }
        // }

        // public static GUIStyle DebugStyle (TextAnchor textAnchor = TextAnchor.UpperLeft, int fontSize = -1) {
        //     GUIStyle style = debugStyle;
        //     style.alignment = textAnchor;
        //     style.fontSize = fontSize == -1 ? ConsoleSettings.instance.fontSize : fontSize;
        //     return style;
        // }
    }
}
