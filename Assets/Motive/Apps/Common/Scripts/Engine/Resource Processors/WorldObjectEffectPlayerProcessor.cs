// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D.Models;
using Motive.Unity.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Motive.Core.Scripting;
using Motive.Unity.World;

namespace Motive.Unity.Scripting
{
    public class WorldObjectEffectPlayerProcessor : ThreadSafeScriptResourceProcessor<WorldObjectEffectPlayer>
    {
        public override void ActivateResource(ResourceActivationContext context, WorldObjectEffectPlayer resource)
        {
            WorldObjectManager.Instance.ApplyEffects(context, resource);
        }

        public override void DeactivateResource(ResourceActivationContext context, WorldObjectEffectPlayer resource)
        {
            WorldObjectManager.Instance.RemoveEffects(context, resource);
        }
    }
}