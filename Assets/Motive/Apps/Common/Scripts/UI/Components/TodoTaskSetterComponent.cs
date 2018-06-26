// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Apps;
using Motive.Unity.Gaming;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Panel component that sets a task todo override.
    /// </summary>
    public class TodoTaskSetterComponent : PanelComponent<IPlayerTaskDriver>
    {
        public override void Populate(IPlayerTaskDriver obj)
        {
            TodoManager.Instance.OverrideTodoTask(obj);

            base.Populate(obj);
        }

        public override void DidHide()
        {
            TodoManager.Instance.ClearOverrideTodoTask(Data);

            base.DidHide();
        }
    }

}