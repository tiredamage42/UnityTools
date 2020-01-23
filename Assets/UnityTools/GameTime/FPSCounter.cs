using UnityEngine;

using UnityTools.DevConsole;
namespace UnityTools {


    public class FPSCounter {
        const float fpsMeasurePeriod = 0.5f;
        float deltaTime, m_FpsNextPeriod;
        int m_CurrentFps, m_FpsAccumulator;

        bool initGUI;
        Vector2 size;
        void InitGUI () {

            if (!initGUI) {
                gui = new GUIContent(TimeString(100.0f, 1000));
                size = DebugUI.DebugStyle(TextAnchor.MiddleLeft, 14).CalcSize(gui);
                initGUI = true;
            }
        }
        // const string tString = "{0:0.0} ms ({1} fps)";

        const string tString0 = " ms (";
        const string tString1 = " fps)";
        
        string TimeString (float deltaTime, int FPS) {
            // return ((int)(deltaTime * 10)) * .1f + tString0 + FPS + tString1;// string.Format(tString, deltaTime, FPS);
            return (int)deltaTime + tString0 + FPS + tString1;// string.Format(tString, deltaTime, FPS);
        }
        GUIStyle style { get { return DebugUI.DebugStyle(TextAnchor.MiddleLeft, 14); } }
        GUIContent gui;
        public void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            // measure average frames per second
            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod) {
                m_CurrentFps = (int)(m_FpsAccumulator/fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;
            }
        }
        public void OnGUI()
        {
            InitGUI();
            gui.text = TimeString(deltaTime * 1000f, m_CurrentFps);
            GUI.Box(new Rect(Screen.width - size.x, 0, size.x, size.y), gui, style);
        }
    }
    // public class FPSCounter : InitializationSingleTon<FPSCounter>
    // {
    //     const float fpsMeasurePeriod = 0.5f;
    //     float deltaTime, m_FpsNextPeriod;
    //     int m_CurrentFps, m_FpsAccumulator;

        
    //     [Command("showfps")]
    //     static bool showFPS = true;
        
    //     void ShowFPS()
    //     {
    //         GUIContent g = new GUIContent(string.Format("{0:0.0} ms ({1} fps)", deltaTime * 1000f, m_CurrentFps));
    //         GUIStyle style = ConsoleGUI.DebugStyle(TextAnchor.MiddleLeft);
    //         Vector2 s = style.CalcSize(g);
    //         GUI.Box(new Rect(Screen.width - s.x, 0, s.x, s.y), g, style);
    //     }

    //     void OnGUI()
    //     {
    //         if (showFPS)
    //             ShowFPS();
    //     }
            
    //     void Update()
    //     {
    //         if (!showFPS)
    //             return;

    //         deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    //         // measure average frames per second
    //         m_FpsAccumulator++;
    //         if (Time.realtimeSinceStartup > m_FpsNextPeriod) {
    //             m_CurrentFps = (int)(m_FpsAccumulator/fpsMeasurePeriod);
    //             m_FpsAccumulator = 0;
    //             m_FpsNextPeriod += fpsMeasurePeriod;
    //         }
    //     }
    // }
}
