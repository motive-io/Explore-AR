// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Makes a game object move with a MapTexture.
    /// </summary>
    public class MoveWithMapTexture : MonoBehaviour
    {
        public MapTexture MapTexture;

        Renderer m_mapRenderer;
        Renderer m_objRenderer;

        void Awake()
        {
            m_objRenderer = this.GetComponent<Renderer>();
            m_mapRenderer = MapTexture.GetComponent<Renderer>();
        }

        void LateUpdate()
        {
            var offset = m_mapRenderer.material.mainTextureOffset;
            offset *= m_objRenderer.material.mainTextureScale.x / m_mapRenderer.material.mainTextureScale.x / transform.localScale.x;
            m_objRenderer.material.mainTextureOffset = offset;
        }

    }

}