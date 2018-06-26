// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

namespace Motive.UI.Framework
{
    /// <summary>
    /// A panel that manages a tabbed interface.
    /// </summary>
    public class TabPanel : Panel
    {
        public TabControllerButton SelectedTab;

        TabControllerButton[] m_buttons;

        void Start()
        {
            m_buttons = GetComponentsInChildren<TabControllerButton>().ToArray();
            TabControllerButton selectedButton = m_buttons.FirstOrDefault();

            foreach (var tabButton in m_buttons)
            {
                var button = tabButton.GetComponent<Button>();

                if (tabButton == SelectedTab)
                {
                    selectedButton = tabButton;
                }

                if (button)
                {
                    var _t = tabButton;

                    button.onClick.AddListener(() => { Select(_t); });
                }
            }

            Select(selectedButton);
        }

        public virtual void SelectPane(GameObject pane)
        {
            var button = m_buttons.Where(b => b.Pane == pane).FirstOrDefault();

            if (button)
            {
                Select(button);
            }
        }

        public virtual void Select(TabControllerButton button)
        {
            SelectedTab = button;

            SelectedTab.Pane.SetActive(true);
            button.SetSelected(true);

            foreach (var obj in m_buttons)
            {
                if (obj != SelectedTab)
                {
                    obj.SetSelected(false);
                    obj.Pane.SetActive(false);
                }
            }
        }
    }
}
