// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using Motive.Core.Media;
using Motive.UI.Framework;

namespace Motive.Unity.UI
{
    public class AudioVideoEmbeddedPanel : Panel
    {
        private ControlsPanel m_audioControlsPushedPanel;

        private Action m_didLoseTopAudio;
        private ControlsPanel m_videoControlsPushedPanel;

        public AudioSubpanel AudioContainer;
        public ControlsPanel AudioControls;

        public EmbeddedPanelContainer AudioControlsTarget;

        public bool UseFullScreen;
        public VideoSubpanel VideoContainer;

        public ControlsPanel VideoControls;
        public EmbeddedPanelContainer VideoControlsTarget;

        public void PlayAudio(MediaItem media)
        {
            AudioContainer.gameObject.SetActive(true);
            AudioContainer.Play(media);

            m_audioControlsPushedPanel = AudioControlsTarget.PushPanel(AudioControls) as ControlsPanel;

            if (m_audioControlsPushedPanel == null)
            {
                return;
            }

            VideoContainer.gameObject.SetActive(false);
            if (m_videoControlsPushedPanel)
            {
                m_videoControlsPushedPanel.SetActive(false);
            }

            m_audioControlsPushedPanel.gameObject.SetActive(true);
            m_audioControlsPushedPanel.Disable(typeof(VideoSubpanel));

            m_didLoseTopAudio = () => { m_audioControlsPushedPanel.AudioSubPanel.Stop(); };

            OnClose += m_didLoseTopAudio;
        }


        public void PlayVideo(MediaItem media)
        {
            // Sanity, video should be enabled. Audio disabled.
            VideoContainer.gameObject.SetActive(true);
            AudioContainer.gameObject.SetActive(false);

            VideoContainer.SetFullScreen(UseFullScreen);

            // Pass the ref to video controls so that it can rotate the video.
            if (VideoContainer.FullScreen)
            {
                VideoContainer.FullScreen.GetComponentInChildren<VideoControls>().VideoSubpanel = VideoContainer;
            }

            VideoContainer.Play(media, () => { VideoContainer.SetFullScreen(UseFullScreen); });

            m_videoControlsPushedPanel = VideoControlsTarget.PushPanel(VideoControls) as ControlsPanel;

            if (m_videoControlsPushedPanel == null)
            {
                return;
            }

            // Set the controls to on and disable the audio control functionality of the controls object.
            m_videoControlsPushedPanel.gameObject.SetActive(true);
            m_videoControlsPushedPanel.Disable(typeof(AudioSubpanel));

            if (!VideoContainer.FullScreen)
            {
                m_videoControlsPushedPanel.DisableFullScreen();
            }

            // HACK: toggle to get audio playing. Fix this asap.
            VideoContainer.gameObject.SetActive(false);
            VideoContainer.gameObject.SetActive(true);
        }

        private void Start()
        {
            VideoControls.VideoSubPanel = VideoContainer;
            AudioControls.AudioSubPanel = AudioContainer;
        }
    }

}