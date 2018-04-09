// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Pushes a task info panel when sent a task driver.
    /// </summary>
    public class TaskInfoPanelSelectComponent : PanelComponent<IPlayerTaskDriver>
    {
        public PanelLink TaskInfoPanel;

        public override void Populate(IPlayerTaskDriver obj)
        {
            if (TaskInfoPanel)
            {
                TaskInfoPanel.Push(obj);
            }
        }
    }
}