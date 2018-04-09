// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.UI.Framework;

namespace Motive.UI.Framework
{
    /// <summary>
    /// A specialized container for handling tabbed interfaces.
    /// </summary>
    public class TabController : PanelContainer
    {
        public TabControllerButton[] Tabs;
        public TabControllerButton SelectedTab;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();

            Select(SelectedTab);
            StackBehavior = PanelStackBehavior.OneAtATime;
        }

        public void Select(TabControllerButton tab)
        {
            SelectedTab = tab;

            foreach (var obj in Tabs)
            {
                obj.SetSelected(obj == SelectedTab);
            }
        }
    }

}