using UnityEngine;
using UnityTools.GameSettingsSystem;

namespace UnityTools.DevConsole 
{
    // [CreateAssetMenu()]
    public class ConsoleSettings : GameSettingsObjectSingleton<ConsoleSettings>
    {
        // public Font consoleFont;
        public int fontSize = 12;
        public LayerMask clickMask = -1;
    }
}
