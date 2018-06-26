// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Core.Scripting;
using Motive.Unity.AR;
using Motive.Unity.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Scripting
{
    public class AugmentedMediaProcessor : ThreadSafeScriptResourceProcessor<AugmentedMedia>
    {
        public override void ActivateResource(ResourceActivationContext context, AugmentedMedia resource)
        {
            ARWorld.Instance.AddAugmentedMedia(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, AugmentedMedia resource)
        {
            ARWorld.Instance.RemoveAugmentedMedia(context.InstanceId);
        }
    }
}