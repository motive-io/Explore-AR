// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Unity.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Scripting;
using Motive.Unity.AR;

namespace Motive.Unity.Scripting
{
    public class LocationAugmentedImageProcessor : ThreadSafeScriptResourceProcessor<LocationAugmentedImage>
    {
        public override void ActivateResource(ResourceActivationContext context, LocationAugmentedImage resource)
        {
            if (ARWorld.Instance)
            {
                ARWorld.Instance.AddLocationAugmentedImage(context, resource);
            }
        }

        public override void DeactivateResource(ResourceActivationContext context, LocationAugmentedImage resource)
        {
            if (ARWorld.Instance)
            {
                ARWorld.Instance.RemoveLocationAugmentedImage(context, resource);
            }
        }
    }
}