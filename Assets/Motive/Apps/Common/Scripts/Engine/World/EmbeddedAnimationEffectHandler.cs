// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Motive.Unity.World
{
    class EmbeddedAnimationEffectHandler : StackedAnimationHandler<GenericScriptObject>
    {
        private ListDictionary<string, AppliedEffect> m_worlObjectNamedAnimations = new ListDictionary<string, AppliedEffect>();

        private void StartAnimation(AppliedEffect appliedEffect)
        {
            var animName = ((GenericScriptObject)appliedEffect.Effect)["name"].ToString();
            var animTarget = appliedEffect.WorldObject.GetAnimationTarget();

            StartAnimationWithName(appliedEffect, animTarget, animName);
        }

        protected override void StartEffect(AppliedEffect appliedEffect, GenericScriptObject effect)
        {
            StartAnimation(appliedEffect);
        }

        protected override void StopEffect(AppliedEffect appliedEffect, GenericScriptObject effect)
        {
            var objAnim = appliedEffect.WorldObject.GetAnimationTarget().GetComponent<Animation>();

            if (objAnim)
            {
                objAnim.Stop();
            }
        }
    }
}
