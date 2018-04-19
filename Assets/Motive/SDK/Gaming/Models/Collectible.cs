// Copyright (c) 2018 RocketChicken Interactive Inc.

using Motive._3D.Models;
using Motive.Core.Globalization;
using Motive.Core.Models;
using Motive.Core.Scripting;
using System.Collections.Generic;
using System.Linq;

namespace Motive.Gaming.Models
{
    /// <summary>
    /// Represents an item that can be collected. Collectibles end up in the player's Inventory.
    /// </summary>
    public class Collectible : ScriptObject, IMediaItemProvider, IActiveEntity
    {
        public string[] Attributes { get; set; }
        /// <summary>
        /// A localized image representing the collectible.
        /// </summary>
        public LocalizedMedia LocalizedImage { get; set; }
        /// <summary>
        /// A localized string representing the collectible.
        /// </summary>
        public LocalizedText LocalizedTitle { get; set; }
        /// <summary>
        /// A localized string describing the collectible.
        /// </summary>
        public LocalizedText LocalizedDescription { get; set; }
        /// <summary>
        /// Returns the title for the current localization.
        /// </summary>
        public string Title { get { return LocalizedText.GetText(LocalizedTitle); } }
        /// <summary>
        /// Returns the description for the current localization.
        /// </summary>
        public string Description { get { return LocalizedText.GetText(LocalizedDescription); } }
        /// <summary>
        /// Optional order for the item in inventory.
        /// </summary>
        public int? InventoryOrder { get; set; }
        /// <summary>
        /// Optional path for folder-based inventory.
        /// </summary>
        public string InventoryPath { get; set; }

        public AssetInstance AssetInstance { get; set; }

        /// <summary>
        /// Actions that this collectible "emits". Can interact with other
        /// IActiveEntity objects through ActiveEntityManager.
        /// </summary>
        public EntityActionBehaviour[] EmittedActions { get; set; }
        /// <summary>
        /// Actions that this collectible "receives". Can interact with other
        /// IActiveEntity objects ActiveEntityManager.
        /// </summary>
        public EntityActionBehaviour[] ReceivedActions { get; set; }

        /// <summary>
        /// Returns an image URL for the current localization.
        /// </summary>
        public string ImageUrl
        {
            get
            {
                var mediaItem = LocalizedMedia.GetMediaItem(LocalizedImage);

                if (mediaItem != null && mediaItem.MediaType == Motive.Core.Media.MediaType.Image)
                {
                    return mediaItem.Url;
                }

                return null;
            }
        }

        /// <summary>
        /// An array of story tags for this collectible.
        /// </summary>
        public string[] StoryTags { get; set; }

        /// <summary>
        /// An array of attachments for this collectible. The use of these is up to
        /// the game implementation.
        /// </summary>
        public IContent[] Attachments { get; set; }

        /// <summary>
        /// Convenience method to return the first attachment.
        /// </summary>
        public IContent Content
        {
            get
            {
                return (Attachments != null) ?
                    Attachments.FirstOrDefault() : null;
            }
        }

        public bool IsSingleton
        {
            get
            {
                return HasAttribute("singleton");
            }
        }

        public bool IsArchive
        {
            get
            {
                return HasAttribute("archive");
            }
        }

        public Collectible()
        {
        }

        public void GetMediaItems(IList<Motive.Core.Media.MediaItem> items)
        {
            if (LocalizedImage != null)
            {
                LocalizedImage.GetMediaItems(items);
            }

            if (AssetInstance != null && AssetInstance is IMediaItemProvider)
            {
                ((IMediaItemProvider)AssetInstance).GetMediaItems(items);
            }

            if (Attachments != null)
            {
                foreach (var content in Attachments)
                {
                    var mediaProvider = content as IMediaItemProvider;

                    if (mediaProvider != null)
                    {
                        mediaProvider.GetMediaItems(items);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if this collectible has a particular story tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool HasTag(string tag)
        {
            return StoryTags != null && StoryTags.Contains(tag);
        }

        public bool HasAnyAttribute(IEnumerable<string> attributes)
        {
            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    if (HasAttribute(attr))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool HasAttribute(string attribute)
        {
            if (Attributes != null)
            {
                return Attributes.Contains(attribute);
            }

            return false;
        }

        public bool HasAttributesOr(IEnumerable<string> attributes)
        {
            if (Attributes != null)
            {
                foreach (var attr in attributes)
                {
                    if (Attributes.Contains(attr))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}