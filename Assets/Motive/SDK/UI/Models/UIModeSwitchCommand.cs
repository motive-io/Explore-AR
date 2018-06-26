using Motive.Core.Scripting;

namespace Motive.UI.Models
{
    /// <summary>
    /// Initiates a switch between UI modesl
    /// </summary>
    public class UIModeSwitchCommand : ScriptObject
    {
        public string Mode { get; set; }
    }
}
