using Motive.UI.Framework;
using Motive.Unity.Gaming;

namespace Motive.Unity.UI
{
    public class TaskItemActionButtonHandlerComponent : PanelComponent<IPlayerTaskDriver>
    {
        public override void Populate(IPlayerTaskDriver obj)
        {
            base.Populate(obj);
        }

        public void Action()
        {
            if (Data is ARTaskDriver)
            {
                ARQuestAppUIManager.Instance.SetARMode();
            }
            else
            {
                Data.Action();
            }
        }
    }
}