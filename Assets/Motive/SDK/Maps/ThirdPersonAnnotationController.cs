// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using Motive.AR.LocationServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Controls for 3D map avatars.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ThirdPersonAnnotationController : MonoBehaviour
    {

        public enum AnimationState
        {
            None,
            Walking,
            Running
        }

        public float RunSpeed = 100f;
        public float WalkSpeed = 5f;

        public AnimationState DebugAnimationState;

        Coordinates m_lastPosition;
        float m_lastTime;

        Animator m_animator;
        // Use this for initialization
        void Start()
        {
            m_animator = GetComponent<Animator>();

            InvokeRepeating("CheckSpeed", .1f, .1f);

            m_lastPosition = ForegroundPositionService.Instance.Position;
            m_lastTime = Time.time;
        }

        void CheckSpeed()
        {
            var pos = ForegroundPositionService.Instance.Position;

            if (DebugAnimationState == AnimationState.None)
            {
                var spm = Platform.Instance.Pedometer.StepsPerMinute;

                if (spm > 0)
                {
                    bool run = spm > 140;
                    bool walk = (spm > 0) && !run;

                    m_animator.SetBool("Walk", walk);
                    m_animator.SetBool("Run", run);
                }
                else
                {
                    if (pos != null && m_lastPosition != null)
                    {
                        var d = m_lastPosition.GetDistanceFrom(pos);
                        var dt = Time.time - m_lastTime;

                        var refSpeed = (d / dt) * 60; // m/s * 60 = m / min

                        bool run = refSpeed > RunSpeed;
                        bool walk = refSpeed > WalkSpeed;

                        m_animator.SetBool("Walk", walk && !run);
                        m_animator.SetBool("Run", run);
                    }
                }
            }
            else
            {
                m_animator.SetBool("Walk", DebugAnimationState == AnimationState.Walking);
                m_animator.SetBool("Run", DebugAnimationState == AnimationState.Running);
            }

            m_lastPosition = pos;
            m_lastTime = Time.time;
        }
    }

}