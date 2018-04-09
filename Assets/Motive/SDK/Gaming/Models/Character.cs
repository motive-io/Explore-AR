// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Globalization;
using Motive.Core.Media;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Represents an NPC.
    /// </summary>
    public class Character : ScriptObject, IMediaItemProvider
    {
        public LocalizedMedia LocalizedProfileImage { get; set; }
        public LocalizedText LocalizedFirstName { get; set; }
        public LocalizedText LocalizedLastName { get; set; }
        public LocalizedText LocalizedAlias { get; set; }

        public MediaItem ProfileImage
        {
            get
            {
                return LocalizedMedia.GetMediaItem(LocalizedProfileImage);
            }
        }
        public string FirstName
        {
            get
            {
                return LocalizedText.GetText(LocalizedFirstName);
            }
        }

        public string LastName
        {
            get
            {
                return LocalizedText.GetText(LocalizedLastName);
            }
        }

        public string Alias
        {
            get
            {
                return LocalizedText.GetText(LocalizedAlias);
            }
        }

        public string ImageUrl
        {
            get
            {
                if (ProfileImage != null) return ProfileImage.Url;

                return null;
            }
        }

        public string Name
        {
            get
            {
                return Alias;
            }
        }

        public void GetMediaItems(IList<MediaItem> items)
        {
            if (LocalizedProfileImage != null)
            {
                LocalizedProfileImage.GetMediaItems(items);
            }
        }
    }
}