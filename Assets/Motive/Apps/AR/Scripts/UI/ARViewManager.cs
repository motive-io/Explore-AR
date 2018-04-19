// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Utilities;
using Motive.UI.Framework;
using Motive.Unity.Apps;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Manages the AR view panels, including the Guide Panel (arrows and instructions),
    /// Task Complete Panel (checkmark for task completion), Assignment Complete Panel
    /// (checkmark for assignment completion).
    /// </summary>
    /// <seealso cref="SingletonComponent{Motive.Unity.UI.ARViewManager}" />
    public class ARViewManager : SingletonComponent<ARViewManager>
    {
        public Panel GuidePanel;
        public Panel TaskCompletePanel;
        public Panel AssignmentCompletePanel;

        public bool ShowTaskComplete;
        public bool ShowAssignmentComplete;

        ListDictionary<string, CollectibleTableItem> m_interactiveCollectibles;
        HashSet<string> m_currentCollectibles;

        ARGuideData m_guideData;
        IPlayerTaskDriver m_taskCompleteDriver;

        protected override void Start()
        {
            m_currentCollectibles = new HashSet<string>();
            m_interactiveCollectibles = new ListDictionary<string, CollectibleTableItem>();

            AppManager.Instance.Initialized += (a, b) =>
            {
                TaskManager.Instance.Updated += Tasks_Updated;
            };

            ARQuestAppUIManager.Instance.OnModeChanged.AddListener(UIModeChanged);
        }

        void UIModeChanged()
        {
            if (ARQuestAppUIManager.Instance.CurrentMode != ARQuestAppUIManager.Mode.AR)
            {
                // Clear complete task
                m_taskCompleteDriver = null;
            }
            else
            {
                SyncPanels();
            }
        }

        void SyncPanels()
        {
            // 1. Show AssignmentComplete if required.
            // 2. Show guide.
            // 3. Show task complete.
            var assnDriver = TaskManager.Instance.FirstAssignment;

            if (ShowAssignmentComplete &&
                AssignmentCompletePanel &&
                assnDriver != null &&
                assnDriver.IsComplete && !
                assnDriver.ActivationContext.IsClosed)
            {
                PanelManager.Instance.Push(AssignmentCompletePanel, assnDriver, () =>
                {
                    SyncPanels();
                });

                if (TaskCompletePanel)
                {
                    TaskCompletePanel.Back();
                }

                if (GuidePanel)
                {
                    GuidePanel.Back();
                }

                // Either of these options undoes task complete
                m_taskCompleteDriver = null;
            }
            else if (m_guideData != null && GuidePanel)
            {
                PanelManager.Instance.Push(GuidePanel, m_guideData);

                if (TaskCompletePanel)
                {
                    TaskCompletePanel.Back();
                }

                m_taskCompleteDriver = null;
            }
            else if (m_taskCompleteDriver != null && ShowTaskComplete)
            {
                PanelManager.Instance.Push(TaskCompletePanel, m_taskCompleteDriver, () =>
                {
                    m_taskCompleteDriver = null;

                    SyncPanels();
                });
            }
        }

        private void Tasks_Updated(object sender, EventArgs e)
        {
            ThreadHelper.Instance.CallExclusive(SyncPanels);
        }

        /// <summary>
        /// Clears previously set guide data.
        /// </summary>
        /// <param name="guideData"></param>
        public void ClearGuide(ARGuideData guideData)
        {
            if (GuidePanel && GuidePanel.Data == guideData)
            {
                m_guideData = null;

                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    GuidePanel.Back();
                });

                ThreadHelper.Instance.CallExclusive(SyncPanels);
            }
        }

        /// <summary>
        /// Sets an AR "guide" with title and arrows that direct the user to
        /// the AR item.
        /// </summary>
        /// <param name="guideData"></param>
        public void SetGuide(ARGuideData guideData)
        {
            m_guideData = guideData;

            ThreadHelper.Instance.CallExclusive(SyncPanels);
        }

        /// <summary>
        /// Sets the task complete panel for the given driver.
        /// </summary>
        /// <param name="taskDriver"></param>
        public void SetTaskComplete(IPlayerTaskDriver taskDriver)
        {
            m_taskCompleteDriver = taskDriver;

            ThreadHelper.Instance.CallExclusive(SyncPanels);
        }
    }
}