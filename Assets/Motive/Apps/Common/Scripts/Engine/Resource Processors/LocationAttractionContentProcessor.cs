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
    public class LocationAttractionContentProcessor : ScriptResourceProcessor<LocationAttractionContent>
    {
        public override void ActivateResource(ResourceActivationContext context, LocationAttractionContent resource)
        {
            AttractionManager.Instance.ActivateContent(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, LocationAttractionContent resource)
        {
            AttractionManager.Instance.DeactivateContent(context, resource);
        }
    }
}