
using UnityEngine;
using UnityTools.DevConsole;
namespace UnityTools {
    public class TimeDilator {
        int phase;
        float timeT, dilation;
        Vector3 speeds;

        public void EndTimeDilation (float speed) {
            speeds.z = speed;
            SetPhase(2);
        }
        public TimeDilator (float dilation, float beginTime, float duration, float endTime) {
            SetPhase(0);
            speeds = new Vector3(beginTime, duration, endTime);
            this.dilation = Mathf.Clamp(dilation, .01f, 1.0f);
        }
        void SetPhase(int nextPhase) {
            phase = nextPhase;
            timeT = Mathf.Min(1, nextPhase);
        }
        bool SmoothTime (float unscaledDeltaTime, float target){
            if (speeds[phase] <= 0) {
                timeT = target;
            }
            else {
                timeT += unscaledDeltaTime * (1.0f / speeds[phase]) * Mathf.Lerp(-1, 1, target);
                timeT = Mathf.Clamp01(timeT);
            }
            return timeT == target;
        }
        public float CalculateDilation (float baseDilation) {
            return Mathf.Lerp(baseDilation, dilation, timeT);
        }
        public bool UpdateTimeDilation (float unscaledDeltaTime) {
            if (phase == 0) {
                if (SmoothTime (unscaledDeltaTime, 1))
                    SetPhase(1);
            }
            else if (phase == 1) {
                if (speeds.y >= 0) {
                    timeT += unscaledDeltaTime;
                    if (timeT >= speeds.y) 
                        SetPhase(2);
                }
            }
            else if (phase == 2) {
                if (SmoothTime (unscaledDeltaTime, 0))
                    return true;
            }
            return false;
        }  
    }
}