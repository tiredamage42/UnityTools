using UnityEngine;

using System;

namespace UnityTools {
    public abstract class ShakeInfo {
        public UpdateMode updateMode;
        public Vector3 magnitude = Vector3.one;
    }



    public class ShakeHandler {
        
        public float value;
        public bool inShake, towardsTarget;
        Vector2 toFromSpeeds;
        SmoothedValue smoother = new SmoothedValue();
        
        public void StartShake (Vector2 toFromSpeeds, SmoothMethod smooth, bool from0) {
            this.toFromSpeeds = toFromSpeeds;
            towardsTarget = true;
            inShake = true;
            smoother.smooth = smooth;
            if (from0) value = 0;
        }

        public void UpdateShake (float deltaTime, Action onReachTarget, Action onShakeDone) {
            
            float targ = towardsTarget ? 1.0f : 0.0f;
            if (value != targ) {
                smoother.speed = towardsTarget ? toFromSpeeds.x : toFromSpeeds.y;
                value = smoother.Smooth(value, targ, deltaTime);
            }
            if (inShake) {

                if (towardsTarget) {
                    if (value > .99f) {
                        value = 1.0f;
                        towardsTarget = false;
                        if (onReachTarget != null) onReachTarget();
                    }
                }
                else {
                    if (value < .01f) {
                        value = 0.0f;
                        inShake = false;
                        if (onShakeDone != null) onShakeDone();
                    }
                }
            }    
        }
    }

    public abstract class Shake<T> : CustomUpdater where T : ShakeInfo {
        protected ShakeHandler shakeHandler = new ShakeHandler();
        protected T shakeInfo;

        protected void BroadcastShakeEnd (){
            currentOffset = Vector3.zero;
            shakeInfo = null;
            if (onShakeEnd != null) onShakeEnd();
        }

        public event Action onShakeEnd;
        
        protected override UpdateMode GetUpdateMode() {
            if (shakeInfo == null) return UpdateMode.Custom;
            return shakeInfo.updateMode;
        }        
        protected Vector3 targetOffset;
        public Vector3 currentOffset;
        public abstract void StartShake (T shakeInfo);

    }
}