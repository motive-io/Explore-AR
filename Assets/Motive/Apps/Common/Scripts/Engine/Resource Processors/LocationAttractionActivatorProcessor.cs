// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Attractions.Models;
using Motive.Core.Scripting;
using Motive.Unity.Apps.Attractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Scripting
{
    public class LocationAttractionActivatorProcessor : ScriptResourceProcessor<LocationAttractionActivator>
    {
        public override void ActivateResource(ResourceActivationContext context, LocationAttractionActivator resource)
        {
            AttractionManager.Instance.ActivateAttractions(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, LocationAttractionActivator resource)
        {
            AttractionManager.Instance.DeactivateAttractions(context, resource);
        }
    }

}