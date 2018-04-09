// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Apps;
using Motive.Unity.Gaming;
using Motive.Unity.Maps;
using System;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Monitors the todo manager and displays a panel for the current todo item
    /// if possible. This is important for guiding the user through their experience.
    /// </summary>
    public class TodoPreviewPanelHandler : SelectedLocationPanelHandler
    {
        Panel m_toDoPanel;
        object m_toDoPanelData;
        IPlayerTaskDriver m_currTodoDriver;

        // Other panels not linked to annotation handlers
        public Panel CharacterTaskPanel;
        public Panel SimpleTaskPanel;
        public Panel VisualMarkerTaskPanel;
        public Panel ObjectivePanel;

        protected override void Awake()
        {
            base.Awake();

            AppManager.Instance.Initialized += App_Initialized;
        }

        void App_Initialized(object sender, System.EventArgs e)
        {
            TodoManager.Instance.OnUpdate.AddListener(HandleUpdated);
        }

        private void HandleUpdated()
        {
            if (TodoManager.Instance.CurrentTodoDriver != null)
            {
                SetTodoTask(TodoManager.Instance.CurrentTodoDriver);
            }
            else if (TodoManager.Instance.CurrentTodoObjective != null)
            {
                SetToDoPanel(ObjectivePanel, TodoManager.Instance.CurrentTodoObjective);
            }
            else
            {
                SetTodoTask(null);
            }
        }

        /// <summary>
        /// Sets the current todo panel
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="data"></param>
        public void SetToDoPanel(Panel panel, object data = null)
        {
            // Don't re-push the same panel with the same data
            if (panel == CurrentPanel &&
                panel != null &&
                panel.Data == data)
            {
                return;
            }

            var oldToDoPanel = m_toDoPanel;
            m_toDoPanelData = data;
            m_toDoPanel = panel;

            if (oldToDoPanel != null && oldToDoPanel != m_toDoPanel)
            {
                oldToDoPanel.Back();
            }

            ShowSelectedLocationPanel(m_toDoPanel, data);
        }

        public override void ShowSelectedLocationPanel(Panel toShow, object data, Action onClose = null)
        {
            base.ShowSelectedLocationPanel(toShow, data, () =>
                {
                    if (CurrentPanel == toShow)
                    {
                        CurrentPanel = null;
                    }

                    if (onClose != null)
                    {
                        onClose();
                    }
                });
        }

        public override void HideSelectedLocationPanel(Panel toHide, object data)
        {
            if (toHide != null && toHide == m_toDoPanel)
            {
                if (toHide.Data == m_toDoPanelData)
                {
                    // Same panel, same data as the todo panel. Leave it open.
                    return;
                }
            }
            else
            {
                base.HideSelectedLocationPanel(toHide, data);
            }

            if (m_toDoPanel != null)
            {
                PanelManager.Instance.Push(m_toDoPanel, m_toDoPanelData);
            }
        }

        public static new TodoPreviewPanelHandler Instance
        {
            get
            {
                return (TodoPreviewPanelHandler)SelectedLocationPanelHandler.Instance;
            }
        }

        // TODO: everything below here is a bit of a grab-bag that needs a refactor.
        // Essentially, task driver delegates should handle the todo list in some way.
        // For now, we know what we want each one to do so we'll do it directly.
        void SetTodoTask(IPlayerTaskDriver driver)
        {
            // Don't preview complete/closed tasks
            if (driver != null && (driver.IsComplete || driver.ActivationContext.IsClosed))
            {
                m_currTodoDriver = null;

                driver.SetFocus(false);

                SetToDoPanel(null);

                return;
            }

            if (m_currTodoDriver != null && m_currTodoDriver != driver)
            {
                m_currTodoDriver.SetFocus(false);
            }

            m_currTodoDriver = driver;

            if (driver != null)
            {
                if (driver is LocationTaskDriver && LocationTaskAnnotationHandler.Instance)
                {
                    var ann = LocationTaskAnnotationHandler.Instance.GetNearestAnnotation((LocationTaskDriver)driver);

                    if (ann != null)
                    {
                        MapController.Instance.FocusAnnotation(ann);
                    }
                    else
                    {
                        SetToDoPanel(LocationTaskAnnotationHandler.Instance.SelectedLocationPanel, driver);
                    }
                }
                else if (driver is ARTaskDriver && ARTaskAnnotationHandler.Instance)
                {
                    var ann = ARTaskAnnotationHandler.Instance.GetNearestAnnotation((ARTaskDriver)driver);

                    if (ann != null)
                    {
                        MapController.Instance.FocusAnnotation(ann);
                    }
                    else
                    {
                        SetToDoPanel(ARTaskAnnotationHandler.Instance.SelectedLocationPanel, driver);
                    }
                }
#if MOTIVE_VUFORIA
            else if (driver is VisualMarkerTaskDriver)
            {
                SetToDoPanel(VisualMarkerTaskPanel, driver);
            }
#endif
                else if (driver.Task.Type == "motive.gaming.characterTask")
                {
                    SetToDoPanel(CharacterTaskPanel, driver);
                }
                else
                {
                    SetToDoPanel(SimpleTaskPanel, driver);
                }

                driver.SetFocus(true);
            }
            else
            {
                SetToDoPanel(null);
            }            
        }
    }

}