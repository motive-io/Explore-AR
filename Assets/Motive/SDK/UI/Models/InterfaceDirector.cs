// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;

namespace Motive.UI.Models
{
    /// <summary>
    /// Used to send commands to the app, generally to update
    /// the interface or state in some way.
    /// </summary>
    public class InterfaceDirector : ScriptObject
    {
        public ScriptObject[] Commands { get; set; }
    }
}
