// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;
using Motive.Core.Models;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// A condition that is met when an objective is linked to a task that
    /// can be completed, usually a "put" or "exchange" task for which the
    /// player has the required items.
    /// </summary>
    public class CanCompleteObjectiveTaskCondition : AtomicCondition
    {
        public ObjectReference ObjectiveReference { get; set; }
    }
}

