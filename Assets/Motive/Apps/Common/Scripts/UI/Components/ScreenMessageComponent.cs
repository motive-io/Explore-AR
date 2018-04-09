// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.UI.Framework;
using Motive.Unity.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays screen message information.
    /// </summary>
    public class ScreenMessageComponent : PanelComponent<ScreenMessage>
    {
        public Text Title;
        public GameObject TitleLayoutObject;
        public Text Subtitle;
        public GameObject SubtitleLayoutObject;
        public Text Text;
        public GameObject TextLayoutObject;

        public Button SelectButton;

        public RawImage Image;
        public GameObject ImageLayoutObject;

        string m_defaultButtonText;

        protected override void Awake()
        {
            if (SelectButton)
            {
                var text = SelectButton.GetComponentInChildren<Text>();

                if (text)
                {
                    m_defaultButtonText = text.text;
                }
            }

            base.Awake();
        }

        public override void Populate(ScreenMessage screenMessage)
        {
            ImageLoader.LoadImageOnThread(screenMessage.ImageUrl, Image, () =>
            {
                PopulateComponent<MediaItemComponent>(screenMessage.MediaItem);
            });

            if (ImageLayoutObject)
            {
                ImageLayoutObject.SetActive(screenMessage.ImageUrl != null);
            }

            if (Text)
            {
                Text.text = screenMessage.Text;

                if (TextLayoutObject)
                {
                    TextLayoutObject.SetActive(!string.IsNullOrEmpty(screenMessage.Text));
                }
            }

            if (SelectButton)
            {
                var text = SelectButton.GetComponentInChildren<Text>();

                if (text)
                {
                    if (string.IsNullOrEmpty(screenMessage.ButtonText))
                    {
                        text.text = m_defaultButtonText;
                    }
                    else
                    {
                        text.text = screenMessage.ButtonText;
                    }
                }
            }

            if (Title) Title.text = screenMessage.Title;

            if (TitleLayoutObject)
            {
                TitleLayoutObject.SetActive(!string.IsNullOrEmpty(screenMessage.Title));
            }

            if (Subtitle) Subtitle.text = screenMessage.Subtitle;

            if (SubtitleLayoutObject)
            {
                SubtitleLayoutObject.SetActive(!string.IsNullOrEmpty(screenMessage.Subtitle));
            }
        }

        public override void DidHide()
        {
            if (Text)
            {
                Text.text = null;
            }

            if (Image)
            {
                //Destroy(Image.texture);
                Image.texture = null;
            }

            base.DidHide();
        }
    }

}