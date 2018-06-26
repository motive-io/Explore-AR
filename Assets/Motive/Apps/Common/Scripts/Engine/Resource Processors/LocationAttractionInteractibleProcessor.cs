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
    public class LocationAttractionInteractibleProcessor : ScriptResourceProcessor<LocationAttractionInteractible>
    {
        public override void ActivateResource(ResourceActivationContext context, LocationAttractionInteractible resource)
        {
            AttractionManager.Instance.ActivateInteractible(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, LocationAttractionInteractible resource)
        {
            AttractionManager.Instance.DeactivateInteractible(context, resource);
        }
    }
}