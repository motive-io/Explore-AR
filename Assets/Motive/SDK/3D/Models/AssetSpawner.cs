// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive._3D.Models
{
    public class AssetSpawner : ScriptObject
    {
        public AssetInstance AssetInstance { get; set; }
        public ScriptObject SpawnPosition { get; set; }
    }
}
