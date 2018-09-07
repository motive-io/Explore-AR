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
        public VideoControls VideoControls;

        public UnityEvent PlaybackCompleted;

        public bool UseFullScreen;

        bool m_didFadeIn;
        //    bool m_didShowInfo;
        //Logger m_logger;

        protected override void Awake()
        {
            base.Awake();

            if (!VideoSubpanel)
            {
                VideoSubpanel = GetComponentInChildren<VideoSubpanel>();
            }

            VideoSubpanel.PlaybackCompleted.AddListener(m_videoSubpanel_PlaybackCompleted);

            if (!VideoControls)
            {
                VideoControls = GetComponentInChildren<VideoControls>();
            }

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
            VideoSubpanel.Play(data.MediaItem);

            PopulateComponents(VideoSubpanel);

            if (UseFullScreen && VideoControls)
            {
                VideoControls.EnterFullScreen();
            }

            base.Populate(data);
        }

        public override void DidHide()
        {
            VideoSubpanel.Close();

            base.DidHide();
        }
    }
}