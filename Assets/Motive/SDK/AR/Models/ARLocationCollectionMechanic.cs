// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Unity.AR;

namespace Motive.AR.Models
{
    public class ARLocationCollectionMechanic : ScriptObject, ILocationCollectionMechanic
    {
        public bool ShowMapAnnotation { get; set; }

		public LocationAugmentedOptions AROptions { get; set; }
    }
}
