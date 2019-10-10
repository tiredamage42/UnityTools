using UnityEngine;

namespace UnityTools {
  
    [System.Serializable] public class DurationShakeInfo : ShakeInfo {

        public float duration = 1;
        public float frequency = .1f;
        public AnimationCurve decay = AnimationCurve.Linear(0, 1, 1, 0);
        public SmoothedValue smoother;
    }


    public class DurationShake : Shake<DurationShakeInfo>
    {
         
        Vector3 lastOffset;
        float timeShook, frequencyTimer;

        void OnShakeReached () {
            // set last offset to zero, so it starts resetting
            lastOffset = Vector3.zero;
        }
        
        public override void UpdateLoop(float deltaTime) {

            shakeHandler.UpdateShake(deltaTime, OnShakeReached, null);

            frequencyTimer += deltaTime;
            if (frequencyTimer >= shakeInfo.frequency) {   
                OnFrequencyReached(CalculateDecay());
            }

            currentOffset = Vector3.Lerp(lastOffset, targetOffset, shakeHandler.value);

            if (shakeHandler.value == 0 && Time.time - timeShook >= shakeInfo.duration) {
                BroadcastShakeEnd();
            }
        }
        
        float CalculateDecay () {
            float timeLerp = Mathf.Clamp01( (Time.time - timeShook) / shakeInfo.duration );
            
            // just in case curve doesnt end at 0
            if (timeLerp == 1) return 0;

            return shakeInfo.decay.Evaluate(timeLerp);
        }


        public override void StartShake (DurationShakeInfo shakeInfo){
            this.shakeInfo = shakeInfo;
            timeShook = Time.time;
            OnFrequencyReached(1);
        }
            
        void OnFrequencyReached (float decay) {
            shakeHandler.StartShake(Vector2.one * shakeInfo.smoother.speed, shakeInfo.smoother.smooth, true);
            
            lastOffset = currentOffset;
            targetOffset = decay == 0 ? Vector3.zero : RandomTools.RandomSign(shakeInfo.magnitude) * decay;
            frequencyTimer = 0;
        }
    }
    
}