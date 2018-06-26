// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    interface IARMediaPlaybackProperties : ISimplePlaybackProperties
    {
        bool OnlyPlayWhenVisible { get; }
    }
}
