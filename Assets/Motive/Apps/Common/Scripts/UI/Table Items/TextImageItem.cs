// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Common base class for many table items that display text and an image.
    /// </summary>
    public class TextImageItem : SelectableTableItem
    {
        public Text Text;
        public RawImage Image;

        public GameObject TextLayoutObject;
        public GameObject ImageLayoutObject;

        public void SetText(Text textObj, string value)
        {
            if (textObj)
            {
                textObj.text = value;
            }
        }

        public void SetImage(RawImage imageObj, string imageUrl)
        {
            if (imageObj && imageUrl != null)
            {
                ImageLoader.LoadImageOnThread(imageUrl, imageObj);
            }
        }

        public void SetText(string text)
        {
            SetText(Text, text);

            if (TextLayoutObject)
            {
                TextLayoutObject.SetActive(!string.IsNullOrEmpty(text));
            }
        }

        public void SetImage(string imageUrl)
        {
            SetImage(Image, imageUrl);

            if (ImageLayoutObject)
            {
                ImageLayoutObject.SetActive(!string.IsNullOrEmpty(imageUrl));
            }
        }
    }

}