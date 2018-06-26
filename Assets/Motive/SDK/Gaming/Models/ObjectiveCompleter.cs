// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Completes an objective with a Motive script.
    /// </summary>
    public class ObjectiveCompleter : ScriptObject
    {
        public TaskObjective Objective { get; set; }
    }
}
