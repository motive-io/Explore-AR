using Motive.Core.Scripting;
using Motive.UI.Models;
using System;

namespace Motive.Unity.UI
{
    public class UIModeSwitchCommandHandler : InterfaceCommandHandler<UIModeSwitchCommand>
    {
        private void Awake()
        {
            RegisterType("motive.ui.uiModeSwitchCommand");
        }

        public override void ProcessInterfaceCommand(ResourceActivationContext context, UIModeSwitchCommand command, Action onComplete)
        {
            switch (command.Mode)
            {
                case "map":
                    ARQuestAppUIManager.Instance.SetMapMode();
                    break;
                case "ar":
                    ARQuestAppUIManager.Instance.SetARMode();
                    break;
            }

            onComplete();
        }
    }
}