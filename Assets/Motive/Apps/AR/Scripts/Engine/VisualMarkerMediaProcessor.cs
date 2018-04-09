// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Models;
using Motive.Unity.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Scripting;

#if MOTIVE_VUFORIA
using Motive.AR.Vuforia;
#endif

namespace Motive.Unity.Scripting
{
    public class VisualMarkerMediaProcessor : ThreadSafeScriptResourceProcessor<VisualMarkerMedia>
    {
        public override void ActivateResource(ResourceActivationContext context, VisualMarkerMedia resource)
        {
#if MOTIVE_VUFORIA
        if (VuforiaWorld.Instance != null)
        {
            VuforiaWorld.Instance.AddMarkerMedia(context, resource);
        }
#endif
        }

        public override void DeactivateResource(ResourceActivationContext context, VisualMarkerMedia resource)
        {
#if MOTIVE_VUFORIA
        if (VuforiaWorld.Instance != null)
        {
            VuforiaWorld.Instance.RemoveMarkerMedia(context, resource);
        }
#endif
        }
    }
}