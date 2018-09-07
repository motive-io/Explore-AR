// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Core.Models;
using Motive.Core.Globalization;
using Motive.Core.Media;
using System.Collections.Generic;

namespace Motive._3D.Models
{
    class ScriptedParticleEffect : ScriptObject, IMediaItemProvider
    {
        public float? RateOverTime { get; set; }

        public int? MaxParticles { get; set; }

        public Color Color { get; set; }

        public float? ParticleSize { get; set; }

        public bool Loop { get; set; }

        public LocalizedMedia LocalizedTextureImage { get; set; }

        public string ImageUrl
        {
            get
            {
                return LocalizedMedia.GetMediaUrl(LocalizedTextureImage);
            }
        }

        public MediaItem MediaItem
        {
            get
            {
                return LocalizedMedia.GetMediaItem(this.LocalizedTextureImage);
            }
        }

        public virtual void GetMediaItems(IList<MediaItem> items)
        {
            if (this.LocalizedTextureImage != null)
            {
                this.LocalizedTextureImage.GetMediaItems(items);
            }
        }
    }
}
