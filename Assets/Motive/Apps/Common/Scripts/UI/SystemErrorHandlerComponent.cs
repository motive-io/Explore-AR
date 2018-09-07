// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using System;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class SystemErrorHandlerComponent : PanelComponent, ISystemErrorHandlerDelegate
    {
        public Text ErrorMessage;

        public override Panel Panel
        {
            set
            {
                base.Panel = value;

                SystemErrorHandler.Instance.Delegate = this;
            }
        }
        
        public void ReportError(string errorMessage)
        {
            ErrorMessage.text = errorMessage;

            PanelManager.Instance.Push(Panel);
        }

        public void ReportException(Exception x, string errorMessage = null)
        {
            ErrorMessage.text = x.ToString();

            PanelManager.Instance.Push(Panel);
        }
    }
}
