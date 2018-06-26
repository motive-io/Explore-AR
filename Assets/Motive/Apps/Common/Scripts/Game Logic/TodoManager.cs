// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using Motive.Unity.Apps.Attractions;
using Motive.Unity.AR;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using System;
using System.Linq;
using UnityEngine.Events;

namespace Motive.Unity.Apps
{
    /// <summary>
    /// This class manages the player's "todo" list, and in particular
    /// tries to decide which particular item should be at the forefront 
    /// of the list.
    /// </summary>
    public class TodoManager : Singleton<TodoManager>
    {
        public IPlayerTaskDriver m_currTodoDriver;
        public TaskObjective m_currTodoObjective;
        bool m_todoIsOverride;

        public IPlayerTaskDriver CurrentTodoDriver
        {
            get
            {
                return m_currTodoDriver;
            }
        }

        public TaskObjective CurrentTodoObjective
        {
            get
            {
                return m_currTodoObjective;
            }
        }

        public UnityEvent OnUpdate;

        public TodoManager()
        {
            OnUpdate = new UnityEvent();
        }

        public void Initialize()
        {
            if (AttractionManager.Instance)
            {
                AttractionManager.Instance.OnUpdated.AddListener(HandleUpdated);
            }
            TaskManager.Instance.Updated += Handle_Updated;

            if (ARWorld.Instance)
            {
                ARWorld.Instance.OnUpdated.AddListener(HandleUpdated);
            }

            AppManager.Instance.ScriptsStarted += Handle_Updated;
        }

        private void HandleUpdated()
        {
            Update();
        }

        private void Handle_Updated(object sender, EventArgs e)
        {
            Update();
        }

        // TODO: everything below here is a bit of a grab-bag that needs a refactor.
        // Essentially, task driver delegates should handle the todo list in some way.
        // For now, we know what we want each one to do so we'll do it directly.
        void SetTodoTask(IPlayerTaskDriver driver)
        {
            // Don't preview complete/closed tasks
            if (driver != null && (driver.IsComplete || driver.ActivationContext.IsClosed))
            {
                m_todoIsOverride = false;
                m_currTodoDriver = null;

                driver.SetFocus(false);

                return;
            }

            if (m_currTodoDriver != null && m_currTodoDriver != driver)
            {
                m_currTodoDriver.SetFocus(false);
            }

            m_currTodoDriver = driver;

            if (m_currTodoDriver != null)
            {
                m_currTodoDriver.SetFocus(true);
            }
        }

        public void OverrideTodoTask(IPlayerTaskDriver driver)
        {
            if (driver != null)
            {
                m_currTodoObjective = null;

                m_todoIsOverride = true;

                SetTodoTask(driver);

                ThreadHelper.Instance.CallExclusive(RaiseEvent);
            }
        }

        public void OverrideTodoObjective(TaskObjective objective)
        {
            if (objective != null)
            {
                SetTodoTask(null);

                m_todoIsOverride = true;

                m_currTodoObjective = objective;

                ThreadHelper.Instance.CallExclusive(RaiseEvent);
            }
        }

        void ClearTodoTask(IPlayerTaskDriver driver)
        {
            if (driver == m_currTodoDriver)
            {
                m_todoIsOverride = false;

                SetTodoTask(null);

                ThreadHelper.Instance.CallExclusive(RaiseEvent);
            }
        }

        public void ClearOverrideTodoTask(IPlayerTaskDriver driver)
        {
            m_todoIsOverride = false;

            if (driver == m_currTodoDriver)
            {
                SetTodoTask(null);

                ThreadHelper.Instance.CallExclusive(RaiseEvent);
            }
        }

        void RaiseEvent()
        {
            if (OnUpdate != null)
            {
                OnUpdate.Invoke();
            }
        }

        void Update()
        {
            // Only update the preview panel if:
            // 1. There is no current panel, or
            // 2. The current driver is either inactive, complete, or closed
            // Otherwise we don't change anything because we want this panel to be
            // somewhat sticky.
            if (m_currTodoDriver != null &&
                (!TaskManager.Instance.ActiveTaskDrivers.Contains(m_currTodoDriver) ||
                    m_currTodoDriver.IsComplete ||
                    m_currTodoDriver.ActivationContext.IsClosed))
            {
                ClearTodoTask(m_currTodoDriver);
            }

            IPlayerTaskDriver driver = null;

            // Keep the current todo driver if it's an override and it's active.
            // Otherwise compute the best todo driver to show the user.
            if (m_currTodoDriver != null &&
                m_todoIsOverride &&
                TaskManager.Instance.ActiveTaskDrivers.Contains(m_currTodoDriver))
            {
                driver = m_currTodoDriver;
            }
            else
            {
                m_todoIsOverride = false;

                // Todo: can we centralize the "todo" item into
                // one property?
                m_currTodoObjective = null;

                var adriver = TaskManager.Instance.FirstAssignment;

                if (adriver != null)
                {
                    driver = TaskManager.Instance.GetActiveObjectiveTasks(adriver)
                        .OrderBy(d => d.ActivationContext.ActivationTime)
                        .FirstOrDefault();
                }

                // Prioritize task drivers attached to the assignment,
                // fall back to the rest of the drivers.
                if (driver == null)
                {
                    driver = TaskManager.Instance.ActiveTaskDrivers
                        .OrderBy(d => d.ActivationContext.ActivationTime)
                        .FirstOrDefault();
                }
            }

            if (driver != null)
            {
                SetTodoTask(driver);
            }
            /*
             * Looking forward to attractions
            else
            {
                var todo = AttractionManager.Instance.GetNextToDo();

                if (todo != null)
                {
                    var annotation = AttractionManager.Instance.GetAnnotation(todo.Attraction.Id);

                    SetToDoPanel(AttractionManager.Instance.SelectedLocationPanel, annotation);
                }
                else
                {
                    SetToDoPanel(null);
                }
            }*/

            ThreadHelper.Instance.CallExclusive(RaiseEvent);
        }
    }

}