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
    public class LocationAugmentedMediaProcessor : ThreadSafeScriptResourceProcessor<LocationAugmentedMedia>
    {
        public override void ActivateResource(ResourceActivationContext context, LocationAugmentedMedia resource)
        {
            ARWorld.Instance.AddLocationAugmentedMedia(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, LocationAugmentedMedia resource)
        {
            ARWorld.Instance.RemoveAugmentedMedia(context.InstanceId);
        }
    }
}