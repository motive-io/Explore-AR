// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Motive.Unity.World
{
    abstract class StackedAnimationHandler<T> : StackedEffectHandler<T>
        where T : IScriptObject
    {
        protected void StopAnimation(AppliedEffect appliedEffect, GameObject animTarget)
        {
            var anim = animTarget.GetComponent<Animation>();

            anim.Stop();
        }

        protected void StartAnimationWithName(AppliedEffect appliedEffect, GameObject animTarget, string name)
        {
            var anim = animTarget.GetComponent<Animation>();

            if (anim && name != null)
            {
                foreach (var a in anim)
                {
                    var state = (AnimationState)a;

                    if (state.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        state.speed = (float)appliedEffect.EffectPlayer.Speed.GetValueOrDefault(1);
                        anim.clip = state.clip;
                        anim.Play();

                        break;
                    }
                }
            }
        }
    }
}
