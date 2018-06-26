// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// A component that pushes a panel with assignment item data. For example,
    /// if you attach this to a panel with an AssignmentItemTableComponent, it 
    /// will push the info panel when the user selects an assignment item.
    /// </summary>
    public class AssignmentItemPanelSelectComponent : PanelComponent<AssignmentItem>
    {
        public PanelLink AssignmentItemInfoPanel;

        public override void Populate(AssignmentItem obj)
        {
            if (AssignmentItemInfoPanel)
            {
                AssignmentItemInfoPanel.Push(obj);
            }
        }
    }

}