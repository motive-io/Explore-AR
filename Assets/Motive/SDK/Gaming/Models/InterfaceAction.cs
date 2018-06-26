using Motive.Core.Scripting;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Interface actions let you add customized instructions to your app that you can control
    /// through Motive.
    /// </summary>
    public class InterfaceAction : ScriptObject
    {
        public string Instruction { get; set; }
    }
}
