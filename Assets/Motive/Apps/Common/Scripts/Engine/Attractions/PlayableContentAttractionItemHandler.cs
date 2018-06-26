// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Attractions.Models;
using Motive.Core.Models;
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Playables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Apps.Attractions
{
    public class PlayableContentAttractionItemHandler
: AttractionItemHandler<LocationAttractionContent, IContent>
    {
        public PlayableContentAttractionItemHandler(
            ResourceActivationContext ctxt, LocationAttractionContent resource)
            : base(ctxt, resource, resource.Content)
        {
            ctxt.FiredEvent += ctxt_FiredEvent;
        }

        void ctxt_FiredEvent(object sender, EventFiredArgs e)
        {
            if (e.EventName == "close")
            {
                ActivationContext.FireEvent("complete");
            }
        }

        public override void Activate(LocationAttraction attraction, bool autoplay)
        {
            // Activate (with autoplay)
            if (autoplay && Item != null)
            {
                var playable = new PlayableContent
                {
                    Content = Item,
                    Route = PlayableContentRoute.Screen
                };

                PlayableContentHandler.Instance.Play(ActivationContext, playable);
            }
        }

        public override void Deactivate(LocationAttraction attraction)
        {
            // No-op?
            // PlayableContentHandler.Instance.StopPlaying(ActivationContext);
        }

        public override void DeactivateAll()
        {
            PlayableContentHandler.Instance.StopPlaying(ActivationContext);
        }
    }

}