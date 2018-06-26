// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Apps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Scripting
{
    public class SavePointProcessor : ScriptResourceProcessor<SavePoint>
    {
        public override void ActivateResource(ResourceActivationContext context, SavePoint resource)
        {
            if (!context.IsClosed)
            {
                var time = context.Retrieve<string>();

                if (time == null)
                {
                    context.Store(DateTime.Now.ToString());

                    SavePointManager.Instance.ActivateSavePoint(context, resource);
                }

                context.Close();
            }

            base.ActivateResource(context, resource);
        }
    }
}