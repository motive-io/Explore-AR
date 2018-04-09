// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Apps;
using Motive.Unity.Gaming;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Automatically pushes a panel with Assignment Item information when
    /// there's a new Assignment Item from the TodoManager.
    /// </summary>
    public class AssignmentTodoItemPanelPusher : PanelComponent<AssignmentDriver>
    {
        public PanelLink AssignmentItemPanel;
        public bool CloseWhenTaskCompletes;

        AssignmentItem m_pushedItem;

        public override void DidShow()
        {
            TodoManager.Instance.OnUpdate.AddListener(Todo_Updated);

            Todo_Updated();

            base.DidShow();
        }

        void Todo_Updated()
        {
            var assnDriver = Data ?? TaskManager.Instance.FirstAssignment;

            if (m_pushedItem != null &&
                m_pushedItem.IsComplete &&
                AssignmentItemPanel.GetPanel().Data == m_pushedItem)
            {
                AssignmentItemPanel.Back();
            }

            if (TodoManager.Instance.CurrentTodoDriver != null &&
                assnDriver != null &&
                assnDriver.Assignment.Objectives != null)
            {
                var idx = 1;

                foreach (var obj in TaskManager.Instance.GetActiveObjectivesForAssignment(assnDriver.Assignment))
                {
                    var task = TaskManager.Instance.GetTaskDriverForObjective(obj.Objective);

                    if (task == TodoManager.Instance.CurrentTodoDriver)
                    {
                        m_pushedItem = new TaskAssignmentItem(assnDriver, task, idx);

                        AssignmentItemPanel.Push(m_pushedItem);
                    }

                    idx++;
                }
            }
        }

        public override void DidHide()
        {
            base.DidHide();
        }
    }

}