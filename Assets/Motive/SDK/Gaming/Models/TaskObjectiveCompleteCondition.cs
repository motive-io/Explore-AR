// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// A condition that is met when the given objective has been completed.
    /// </summary>
    public class TaskObjectiveCompleteCondition : AtomicCondition
    {
        public ObjectReference TaskObjectiveReference { get; set; }
    }
}
