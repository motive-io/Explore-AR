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
    /// Displays character message info.
    /// </summary>
    public class CharacterMessageComponent : PanelComponent<CharacterMessage>
    {
        public Text CharacterName;
        public RawImage Image;
        public Text Text;

        public override void Populate(CharacterMessage charMsg)
        {
            var character = charMsg.Character;

            var imageUrl = charMsg.ImageUrl ??
                (character != null ? character.ImageUrl : null);

            ImageLoader.LoadImageOnThread(imageUrl, Image);

            if (character != null)
            {
                if (CharacterName)
                {
                    CharacterName.text = character.Alias;
                }
            }

            Text.text = charMsg.Text;

            PopulateComponent<MediaItemComponent>(Data.MediaItem);
        }

        public override void DidHide()
        {
            Text.text = null;

            if (Image.texture)
            {
                Image.texture = null;
            }

            base.DidHide();
        }
    }

}