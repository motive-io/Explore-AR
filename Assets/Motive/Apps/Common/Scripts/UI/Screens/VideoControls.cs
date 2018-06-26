// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Controls for video playback.
    /// </summary>
    public class VideoControls : MonoBehaviour
    {
        public AspectRatioFitter AspectRatioFitter;
        public string PauseCharacter = "";
        public string PlayCharacter = "";
        public Button PlayPauseButton;
        public UserInputSlider Slider;

        public Text Time;

        public VideoSubpanel VideoSubpanel;

        public void ExitFullScreen()
        {
            VideoSubpanel.SetFullScreen(false);
        }

        public void PlayPause()
        {
            VideoSubpanel.PlayPause();
        }

        // Update is called once per frame
        private void Update()
        {
            var duration = VideoSubpanel.Duration;
            var posPct = VideoSubpanel.Position / VideoSubpanel.Duration;

            if (AspectRatioFitter)
            {
                AspectRatioFitter.aspectRatio = VideoSubpanel.AspectRatio;
            }

            if (VideoSubpanel == null || !(duration > 0))
            {
                return;
            }

            if (Slider && !Slider.IsUserUpdate)
            {
                Slider.value = duration > 0 ? posPct : 0;
            }

            if (Time)
            {
                var durationTs = TimeSpan.FromSeconds(duration);
                var posTs = TimeSpan.FromSeconds(duration * posPct);

                Time.text = string.Format("{0}  /  {1}",
                    StringFormatter.FormatTimespan(posTs),
                    StringFormatter.FormatTimespan(durationTs));
            }

            if (PlayPauseButton)
            {
                PlayPauseButton.GetComponentInChildren<Text>().text =
                    VideoSubpanel.IsPlaying ? PauseCharacter : PlayCharacter;
            }
        }

        public void UpdatePosition()
        {
            if (Slider.IsUserUpdate)
            {
                VideoSubpanel.UpdatePosition(Slider.value);
            }
        }
    }

}