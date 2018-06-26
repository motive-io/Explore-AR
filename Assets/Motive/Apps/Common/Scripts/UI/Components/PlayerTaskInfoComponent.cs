// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Gaming;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays information for a player task. Takes a driver.
    /// </summary>
    public class PlayerTaskInfoComponent : TaskInfoComponent<IPlayerTaskDriver>
    {
        protected override void Awake()
        {
            base.Awake();
        }
    }
}