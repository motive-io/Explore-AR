// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.UI.Framework
{
    /// <summary>
    /// A button used as part of a TabController.
    /// </summary>
    public class TabControllerButton : MonoBehaviour
    {
        public TabController Controller;
        public Panel Pane;

        public GameObject ActiveWhenSelected;
        public GameObject ActiveWhenNotSelected;

        public bool IsSelected { get; private set; }

        public virtual void SetSelected(bool selected)
        {
            IsSelected = selected;

            if (ActiveWhenSelected)
            {
                ActiveWhenSelected.SetActive(IsSelected);
            }

            if (ActiveWhenNotSelected)
            {
                ActiveWhenNotSelected.SetActive(!IsSelected);
            }

            if (Pane)
            {
                if (selected)
                {
                    Controller.Show(Pane);
                }
                else
                {
                    Controller.Hide(Pane);
                }
            }
        }
    }
}
