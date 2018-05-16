// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using Motive.Core.Media;
using Motive.Core.Models;
using Motive.UI.Framework;
using Motive.Unity.Media;
using Motive.Unity.Playables;
using System;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// General purpose panel for displaying text with optional connected audio.
    /// </summary>
    public class TextMediaPopupPanel : Panel<ITextMediaContent>
    {
        public Text Text;

        private IAudioPlayer m_player;

        public override void Populate(ITextMediaContent data)
        {
            StopPlayer();

            Text.text = data.Text;

            base.Populate(data);

            if (data.MediaItem != null && data.MediaItem.MediaType == Motive.Core.Media.MediaType.Audio)
            {
                var localUrl = WebServices.Instance.MediaDownloadManager.GetPathForItem(data.MediaItem.Url);

                m_player = Platform.Instance.ForegroundAudioChannel.CreatePlayer(new Uri(localUrl));

                m_player.Play();

                AudioContentPlayer.Instance.Pause();
            }
        }

        void StopPlayer()
        {
            if (m_player != null)
            {
                m_player.Stop();
                m_player.Dispose();

                m_player = null;
            }
        }

        public override void DidPop()
        {
            StopPlayer();

            AudioContentPlayer.Instance.Resume();

            base.DidPop();
        }
    }

}