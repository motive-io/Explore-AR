// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Models;
using Motive.UI.Framework;
using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays playable audio.
    /// </summary>
    public class PlayableAudioPanel : PlayableMediaPanel
    {
        public UnityEvent PlaybackCompleted;

        protected override void Awake()
        {
            var component = GetComponent<AudioContentPlayerComponent>();

            if (component)
            {
                component.PlaybackCompleted.AddListener(() =>
                    {
                        if (PlaybackCompleted != null)
                        {
                            PlaybackCompleted.Invoke();
                        }
                    });
            }

            base.Awake();
        }
    }

}