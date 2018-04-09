// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// An objective in an assignment.
    /// </summary>
    public class AssignmentObjective : ScriptObject
    {
        public TaskObjective Objective { get; set; }
        /// <summary>
        /// If true, this objective is optional.
        /// </summary>
        public bool IsOptional { get; set; }
        /// <summary>
        /// If true, this objective is hidden from the user. Can be un-hidden using
        /// an ObjectiveActivator.
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
