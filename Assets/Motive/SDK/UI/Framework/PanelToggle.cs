// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Toggles a panel between active and inactive.
    /// </summary>
    public class PanelToggle : MonoBehaviour
    {
        public PanelLink PanelLink;
        public GameObject[] ActiveWhenSelected;
        public GameObject[] ActiveWhenNotSelected;

        bool m_selected;

        // Use this for initialization
        void Start()
        {
            SetSelected(false);
        }

        void SetSelected(bool selected)
        {
            m_selected = selected;

            if (ActiveWhenSelected != null)
            {
                foreach (var go in ActiveWhenSelected)
                {
                    if (go)
                    {
                        go.SetActive(selected);
                    }
                }
            }

            if (ActiveWhenNotSelected != null)
            {
                foreach (var go in ActiveWhenNotSelected)
                {
                    if (go)
                    {
                        go.SetActive(!selected);
                    }
                }
            }
        }

        public void Toggle()
        {
            if (PanelLink == null)
            {
                PanelLink = GetComponent<PanelLink>();
            }


            if (m_selected)
            {
                PanelLink.Back();
            }
            else
            {
                SetSelected(true);

                PanelLink.Push(null, () =>
                {
                    SetSelected(false);
                });
            }
        }
    }
}