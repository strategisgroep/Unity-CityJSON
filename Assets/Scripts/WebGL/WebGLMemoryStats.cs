using UnityEngine;
using System.Runtime.InteropServices;

namespace Kongregate {
    public class WebGLMemoryStats : MonoBehaviour {
        [Tooltip("Interval (in seconds) between log entries")]
        public uint LogIntervalSeconds = 15;

        //public static uint GetUsedMemorySize() {
        //    return GetTotalStackSize() + GetStaticMemorySize() + GetDynamicMemorySize();
        //}

#if UNITY_WEBGL && !UNITY_EDITOR
        void Start() {
            InvokeRepeating("Log", 0, LogIntervalSeconds);
        }

        private void Log() {
            //var total = GetTotalMemorySize() / 1024 / 1024;
            //var used = GetUsedMemorySize() / 1024 / 1024;
            //var free = GetFreeMemorySize() / 1024 / 1024;
            
            var used = GetUsedMemorySize() / 1024 / 1024;

            Debug.Log(string.Format("WebGL Memory - used: {0}MB",used));
        }


        [DllImport("__Internal")]
        public static extern uint GetUsedMemorySize();
        
#else
        public static uint GetUsedMemorySize() { return 0; }
      
#endif
    }
}
