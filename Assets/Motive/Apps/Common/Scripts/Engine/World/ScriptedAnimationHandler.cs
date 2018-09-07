using UnityEngine;
using Motive._3D.Models;
using Motive.Core.Models;

using Logger = Motive.Core.Diagnostics.Logger;
using System;
using System.Linq;
using System.Collections.Generic;
using Motive.Core.Utilities;

namespace Motive.Unity.World
{
    class ScriptedAnimationHandler : StackedAnimationHandler<ScriptedAnimation>
    {
        private Logger m_logger;

        public ScriptedAnimationHandler()
        {
            m_logger = new Logger(this);
        }

        class PropertyKeyframe
        {
            public Keyframe Keyframe;
            public string Easing;
        }

        protected override void StopEffect(AppliedEffect appliedEffect, ScriptedAnimation effect)
        {
            var sceneObject = appliedEffect.WorldObject.GetAnimationTarget().transform.parent.gameObject;

            base.StopAnimation(appliedEffect, sceneObject);
        }

        protected override void StartEffect(AppliedEffect appliedEffect, ScriptedAnimation effect)
        {
            var sceneObject = appliedEffect.WorldObject.GetAnimationTarget().transform.parent.gameObject;

            StartAnimationWithName(appliedEffect, sceneObject, effect.Id);
        }

        public override void ApplyEffect(AppliedEffect appliedEffect)
        {
            //Retrieve the container holding the object and add animation component to it
            var sceneObject = appliedEffect.WorldObject.GetAnimationTarget().transform.parent.gameObject;
            var sAnimations = appliedEffect.Effect as ScriptedAnimation;

            if (sceneObject && sAnimations != null)
            {
                if(sceneObject.GetComponent<Animation>() == null)
                    sceneObject.AddComponent<Animation>();

                Transform transform = sceneObject.transform;

                //Build a new animation clip, storing the keyframes in a 2D array
                AnimationClip clip = new AnimationClip
                {
                    legacy = true
                };

                float time = 0.0f;

                ListDictionary<string, PropertyKeyframe> propertyKeyframes = new ListDictionary<string, PropertyKeyframe>();
                
                Action<string, float, string, double?> addKeyframe = (prop, _time, easing, val) =>
                {
                    if (val.HasValue)
                    {
                        propertyKeyframes.Add(prop,
                            new PropertyKeyframe
                            {
                                Easing = easing,
                                Keyframe = new Keyframe(_time, (float)val.Value)
                            });                            
                    }
                };

                Action<string, float, string, NullableVector> addKeyframeVector = (prop, _time, easing, vec) =>
                {
                    if (vec != null)
                    {
                        addKeyframe(prop + ".x", _time, easing, vec.X);
                        addKeyframe(prop + ".y", _time, easing, vec.Y);
                        addKeyframe(prop + ".z", _time, easing, vec.Z);
                    }
                };

                // Let's add initial settings for the animation
                //addKeyframeVector("localPosition", 0, 

                foreach (var kFrame in sAnimations.Keyframes)
                {
                    time += (float)kFrame.Duration.TotalSeconds;

                    if (kFrame.Properties != null)
                    {
                        foreach (var property in kFrame.Properties)
                        {
                            if (property is MotiveTransform)
                            {
                                var xform = property as MotiveTransform;

                                addKeyframeVector("localPosition", time, kFrame.Easing, xform.Position);
                                addKeyframeVector("localScale", time, kFrame.Easing, xform.Scale);

                                if (xform.Rotation != null)
                                {
                                    var rot = Quaternion.Euler(
                                        (float)xform.Rotation.X.GetValueOrDefault(), 
                                        (float)xform.Rotation.Y.GetValueOrDefault(),
                                        (float)xform.Rotation.Z.GetValueOrDefault());

                                    addKeyframe("localRotation.x", time, kFrame.Easing, rot.x);
                                    addKeyframe("localRotation.y", time, kFrame.Easing, rot.y);
                                    addKeyframe("localRotation.z", time, kFrame.Easing, rot.z);
                                    addKeyframe("localRotation.w", time, kFrame.Easing, rot.w);
                                }
                            }
                            else
                            {
                                m_logger.Warning("Unknown property type in keyframe: {0}", property.Type);
                            }
                        }
                    }
                }

                //Set Easing Properties
                //SetEasing(sAnimations);

                //Setting the animation curves to the animation clip
                foreach (var kv in propertyKeyframes)
                {
                    SetEasing(kv.Value);
                    // NOTE: For now we know that all properties are transforms. We can extend
                    // the model to handle different property types in the future.
                    clip.SetCurve("", typeof(Transform), kv.Key, new AnimationCurve(kv.Value.Select(v => v.Keyframe).ToArray()));
                    //clip.SetCurve("", typeof(Transform), kv.Key, AnimationCurve.Linear(0, 0, 1, 1));
                }

                //Loop Check
                if (sAnimations.Loop)
                {
                    clip.wrapMode = WrapMode.Loop;
                }

                var anim = sceneObject.GetComponent<Animation>();

                if (anim)
                {
                    anim.AddClip(clip, sAnimations.Id);
                }

                // Uncomment this in the editor to stash a version of the created animation
                //UnityEditor.AssetDatabase.CreateAsset(clip, "Assets/" + sAnimations.Id + ".anim");

                base.ApplyEffect(appliedEffect);
            }
        }
        
        public override void RemoveEffect(AppliedEffect appliedEffect)
        {
            var objAnim = appliedEffect.WorldObject.GetAnimationTarget().transform.parent.GetComponent<Animation>();

            if(objAnim)
            {
                objAnim.RemoveClip(appliedEffect.Effect.Id);
            }

            base.RemoveEffect(appliedEffect);
        }

        private void SetEasing(List<PropertyKeyframe> propKeyframes)
        {
            if (propKeyframes.Count > 1)
            {
                for (int idx = 0; idx < propKeyframes.Count; idx++)
                {
                    var prevIdx = (idx == 0) ? propKeyframes.Count - 1 : idx - 1;
                    var prev = propKeyframes[prevIdx];
                    var next = propKeyframes[(idx + 1) % propKeyframes.Count];
                    var curr = propKeyframes[idx];

                    //Keyframes eases in/out by default thus, make curves linear when not explicitly eased in or out

                    // For linear tangents:
                    // in tangent = prev -> curr
                    // out tangent = curr -> next
                    // first in tangent = first out tangent
                    // last out tangent = last in tangent

                    Func<float> computeLinearOut = () =>
                    {
                        if (idx == propKeyframes.Count - 1)
                        {
                            // If last, compute linear out to match prev -> curr slope
                            return LinearCurve(prev.Keyframe, curr.Keyframe);
                        }
                        else
                        {
                            return LinearCurve(curr.Keyframe, next.Keyframe);
                        }
                    };

                    Func<float> computeLinearIn = () =>
                    {
                        if (idx == 0)
                        {
                            // If first, compute linear out to match curr -> next slope
                            return LinearCurve(curr.Keyframe, next.Keyframe);
                        }
                        else
                        {
                            return LinearCurve(prev.Keyframe, curr.Keyframe);
                        }
                    };

                    switch (curr.Easing)
                    {
                        case "easeIn":
                            {
                                curr.Keyframe.outTangent = computeLinearOut();
                            }
                            break;
                        case "easeOut":
                            {
                                curr.Keyframe.inTangent = computeLinearIn();
                            }
                            break;
                        case "linear":
                            {
                                curr.Keyframe.outTangent = computeLinearOut();
                                curr.Keyframe.inTangent = computeLinearIn();
                            }
                            break;
                            // easeInOut is the default
                        case "easeInOut":
                        default:
                            break;
                    }
                }
            }
        }

        private float LinearCurve(Keyframe a, Keyframe b)
        {
            if (b.time - a.time != 0)
            {
                return (b.value - a.value) / (b.time - a.time);
            }
            else
            {
                return 0;
            }
        }
    }
}
