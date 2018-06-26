// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.AR.Models
{
    public class LocationValuablesCollectionOptions : ScriptObject
    {
        public bool? IsRespawnable { get; set; }
        public DoubleRange CollectRange { get; set; }
        public ILocationCollectionMechanic[] CollectionMechanics { get; set; }

        public bool TryGetMechanic<T>(out T mechanic)
            where T : ILocationCollectionMechanic
        {
            if (CollectionMechanics != null)
            {
                foreach (var m in CollectionMechanics)
                {
                    if (m != null && m.GetType() == typeof(T))
                    {
                        mechanic = (T)m;
                        return true;
                    }
                }
            }

            mechanic = default(T);

            return false;
        }
    }
}
