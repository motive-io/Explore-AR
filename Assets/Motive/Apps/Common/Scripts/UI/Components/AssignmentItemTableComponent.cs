// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using System.Linq;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays a set of assignment items (tasks or objectives) in a table.
    /// </summary>
    public class AssignmentItemTableComponent : TableComponent
    {
        public Text AssignmentTitle;
        public AssignmentItemInfo ItemPrefab;
        public bool ShowAllTasksIfNoAssignment;

        AssignmentDriver m_currentDriver;

        string m_tasksDefaultTitle;

        public override void DidShow()
        {
            if (AssignmentTitle && m_tasksDefaultTitle == null)
            {
                m_tasksDefaultTitle = AssignmentTitle.text;
            }

            base.DidShow();

            TaskManager.Instance.Updated += Tasks_Updated;
        }

        public override void Populate()
        {
            Table.Clear();

            base.Populate();

            m_currentDriver = TaskManager.Instance.FirstAssignment;

            PopulateComponents(m_currentDriver);

            if (m_currentDriver != null)
            {
                SetText(AssignmentTitle, m_currentDriver.Assignment.Title);

                if (m_currentDriver.Assignment.Objectives != null)
                {
                    int idx = 1;

                    foreach (var obj in TaskManager.Instance.GetActiveObjectivesForAssignment(m_currentDriver.Assignment))
                    {
                        var objTask = TaskManager.Instance.GetTaskDriverForObjective(obj.Objective.Id);

                        if (objTask != null)
                        {
                            AddTask(objTask, idx++);
                        }
                        else
                        {
                            AddObjective(obj, idx++);
                        }
                    }
                }
            }
            else if (ShowAllTasksIfNoAssignment)
            {
                SetText(AssignmentTitle, m_tasksDefaultTitle);

                var tasks = TaskManager.Instance.AllTaskDrivers.Where(d => d.ShowInTaskPanel)
                    .OrderByDescending(d => d.IsComplete);

                int idx = 1;

                foreach (var task in tasks)
                {
                    AddTask(task, idx++);
                }
            }
        }

        private void AddObjective(AssignmentObjective obj, int idx)
        {
            var asnItem = new ObjectiveAssignmentItem(m_currentDriver, obj, idx);

            var item = AddSelectableItem(ItemPrefab, (_item) =>
            {
                PopulateComponents(asnItem);
            });

            item.Populate(asnItem);
        }

        private void AddTask(IPlayerTaskDriver objTask, int idx)
        {
            var asnItem = new TaskAssignmentItem(m_currentDriver, objTask, idx);

            var item = AddSelectableItem(ItemPrefab, (_item) =>
            {
                PopulateComponents(asnItem);
            });

            item.Populate(asnItem);
        }

        private void Tasks_Updated(object sender, System.EventArgs e)
        {
            ThreadHelper.Instance.CallOnMainThread(() =>
            {
                Populate();
            });
        }

        public override void DidHide()
        {
            base.DidHide();

            TaskManager.Instance.Updated -= Tasks_Updated;
        }
    }

}