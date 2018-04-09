// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Playables;

namespace Motive.Unity.Scripting
{
    public class PlayableContentBatchProcessor : ThreadSafeScriptResourceProcessor<PlayableContentBatch>
    {

        public override void ActivateResource(ResourceActivationContext context, PlayableContentBatch resource)
        {
            if (!context.IsClosed)
            {
                PlayableContentHandler.Instance.Play(context, resource);
            }
        }

        public override void DeactivateResource(ResourceActivationContext context, PlayableContentBatch resource)
        {
            PlayableContentHandler.Instance.StopPlaying(context);
        }
    }
}