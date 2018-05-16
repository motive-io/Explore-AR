// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Media;
using Motive.AR.Models;
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Motive.AR.Media
{
    public class LocativeAudioDriver : Singleton<LocativeAudioDriver>
    {
        public LocativeAudioEngine Engine { get; private set; }

        public LocativeAudioDriver()
        {
            Engine = new LocativeAudioEngine(
                Platform.Instance.CreateAudioPlayerChannel(),
                Platform.Instance.CreateSpatialAudioPlayerChannel(),
                WebServices.Instance.MediaDownloadManager)
            {
                DefaultInnerFadeInDuration = TimeSpan.FromSeconds(5),
                DefaultInnerFadeOutDuration = TimeSpan.FromSeconds(10),
                DefaultOuterFadeInDuration = TimeSpan.FromSeconds(5),
                DefaultOuterFadeOutDuration = TimeSpan.FromSeconds(10)
            };
        }

        public void Update(string instanceId, LocativeAudioContent locAudio)
        {
            Engine.Update(instanceId, locAudio);
        }

        public void Activate(string instanceId, LocativeAudioContent locAudio, Action onPlay = null, Action<bool> onComplete = null)
        {
            //MapController.Instance.AddLocativeSoundAnnotation(locAudio);

            Engine.Activate(instanceId, locAudio, onPlay, onComplete);
        }

        public void Deactivate(string instanceId, LocativeAudioContent locAudio)
        {
            //MapController.Instance.RemoveLocativeSoundAnnotation(locAudio);

            Engine.Deactivate(instanceId, locAudio);
        }

        public void ActivateSettings(LocativeAudioSettings settings)
        {
            if (settings.DefaultInnerFadeSettings != null)
            {
                var innerFade = settings.DefaultInnerFadeSettings;

                Engine.DefaultInnerFadeInDuration = innerFade.FadeInDuration.GetValueOrDefault(Engine.DefaultInnerFadeInDuration);
                Engine.DefaultInnerFadeOutDuration = innerFade.FadeOutDuration.GetValueOrDefault(Engine.DefaultInnerFadeOutDuration);
            }

            if (settings.DefaultOuterFadeSettings != null)
            {
                var outerFade = settings.DefaultOuterFadeSettings;

                Engine.DefaultOuterFadeInDuration = outerFade.FadeInDuration.GetValueOrDefault(Engine.DefaultOuterFadeInDuration);
                Engine.DefaultOuterFadeOutDuration = outerFade.FadeOutDuration.GetValueOrDefault(Engine.DefaultOuterFadeOutDuration);
            }
        }

        public void Start()
        {
            Engine.Start();
        }

        public void Stop()
        {
            Engine.Stop();
        }
    }
}