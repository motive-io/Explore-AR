// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Activates (or unhides) a hidden objective.
    /// </summary>
    public class ObjectiveActivator : ScriptObject
    {
        public TaskObjective Objective { get; set; }
    }
}
