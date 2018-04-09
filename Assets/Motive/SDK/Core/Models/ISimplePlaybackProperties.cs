// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Core.Models
{
    /// <summary>
    /// Applied to script objects that support simple playback.
    /// </summary>
    public interface ISimplePlaybackProperties
    {
        bool Loop { get; }
        float Volume { get; }
    }
}
