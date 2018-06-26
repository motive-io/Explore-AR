// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Motive.Unity.Utilities;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays assignment item info (task or objective).
    /// </summary>
    public class AssignmentItemInfoComponent : PanelComponent<AssignmentItem>
    {
        public AssignmentItemInfo AssignmentItemInfo;

        protected override void Awake()
        {
            if (!AssignmentItemInfo)
            {
                AssignmentItemInfo = GetComponent<AssignmentItemInfo>();
            }

            base.Awake();
        }

        public override void Populate(AssignmentItem obj)
        {
            if (AssignmentItemInfo)
            {
                AssignmentItemInfo.Populate(obj);
            }

            PopulateComponents(obj.GetData());

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
                if (Data != null)
                {
                    Populate(Data);
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