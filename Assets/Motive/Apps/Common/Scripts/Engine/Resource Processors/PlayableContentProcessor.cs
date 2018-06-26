// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Playables;

namespace Motive.Unity.Scripting
{
    public class PlayableContentProcessor : ScriptResourceProcessor<PlayableContent>
    {

        public override void ActivateResource(ResourceActivationContext context, PlayableContent resource)
        {
            PlayableContentHandler.Instance.Play(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, PlayableContent resource)
        {
            PlayableContentHandler.Instance.StopPlaying(context);
        }
    }
}