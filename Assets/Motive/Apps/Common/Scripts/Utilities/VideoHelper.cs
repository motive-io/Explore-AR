// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using System;
using UnityEngine;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Helper class for video playback.
    /// </summary>
    public static class VideoHelper
    {
        public static void PlayFullScreen(MediaItem mediaItem, Action onClose = null)
        {
            if (mediaItem != null)
            {
                Screen.orientation = ScreenOrientation.AutoRotation;
                var localUrl = WebServices.Instance.MediaDownloadManager.GetPathForItem(mediaItem.Url);
                Handheld.PlayFullScreenMovie(localUrl);
            }
        }
    }

}