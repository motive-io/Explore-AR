// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.UI.Framework;
using Motive.UI.Models;
using UnityEngine.UI;
using Motive.Unity.Timing;
using Motive.Unity.Media;
using System;
using Motive;
using Motive.Unity.Utilities;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays a notification playable.
    /// </summary>
    public class NotificationPanel : Panel<ResourcePanelData<Notification>>
    {
        /// <summary>
        /// If true, always vibrate the device when a notification is received.
        /// </summary>
        public bool Vibrate;

        public RawImage Image;
        public Text ImageTitle;
        public Text ImageMessage;
        public Text NoImageTitle;
        public Text NoImageMessage;
        public float DismissTime = 10f;
        public bool AutoDismiss;

        public GameObject ImagePane;
        public GameObject NoImagePane;

        UnityTimer m_timer;

        void CancelTimer()
        {
            if (m_timer != null)
            {
                m_timer.Cancel();
                m_timer = null;
            }
        }

        public override void Populate(ResourcePanelData<Notification> data)
        {
            CancelTimer();

            base.Populate(data);

            var imageUrl = data.Resource.ImageUrl;

            if (ImagePane)
            {
                ImagePane.SetActive(imageUrl != null);
            }

            if (NoImagePane)
            {
                NoImagePane.SetActive(imageUrl == null);
            }

            Text titleText;
            Text messageText;

            // Supports different layout for notifications with and without images.
            if (imageUrl != null && Image)
            {
                titleText = ImageTitle ?? NoImageTitle;
                messageText = ImageMessage ?? NoImageMessage;

                ImageLoader.LoadImageOnThread(imageUrl, Image);
            }
            else
            {
                titleText = NoImageTitle ?? ImageTitle;
                messageText = NoImageMessage ?? ImageMessage;
            }

            if (titleText)
            {
                titleText.text = data.Resource.Title;
            }

            if (messageText)
            {
                messageText.text = data.Resource.Message;
            }

            if (Vibrate || data.Resource.Vibrate)
            {
                Handheld.Vibrate();
            }

            if (Data.Resource.Sound != null)
            {
                var localUrl = WebServices.Instance.MediaDownloadManager.GetPathForItem(Data.Resource.Sound.Url);

                Platform.Instance.ForegroundAudioChannel.Play(new Uri(localUrl));
            }

            if (AutoDismiss)
            {
                m_timer = UnityTimer.Call(DismissTime, Back);
            }
        }

        public override void DidHide()
        {
            CancelTimer();

            if (Image)
            {
                Image.texture = null;
            }

            base.DidHide();
        }
    }

}