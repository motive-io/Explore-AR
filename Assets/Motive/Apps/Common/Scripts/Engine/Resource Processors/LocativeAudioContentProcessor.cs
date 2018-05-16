// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Media;
using Motive.AR.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Scripting
{
    public class LocativeAudioContentProcessor : ScriptResourceProcessor<LocativeAudioContent>
    {
        public override void ActivateResource(ResourceActivationContext context, LocativeAudioContent resource)
        {
            if (LocativeAudioAnnotationHandler.Instance && resource.ShowOnMap)
            {
                LocativeAudioAnnotationHandler.Instance.AddLocativeAudio(context.InstanceId, resource);
            }

            LocativeAudioDriver.Instance.Activate(context.InstanceId, resource,
                () =>
                {
                    context.Open();
                },
                (success) =>
                {
                    context.Close();
                });
        }

        public override void DeactivateResource(ResourceActivationContext context, LocativeAudioContent resource)
        {
            if (LocativeAudioAnnotationHandler.Instance && resource.ShowOnMap)
            {
                LocativeAudioAnnotationHandler.Instance.RemoveLocativeAudio(context.InstanceId);
            }

            LocativeAudioDriver.Instance.Deactivate(context.InstanceId, resource);
        }

        public override void UpdateResource(ResourceActivationContext context, LocativeAudioContent resource)
        {
            if (LocativeAudioAnnotationHandler.Instance && resource.ShowOnMap)
            {
                LocativeAudioAnnotationHandler.Instance.RemoveLocativeAudio(context.InstanceId);
                LocativeAudioAnnotationHandler.Instance.AddLocativeAudio(context.InstanceId, resource);
            }

            LocativeAudioDriver.Instance.Update(context.InstanceId, resource);
        }
    }
}