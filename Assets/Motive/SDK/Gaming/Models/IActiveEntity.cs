// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Active entities can emit and receive actions to create
    /// sophisticated open-world game play.
    /// </summary>
    public interface IActiveEntity : IScriptObject
    {
        EntityActionBehaviour[] EmittedActions { get; }
        EntityActionBehaviour[] ReceivedActions { get; }
    }
}
