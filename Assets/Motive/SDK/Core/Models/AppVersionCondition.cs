// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Motive.Core.Scripting;

namespace Motive.Core.Models
{
    /// <summary>
    /// Compares the specified version using the numerical operator.
    /// </summary>
    public class AppVersionCondition : AtomicCondition
    {
        public string Version { get; set; }
        public NumericalConditionOperator? Operator { get; set; }
    }
}
