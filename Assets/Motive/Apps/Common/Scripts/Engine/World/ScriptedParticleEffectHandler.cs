// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D.Models;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.World
{
    class ScriptedParticleEffectHandler : IWorldObjectEffectHandler
    {
        const float kDefaultStartSpeed = .5f;
        const float kDefaultStartLifetime = .75f;
        const float kDefaultStartSize = .2f;
        const float kDefaultRadius = .5f;
        const float kDefaultEmissionRate = 20;

        public void ApplyEffect(AppliedEffect appliedEffect)
        {
            var anims = appliedEffect.Effect as ScriptedParticleEffect;
            var animTarget = appliedEffect.WorldObject.GetAnimationTarget().transform.parent.gameObject;
            var particles = animTarget.AddComponent<ParticleSystem>();

            //Target two materials: mat for default and userMat for rewriting 
            Material mat = Resources.Load("ParticleGlow") as Material;
            Material userMat = Resources.Load("UserParticleMaterial") as Material;

            //Set default particle emission shape
            var sh = particles.shape;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = kDefaultRadius;

            //Apply color, max particles, particle size and looping
            ParticleSystem.MainModule main = particles.main;
            Color color;

            main.startSpeed = kDefaultStartSpeed;
            main.startLifetime = kDefaultStartLifetime;
            main.startSize = kDefaultStartSize;
            main.simulationSpeed = (float)appliedEffect.EffectPlayer.Speed.GetValueOrDefault(1);

            if (anims.Color == null)
            {
                color = Color.white;
            }
            else
            {
                color = new Color32((byte)anims.Color.R, (byte)anims.Color.G, (byte)anims.Color.B, (byte)(anims.Color.A*255));
            }
            main.startColor = color;

            if (anims.MaxParticles.HasValue)
            {
                main.maxParticles = anims.MaxParticles.Value;
            }

            if (anims.ParticleSize.HasValue)
            {
                main.startSizeMultiplier = anims.ParticleSize.Value;
            }

            main.loop = anims.Loop;

            //Rate over time
            ParticleSystem.EmissionModule emit = particles.emission;

            emit.rateOverTime = kDefaultEmissionRate;

            if (anims.RateOverTime.HasValue)
            {
                emit.rateOverTime = anims.RateOverTime.Value;
            }

            //Load material image
            if (anims.ImageUrl == null)
            {
                particles.GetComponent<ParticleSystemRenderer>().material = mat;
            }
            else
            {
                ImageLoader.LoadTexture(anims.ImageUrl, (tex) =>
                {
                    if (tex)
                    {
                        userMat.mainTexture = tex;
                        particles.GetComponent<ParticleSystemRenderer>().material = userMat;
                    }
                });
            }
        }

        public void RemoveEffect(AppliedEffect appliedEffect)
        {
            var effect = appliedEffect.WorldObject.GetAnimationTarget().transform.parent.gameObject;
            var particle = effect.GetComponent<ParticleSystem>();

            if (particle != null)
            {
                GameObject.Destroy(particle);
            }
        }
    }
}
