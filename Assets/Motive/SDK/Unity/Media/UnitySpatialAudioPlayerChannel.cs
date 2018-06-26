// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Media;
using Motive.Core.Models;
using System;
using UnityEngine;

namespace Motive.Unity.Media
{
    /// <summary>
    /// Unity implementation of ISpatialAudioPlayerChannel.
    /// </summary>
    public class UnitySpatialAudioPlayerChannel : UnityAudioPlayerChannel, ISpatialAudioPlayerChannel
    {
        GameObject m_worldObject;

        public bool Spatialize { get; set; }

        public class UnitySpatialAudioPlayer : UnityAudioPlayerChannel.UnityAudioPlayer, ISpatialAudioPlayer
        {
            protected UnitySpatialAudioPlayerChannel m_channel;
            protected GameObject m_playerObject;

            public UnitySpatialAudioPlayer(UnitySpatialAudioPlayerChannel channel)
                : base(channel)
            {
                m_channel = channel;

				m_channel.Call(() => 
				               {
					m_playerObject = new GameObject("SpatialAudioPlayer");
					m_playerObject.transform.parent = m_channel.m_worldObject.transform;
				});

                SpatialPosition = new Core.Models.Vector();
            }

            public DoubleRange DistanceRange
            {
                get;
                set;
            }

            public SoundAttenuationMode AttenuationMode
            {
                get;
                set;
            }

            private Vector m_spatialPosition;

            public Core.Models.Vector SpatialPosition
            {
                get
                {
                    return m_spatialPosition;
                }
                set
                {
                    m_spatialPosition = value;

                    m_channel.Call(() =>
                        {
                            m_playerObject.transform.position =
                                new Vector3((float)value.X, (float)value.Y, (float)value.Z);
                        });
                }
            }

            internal override void OnPlay()
            {
                if (DistanceRange != null)
                {
                    if (DistanceRange.Max.HasValue)
                    {
                        AudioSource.maxDistance = (float)DistanceRange.Max.Value;
                    }

                    if (DistanceRange.Min.HasValue)
                    {
                        AudioSource.minDistance = (float)DistanceRange.Min.Value;
                    }
                }

                switch (AttenuationMode)
                {
                    case SoundAttenuationMode.Steep:
                        AudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                        break;
                    case SoundAttenuationMode.Gentle:
                    case SoundAttenuationMode.Linear:
                        AudioSource.rolloffMode = AudioRolloffMode.Linear;
                        break;
                    case SoundAttenuationMode.Natural:
                    default:
                        AudioSource.rolloffMode = AudioRolloffMode.Logarithmic;

                        var qtr = AudioSource.maxDistance / 4f;
                        var half = qtr * 2;
                        var tqtr = qtr * 3;

                        AnimationCurve curve = new AnimationCurve(
                            new Keyframe(0, 1),
                            new Keyframe(qtr, 0.5625f),
                            new Keyframe(half, 0.25f),
                            new Keyframe(tqtr, 0.0626f),
                            new Keyframe(AudioSource.maxDistance, 0, 0, 0)
                            );

                        curve.SmoothTangents(0, 0);
                        curve.SmoothTangents(1, 0);
                        curve.SmoothTangents(2, 0);
                        curve.SmoothTangents(3, 0);
                        curve.SmoothTangents(4, 1);

                        AudioSource.dopplerLevel = 0;
                        AudioSource.rolloffMode = AudioRolloffMode.Custom;
                        AudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);

                        break;
                }

                base.OnPlay();
            }
            protected override void SetAudioClip(AudioClip audioClip)
            {
                AudioSource = m_playerObject.AddComponent<AudioSource>();
                AudioSource.playOnAwake = false;
                AudioSource.clip = audioClip;
                AudioSource.spatialBlend = 1f;
                AudioSource.spatialize = m_channel.Spatialize;
            }
        }

        public Core.Models.Vector ListenerPosition
        {
            get
            {
                return new Core.Models.Vector(
                    m_worldObject.transform.position.x,
                    m_worldObject.transform.position.y,
                    m_worldObject.transform.position.z);
            }
            set
            {
                m_worldObject.transform.position =
                    new Vector3((float)value.X, (float)value.Y, (float)value.Z);
            }
        }

        public Core.Models.Vector ListenerOrientation
        {
            get
            {
                return new Vector(0, 0, 0);
            }
            set
            {
                m_worldObject.transform.rotation = Quaternion.LookRotation(
                    new Vector3((float)value.X, (float)value.Y, (float)value.Z));
            }
        }

        public virtual UnitySpatialAudioPlayer CreateSpatialPlayerObject()
        {
            return new UnitySpatialAudioPlayer(this);
        }

        public virtual ISpatialAudioPlayer CreateSpatialPlayer(Uri uri)
        {
            var player = CreateSpatialPlayerObject();

            player.Source = uri;

            ConfigurePlayer(player);

            return player;
        }

        public ISpatialAudioPlayer CreateSpatialPlayer(Uri uri, Vector position)
        {
            var player = CreateSpatialPlayer(uri);
            player.SpatialPosition = position;

            return player;
        }

        protected override void Awake()
        {
            base.Awake();

            m_worldObject = new GameObject("SpatialPlayerChannel");

            m_worldObject.transform.parent = gameObject.transform;
        }
    }
}
