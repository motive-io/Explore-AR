// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays video content in a VideoSubpanel.
    /// </summary>
    public class VideoContentPlayerComponent : MediaContentComponent
    {
        public VideoSubpanel VideoSubpanel;

        public UnityEvent PlaybackCompleted;

        public bool UseFullScreen;

        bool m_didFadeIn;
        //    bool m_didShowInfo;
        //Logger m_logger;

        protected override void Awake()
        {
            base.Awake();

            VideoSubpanel = GetComponentInChildren<VideoSubpanel>();
            VideoSubpanel.PlaybackCompleted.AddListener(m_videoSubpanel_PlaybackCompleted);

            if (PlaybackCompleted == null)
            {
                PlaybackCompleted = new UnityEvent();
            }

            //m_logger = new Logger(this);
        }

        void m_videoSubpanel_PlaybackCompleted()
        {
            if (PlaybackCompleted != null)
            {
                PlaybackCompleted.Invoke();
            }
        }

        public override void Populate(MediaContent data)
        {
            VideoSubpanel.SetFullScreen(UseFullScreen);
            VideoSubpanel.Play(data.MediaItem);
        }

        public override void DidHide()
        {
            VideoSubpanel.Close();

            base.DidHide();
        }
    }
}