// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Pusshes a panel with Objective information when an objective info item is selected.
    /// </summary>
    public class ObjectiveInfoPanelSelectComponent : PanelComponent<TaskObjective>
    {
        public PanelLink ObjectiveInfoPanel;

        public override void Populate(TaskObjective obj)
        {
            if (ObjectiveInfoPanel)
            {
                ObjectiveInfoPanel.Push(obj);
            }
        }
    }
}