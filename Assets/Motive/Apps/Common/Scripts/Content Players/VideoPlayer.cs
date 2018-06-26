// Copyright (c) 2018 RocketChicken Interactive Inc.
#if UNITY_5_6_no

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Motive.Core.Models;
using Motive.Core.Media;
using System;

public class VideoPlayer : MonoBehaviour
{
    public UnityEngine.Video.VideoPlayer Player;
    public RawImage PlayerSurface;

    public bool IsPlaying { get; private set; }

    bool m_tryingToLoad;
    bool m_loadedVideo;
    bool m_startedPlay;
    bool m_waitingPlay;
    bool m_waitingSetTexture;
    Action m_onComplete;

    void Start()
    {
#if UNITY_ANDROID
        if (Application.isMobilePlatform)
        {
            var rect = PlayerSurface.uvRect;

            rect.y = 0;
            rect.height = 1;

            PlayerSurface.uvRect = rect;
        }
#endif
    }

    public void Play()
    {
        IsPlaying = true;

        if (!m_loadedVideo)
        {
            m_loadedVideo = false;
            m_tryingToLoad = true;
            m_startedPlay = false;
            m_waitingPlay = false;

            Player.Load();
        }
        else
        {
            Player.Play();

            //PlayerSurface.texture = Player.GetVideoTexture();
        }
    }

    public void Play(MediaItem media, Action onComplete = null)
    {
        FinishPlaying();

        IsPlaying = true;

        m_loadedVideo = false;
        m_startedPlay = false;
        m_waitingPlay = false;

        if (media != null && media.Url != null)
        {
            var videoUrl = WebServices.Instance.MediaDownloadManager.GetSystemUrl(media.Url);

            Player.Load(videoUrl);
            PlayerSurface.texture = Player.GetVideoTexture();
        }
    }

    int m_txCt;

    // Update is called once per frame
    void Update()
    {
        if (!m_loadedVideo)
        {
            m_loadedVideo = !Player.IsLoading();

            if (m_loadedVideo)
            {
                Player.Play();
                m_waitingPlay = true;
            }
        }
        else
        {
            if (m_waitingPlay && Player.IsPlaying())
            {
                m_waitingPlay = false;
                m_startedPlay = true;
                m_waitingSetTexture = true;
            }

            var tex = Player.GetVideoTexture();

            if (m_waitingSetTexture && tex != null) //  && Player.GetCurrentPosition() > 0.01
            {
                PlayerSurface.texture = tex;
                m_waitingSetTexture = false;
            }
        }

        if (m_startedPlay && !Player.IsPlaying())
        {
            m_startedPlay = false;
            IsPlaying = false;

            FinishPlaying();
        }
    }

    private void FinishPlaying()
    {
        if (m_onComplete != null)
        {
            var toCall = m_onComplete;
            m_onComplete = null;

            toCall();
        }
    }

    public void Pause()
    {
        IsPlaying = false;
        Player.Pause();
    }

    public void Stop()
    {
        IsPlaying = false;
        Player.Pause();
        Player.SeekTo(0);
    }

    public void SetVideoPath(string path)
    {
        Player.videoPath = path;
    }
}

#endif