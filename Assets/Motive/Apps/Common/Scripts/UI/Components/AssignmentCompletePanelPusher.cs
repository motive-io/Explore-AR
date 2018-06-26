// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Automatically pushes the specified panel when an assignment is complete.
    /// </summary>
    public class AssignmentCompletePanelPusher : PanelComponent<AssignmentDriver>
    {
        public PanelLink AssignmentCompletePanel;

        public override void Populate(AssignmentDriver obj)
        {
            if (AssignmentCompletePanel)
            {
                if (obj.IsComplete)
                {
                    AssignmentCompletePanel.Push(obj);
                }
                else
                {
                    AssignmentCompletePanel.Back();
                }
            }

            base.Populate(obj);
        }

        public override void DidShow()
        {
            TaskManager.Instance.Updated += Tasks_Updated;
            base.DidShow();
        }

        private void Tasks_Updated(object sender, System.EventArgs e)
        {
            ThreadHelper.Instance.CallOnMainThread(() =>
            {
                if (Data == null || Data.ActivationContext.IsClosed)
                {
                    if (AssignmentCompletePanel)
                    {
                        AssignmentCompletePanel.Back();
                    }
                }
            });
        }

        public override void DidHide()
        {
            TaskManager.Instance.Updated -= Tasks_Updated;

            base.DidHide();
        }
    }
}