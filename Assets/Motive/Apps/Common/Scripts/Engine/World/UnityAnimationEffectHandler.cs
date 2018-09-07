using Motive.Core.Utilities;
using Motive.Unity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Motive.Unity.World
{
    class UnityAnimationEffectHandler : IWorldObjectEffectHandler
    {
        private Dictionary<AppliedEffect, MonoBehaviour> m_effects = new Dictionary<AppliedEffect, MonoBehaviour>();

        public void ApplyEffect(AppliedEffect appliedEffect)
        {
            var anim = appliedEffect.Effect as UnityAnimation;

            if (anim != null)
            {
                var asset = anim.Asset;

                if (asset != null)
                {
                    var animTgt = appliedEffect.WorldObject.GetAnimationTarget();

                    if (animTgt)
                    {
                        // Target may have been destroyed somewhere
                        var objAnim = animTgt.AddComponent<WorldObjectAnimation>();
                        objAnim.AnimationAsset = asset;

                        m_effects.Add(appliedEffect, objAnim);
                    }
                }
            }
        }

        public void RemoveEffect(AppliedEffect appliedEffect)
        {
            MonoBehaviour effect;

            if (m_effects.TryGetValue(appliedEffect, out effect))
            {
                GameObject.Destroy(effect);
            }
        }
    }
}
