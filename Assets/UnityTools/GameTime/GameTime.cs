
using UnityEngine;
using UnityTools.Internal;
using System;
using System.Collections.Generic;
using UnityTools.DevConsole;
namespace UnityTools {

    public class GameTime : InitializationSingleTon<GameTime>
    {
        protected override void Awake()
        {
            base.Awake();
            ResetDilationSystem();
            UpdateEngineScales();
            ResetTimeScale();
            ResetTimeOfDayAndDate();
            GameManager.onLoadMainMenu += OnLoadMainMenu;
            GameManager.onNewGameStart += OnNewGameStart;
            GameState.onGameLoaded += OnGameLoaded;
            GameState.onSaveGame += OnSaveGame;
            GameState.onSettingsLoaded += OnSettingsLoaded;
            GameState.onSettingsSave += OnSettingsSave;
        }

        static void OnLoadMainMenu () {
            ResetDilationSystem();
        }
        static void OnNewGameStart () {
            ResetDilationSystem();
            ResetTimeOfDayAndDate();
        }

        const string timeScaleKey = "timeScale";
        static void OnSettingsLoaded () {
            if (GameState.settingsSaveState.ContainsKey(timeScaleKey))
                timeScale = (float)GameState.settingsSaveState.Load(timeScaleKey);
        }  
        static void OnSettingsSave () {
            GameState.settingsSaveState.UpdateState(timeScaleKey, timeScale);
        }
        
        const string gameDateKey = "gameDate";
        static void OnGameLoaded () {
            ResetDilationSystem();
            if (GameState.settingsSaveState.ContainsKey(gameDateKey))
                gameDate = (GameDate)GameState.settingsSaveState.Load(gameDateKey);
        }

        static void OnSaveGame (List<string> loadedScenes) {
            GameState.settingsSaveState.UpdateState(gameDateKey, gameDate);
        }

        void Update()
        {
            UpdateEngineScales();

            if (showFPS)
                fpsTracker.Update();
            
            if (GameManager.isPaused)
                return;
            if (GameManager.isInMainMenuScene)
                return;
            
            DilationUpdate();
            ProgressTime();
        }


        #region TIMEDILATION
        public static List<TimeDilator> dilators = new List<TimeDilator>();
        // get the lowest possible time dilation, so dilators going above base (if set)
        // dont interfere
        static float dilation {
            get {
                float t = baseDilation;
                for (int i = 0; i < dilators.Count; i++) {
                    float dt = dilators[i].CalculateDilation(baseDilation);
                    if (dt < t) {
                        t = dt;
                    }
                }
                return t;
            }
        }
        static float baseDilation;
                
        //set duration < 0 for permanent time dilation
        public static TimeDilator DilateTime (float dilation, float fadeIn, float duration, float fadeOut) {
            return dilators.AddNew(new TimeDilator(dilation, fadeIn, duration, fadeOut));
        }

        static void ResetDilationSystem () {
            ResetBaseDilation();
            ForceEndDilation();
        }
        static void SetBaseDilation (float dilation) {
            baseDilation = dilation;
        }
        static void ResetBaseDilation () {
            SetBaseDilation(GameTimeSettings.instance.baseTimeDilation);
        }

        public static void ForceEndDilation () {
            dilators.Clear();
        }
        public static void EndDilation (TimeDilator dilator, float speed) {
            dilator.EndTimeDilation(speed);
        }

        static void DilationUpdate () {
            float dTime = Time.unscaledDeltaTime;
            for (int i = dilators.Count - 1; i >= 0; i--) {
                if (dilators[i].UpdateTimeDilation(dTime)) {
                    dilators.Remove(dilators[i]);
                }
            }
        }
        #endregion

        static float actualFixedTimeStep { get { return GameTimeSettings.instance.fixedTimeStep / GameTimeSettings.instance.fixedTimeStepFrequencyMultiplier; } }
        static void UpdateEngineScales () {
            float timeD = dilation;
            float s = timeD * (GameManager.isPaused ? 0 : 1);
            if (Time.timeScale != s)
                Time.timeScale = s;
            float fs = actualFixedTimeStep * timeD;
            if (Time.fixedDeltaTime != fs)
                Time.fixedDeltaTime = fs;
            float ms = GameTimeSettings.instance.maxTimeStep * timeD;
            if (Time.maximumDeltaTime != ms)
                Time.maximumDeltaTime = ms;
        }

        #region TIME_OF_DAY

        [Command("showdatetime", "", "Time", true)]
        static bool showDateTime = true;
        

        public static float _timeScale = 10; 

        [Command("timescale", "The game time of day scale (2 = twice as fast as real life)", "Time", false)]
        static float timeScale { 
            get { return _timeScale; } 
            set { _timeScale = Mathf.Clamp(value, 1, 50); }
        }

        [Command("resettimescale", "Reset timescale to the game default", "Time", false)]
        static void ResetTimeScale () {
            timeScale = GameTimeSettings.instance.defaultTimeScale;
        }

        
        [Command("hoursingameday", "Get the number of real time hours in a game day", "Time", false)]
        static float hoursInGameDay { get { return GameDate.k_HoursPerDay / timeScale; } }
        static float secondsInGameDay { get { return hoursInGameDay * 60 * 60; } }
        
        static GameDate gameDate;

        static void ResetTimeOfDayAndDate () {
            gameDate = new GameDate(GameTimeSettings.instance.defaultGameDate);
        }
		
        [Command("getdate", "Get the current game date", "Time", true)]
        public static string dateString { get { return gameDate.dateString; } }
        [Command("gettod", "Get the current time of day", "Time", true)]
		public static string timeOfDayString { get { return gameDate.timeOfDayString; } }

        // mm/dd/yy
        public static event Action<int, int, int> daily, monthly, yearly;
        public static event Action hourly, halfHourly;

		static void ProgressTime()
		{
            gameDate.ProgressTime(secondsInGameDay, out bool halfHour, out bool hour, out bool day, out bool month, out bool year);
            if (halfHour && halfHourly != null)
                halfHourly();
            if (hour && hourly != null)
                hourly();
            if (day && daily != null)
                daily(gameDate.month, gameDate.day, gameDate.year);
            if (month && monthly != null)
                monthly(gameDate.month, gameDate.day, gameDate.year);
            if (year && yearly != null)
                yearly(gameDate.month, gameDate.day, gameDate.year);
		}



        static bool initGUI;
        static GUIContent dtGUI;
        static Vector2 dtSize;
        static void InitGUI() {
            if (!initGUI) {

                dtGUI = new GUIContent("Jan 31, 100000 [12:12]");
                dtSize = DebugUI.DebugStyle(TextAnchor.MiddleLeft, 14).CalcSize(dtGUI);
                initGUI = true;
            }
        }

        static void OnDateTimeGUI()
        {
            InitGUI();
            dtGUI.text = dateString + " [" + timeOfDayString + "]";
            GUI.Box(new Rect(Screen.width - dtSize.x, 32, dtSize.x, dtSize.y), dtGUI, DebugUI.DebugStyle(TextAnchor.MiddleLeft, 14));
        }
        #endregion


        void OnGUI () {

            if (showFPS)
                fpsTracker.OnGUI();


            if (!GameManager.isInMainMenuScene)
                if (showDateTime)
                    OnDateTimeGUI ();
                
                
        }


        #region FPSCOUNTER
        [Command("showfps", "", "Time", false)]
        static bool showFPS = true;
        static FPSCounter fpsTracker = new FPSCounter();
        #endregion

	}
}