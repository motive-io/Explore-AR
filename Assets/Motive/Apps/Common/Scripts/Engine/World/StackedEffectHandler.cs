// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.World
{
    abstract class StackedEffectHandler<T> : IWorldObjectEffectHandler
            where T : IScriptObject
    {
        private ListDictionary<string, AppliedEffect> m_appliedEffects = new ListDictionary<string, AppliedEffect>();

        public virtual void ApplyEffect(AppliedEffect appliedEffect)
        {
            if (appliedEffect.Effect is T)
            {
                m_appliedEffects.Add(appliedEffect.WorldObject.ActivationContext.InstanceId, appliedEffect);

                StartEffect(appliedEffect, (T)appliedEffect.Effect);
            }
        }

        public virtual void RemoveEffect(AppliedEffect appliedEffect)
        {
            var effects = m_appliedEffects[appliedEffect.WorldObject.ActivationContext.InstanceId];

            if (effects != null && effects.Count > 0)
            {
                var last = effects.Last();

                effects.RemoveAll(a => a.ActivationContext.InstanceId == appliedEffect.ActivationContext.InstanceId);

                if (last.ActivationContext.InstanceId == appliedEffect.ActivationContext.InstanceId)
                {
                    // The last animation is the one that's currently animating.
                    // Stop it.
                    StopEffect(appliedEffect, (T)appliedEffect.Effect);
                    
                    if (effects.Count > 0)
                    {
                        last = effects.Last();

                        // Start playing the next-last one.
                        StartEffect(last, (T)last.Effect);
                    }
                }
            }
        }

        protected abstract void StopEffect(AppliedEffect appliedEffect, T effect);

        protected abstract void StartEffect(AppliedEffect appliedEffect, T effect);
    }
}
