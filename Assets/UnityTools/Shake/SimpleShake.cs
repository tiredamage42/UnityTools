

using UnityEngine;

namespace UnityTools {
   

    [System.Serializable] public class SimpleShakeInfo : ShakeInfo{
        public Vector3 magnitudeRandomMask = new Vector3(0,0,0);
        public Vector2 toFromSpeed = new Vector2(1,1);
        public SmoothMethod smoothMethod;
    }


    
    public class SimpleShaker : Shake<SimpleShakeInfo>
    {
        public override void UpdateLoop(float deltaTime) {
            shakeHandler.UpdateShake(deltaTime, null, BroadcastShakeEnd);
            currentOffset = shakeHandler.value * targetOffset;
        }

        public override void StartShake (SimpleShakeInfo shakeInfo){
            this.shakeInfo = shakeInfo;
            targetOffset = RandomTools.RandomSign(shakeInfo.magnitude, shakeInfo.magnitudeRandomMask);
            shakeHandler.StartShake(shakeInfo.toFromSpeed, shakeInfo.smoothMethod, false);
        }        
    }

}