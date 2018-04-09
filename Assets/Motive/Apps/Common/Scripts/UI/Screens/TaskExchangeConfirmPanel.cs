// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using System;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Used to let the player confirm an exchange before completing an exchange task.
    /// </summary>
    public class TaskExchangeConfirmPanel : Panel<IPlayerTaskDriver>
    {
        public TaskItem TaskItem;

        bool m_confirm;

        public override void Populate(IPlayerTaskDriver data)
        {
            m_confirm = false;

            TaskItem.Populate(data);

            base.Populate(data);
        }

        public void Confirm()
        {
            m_confirm = true;

            Back();
        }

        public override void DidHide()
        {
            TaskItem.Reset();

            base.DidHide();
        }

        public void Show(IPlayerTaskDriver driver, Action onConfirm)
        {
            PanelManager.Instance.Push(this, driver, () =>
                {
                    if (m_confirm)
                    {
                        if (onConfirm != null)
                        {
                            onConfirm();
                        }
                    }
                });
        }
    }

}