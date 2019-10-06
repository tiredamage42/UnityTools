using UnityEngine;
using System;

namespace UnityTools {
    /*
        lets static classes hook into the application's update
        
        and run coroutines
    */
    public class UpdateManager : Singleton<UpdateManager>
    {
        public event Action<float> update;
        void Update()
        {
            if (update != null)
                update(Time.deltaTime);
        }
    }
}
