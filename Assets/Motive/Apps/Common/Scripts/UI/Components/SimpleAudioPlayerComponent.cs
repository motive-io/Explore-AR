// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using Motive.Core.Media;
using Motive.Unity.Media;
using Motive.Unity.Playables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// A panel component for panels with media items for playing audio from a media item.
    /// Optionally autoplays.
    /// </summary>
    public class SimpleAudioPlayerComponent : MediaItemComponent
    {
        public bool Autoplay;

        private IAudioPlayer m_player;

        void StopPlayer()
        {
            if (m_player != null)
            {
                m_player.Stop();
                m_player.Dispose();

                m_player = null;
            }
        }

        public override void Populate(Motive.Core.Media.MediaItem obj)
        {
            StopPlayer();

            if (obj != null && obj.MediaType == Motive.Core.Media.MediaType.Audio)
            {
                var localUrl = WebServices.Instance.MediaDownloadManager.GetPathForItem(obj.Url);

                m_player = Platform.Instance.ForegroundAudioChannel.CreatePlayer(new Uri(localUrl));

                if (Autoplay)
                {
                    Play();
                }
            }
        }

        public void Play()
        {
            if (m_player != null)
            {
                m_player.Play((success) =>
                {
                    if (AudioContentPlayer.Instance)
                    {
                        AudioContentPlayer.Instance.Resume();
                    }
                });

                if (AudioContentPlayer.Instance)
                {
                    AudioContentPlayer.Instance.Pause();
                }
            }
        }

        public override void DidShow()
        {
            //StopPlayer();
        }

        public override void DidHide()
        {
            StopPlayer();

            if (AudioContentPlayer.Instance)
            {
                AudioContentPlayer.Instance.Resume();
            }

            base.DidHide();
        }
    }
}