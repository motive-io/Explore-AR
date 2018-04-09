// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Common base class for text media response items (e.g., for character/
    /// screen dialogs).
    /// </summary>
    public class TextMediaResponseItem : TextImageItem
    {
        public Button Button;

        public virtual void Populate(TextMediaResponse response)
        {
            SetText(response.Text);
            SetImage(response.ImageUrl);
        }
    }

}