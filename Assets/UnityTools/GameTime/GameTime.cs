using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityTools.Internal;
using UnityTools.GameSettingsSystem;
namespace UnityTools {

    public class GameTime : Singleton<GameTime>
    {
        static GameTimeSettings _settings;
        static GameTimeSettings settings {
            get {
                if (_settings == null) _settings = GameSettings.GetSettings<GameTimeSettings>();
                return _settings;
            }
        }

        public static float timeDilation { get { return instance.currentTimeDilation; } }
        public static bool timeDilated { get { return instance.timeDilationInProgress; } }
                
        //set duration < 0 for permanent time dilation
        public static void SetTimeDilation (float timeDilation, float beginTime, float duration, float endTime, bool forceChange) {
            instance._SetTimeDilation ( timeDilation, beginTime, duration, endTime, forceChange);
        }

        public static void ResetTimeDilation (float speed = 0) {
            instance._ResetTimeDilation (speed);
        }
        void _SetTimeDilation (float timeDilation, float beginTime, float duration, float endTime, bool forceChange) {
            if (!forceChange && timeDilationInProgress) {
                // Debug.LogWarning("Already using time dilation");
                return;
            }
            
            timeDilationInProgress = true;
            timeDilationPhase = TimeDilationPhase.StartingDilation;
            timeT = 0;

            timeDilationSpeeds = new Vector3(beginTime, duration, endTime);
            timeDilationTarget = timeDilation;
        }

        void _ResetTimeDilation (float speed = 0) {
            if (timeDilationInProgress) {
                timeDilationSpeeds = new Vector3(speed, speed, speed);
                EndTimeDilationPhase(TimeDilationPhase.EndingDilation);
            }
        }
        
        // Start is called before the first frame update
        void Start()
        {
            currentTimeDilation = settings.baseTimeDilation;
            UpdateScales();
        }

        void Update()
        {
            if (timeDilationInProgress) UpdateTimeDilation(Time.unscaledDeltaTime);
            else currentTimeDilation = settings.baseTimeDilation;
            UpdateScales();
        }
    
        enum TimeDilationPhase { StartingDilation, InDilation, EndingDilation };
        TimeDilationPhase timeDilationPhase;
        float currentTimeDilation, timeDilationTarget, timeT;
        bool timeDilationInProgress;
        Vector3 timeDilationSpeeds;
            
        void UpdateScales () {
            Time.timeScale = currentTimeDilation * (GameManager.isPaused ? 0 : 1);
            Time.fixedDeltaTime = settings.actualFixedTimeStep * currentTimeDilation;
            Time.maximumDeltaTime = settings.maxTimeStep * currentTimeDilation;  
        }
            
        bool SmoothTime (float orig, float target, float unscaledDeltaTime){
            if (timeDilationSpeeds[(int)timeDilationPhase] <= 0) {
                timeT = 1.0f;
            }
            else {
                timeT += unscaledDeltaTime * (1.0f / timeDilationSpeeds[(int)timeDilationPhase]);
                if (timeT > 1.0f)
                    timeT = 1.0f;
            }
            currentTimeDilation = Mathf.Lerp(orig, target, timeT);
            return timeT >= 1.0f;
        }


        void UpdateTimeDilation (float unscaledDeltaTime) {
            //going towards time dilation
            if (timeDilationPhase == TimeDilationPhase.StartingDilation) {
                if (SmoothTime (settings.baseTimeDilation, timeDilationTarget, unscaledDeltaTime)) {
                    EndTimeDilationPhase(TimeDilationPhase.InDilation);
                }        
            }
            //countint duration
            else if (timeDilationPhase == TimeDilationPhase.InDilation) {
                // anything less than 0 is permanenet
                if (timeDilationSpeeds[(int)timeDilationPhase] >= 0) {
                    timeT += unscaledDeltaTime;
                    if (timeT >= timeDilationSpeeds[(int)timeDilationPhase]) {
                        EndTimeDilationPhase(TimeDilationPhase.EndingDilation);
                    }
                }
            }
            //going to normal
            else if (timeDilationPhase == TimeDilationPhase.EndingDilation) {
                if (SmoothTime (timeDilationTarget, settings.baseTimeDilation, unscaledDeltaTime)) {
                    EndTimeDilation();
                }
            }
        }

        


        
    
        void EndTimeDilationPhase(TimeDilationPhase nextPhase) {
            currentTimeDilation = timeDilationTarget;
            timeDilationPhase = nextPhase;
            timeT = 0;
        }

        void EndTimeDilation () {
            currentTimeDilation = settings.baseTimeDilation;
            timeDilationInProgress = false;
        }    
    }
}
