// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Adds or removes the given valuables along with a reward pop-up.
    /// </summary>
    public class PlayerReward : ScriptObject
    {
        public ValuablesCollection Reward { get; set; }
    }
}