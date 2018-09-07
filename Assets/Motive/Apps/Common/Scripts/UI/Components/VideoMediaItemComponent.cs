using System.Collections;
using System.Collections.Generic;
using Motive.Core.Media;
using UnityEngine;
using UnityEngine.Events;

namespace Motive.Unity.UI
{
    public class VideoMediaItemComponent : MediaItemComponent
    {
        public VideoSubpanel VideoSubpanel;
        public VideoControls VideoControls;

        public UnityEvent PlaybackCompleted = new UnityEvent();

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
        }

        void m_videoSubpanel_PlaybackCompleted()
        {
            if (PlaybackCompleted != null)
            {
                PlaybackCompleted.Invoke();
            }
        }

        public override void Populate(MediaItem data)
        {
            if (data.MediaType == MediaType.Video)
            {
                VideoSubpanel.Play(data);

                PopulateComponents(VideoSubpanel);

                if (UseFullScreen && VideoControls)
                {
                    VideoControls.EnterFullScreen();
                }
            }   
            else
            {
                HideLinkedObjects();
            }
        }

        public override void DidHide()
        {
            VideoSubpanel.Close();

            base.DidHide();
        }
    }
}