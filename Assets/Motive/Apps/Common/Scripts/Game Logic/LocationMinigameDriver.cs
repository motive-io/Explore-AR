// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Gaming
{
    /// <summary>
    /// Base class for location task minigames.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LocationMinigameDriver<T> : LocationMinigameDriverBase where T : ITaskMinigame
    {
        public T Minigame { get; private set; }

        protected LocationMinigameDriver(LocationTaskDriver taskDriver, T minigame)
            : base(taskDriver)
        {
            this.Minigame = minigame;
        }
    }
}