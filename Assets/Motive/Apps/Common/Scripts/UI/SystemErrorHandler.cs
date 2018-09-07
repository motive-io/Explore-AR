// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity
{
    public interface ISystemErrorHandlerDelegate
    {
        void ReportError(string errorMessage);
        void ReportException(Exception x, string errorMessage = null);
    }

    public class SystemErrorHandler : Singleton<SystemErrorHandler>
    {
        public ISystemErrorHandlerDelegate Delegate { get; set; }

        public void ReportError(string error, params object[] args)
        {
            ReportError(string.Format(error, args));
        }

        public void ReportError(string errorMessage)
        {
            if (Delegate != null)
            {
                Delegate.ReportError(errorMessage);
            }
        }

        public void ReportException(Exception x, string errorMessage = null)
        {
            if (Delegate != null)
            {
                Delegate.ReportException(x, errorMessage);
            }
        }
    }
}
