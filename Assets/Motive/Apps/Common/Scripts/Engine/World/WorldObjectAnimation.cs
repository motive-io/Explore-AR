// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Models;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.World
{
    public class WorldObjectAnimation : MonoBehaviour
    {
        public UnityAsset AnimationAsset { get; set; }
        Animator m_animator;
        RuntimeAnimatorController m_ctl;

        void Start()
        {
            m_animator = gameObject.GetComponent<Animator>();

            if (!m_animator)
            {
                m_animator = gameObject.AddComponent<Animator>();
            }

            if (AnimationAsset.AssetBundle != null)
            {
                UnityAssetLoader.LoadAsset<RuntimeAnimatorController>(AnimationAsset, (ctl) =>
                {
                    m_ctl = ctl;

                    m_animator.runtimeAnimatorController = m_ctl;
                });
            }
            else
            {
                m_ctl = Resources.Load<RuntimeAnimatorController>(AnimationAsset.AssetName);

                m_animator.runtimeAnimatorController = m_ctl;
            }
        }

        void OnDestroy()
        {
            Destroy(m_animator);
        }
    }

}