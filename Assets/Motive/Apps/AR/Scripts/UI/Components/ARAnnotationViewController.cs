// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Gaming.Models;
using Motive.Unity.AR;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    public class ARAnnotationViewController : SingletonComponent<ARAnnotationViewController>
    {
        Dictionary<ARWorldObject, ARAnnotationContainer> m_containers;

        public ARAnnotationContainer ContainerPrefab;

        protected override void Awake()
        {
            m_containers = new Dictionary<ARWorldObject, ARAnnotationContainer>();

            base.Awake();
        }

        ARAnnotationContainer GetOrCreateContainer(ARWorldObject worldObject)
        {
            ARAnnotationContainer container = null;

            if (!m_containers.TryGetValue(worldObject, out container))
            {
                container = Instantiate(ContainerPrefab);
                container.gameObject.transform.SetParent(this.transform, false);

                PositionContainer(worldObject, container);

                m_containers[worldObject] = container;
            }

            return container;
        }

        private void PositionContainer(ARWorldObject worldObject, ARAnnotationContainer container)
        {
            if (worldObject.IsVisible && worldObject.GameObject)
            {
                container.gameObject.SetActive(true);

                var bounds = ARWorld.Instance.GetScreenBounds(worldObject);

                var rect = (RectTransform)container.transform;

                //var x = Screen.width * pos.x - (Screen.width / 2);
                //var y = Screen.height * pos.y - (Screen.height / 2);

                var minx = container.MinSize.x / Screen.width;
                var miny = container.MinSize.y / Screen.height;

                Vector2 max = bounds.max;
                Vector2 min = bounds.min;

                var size = bounds.max - bounds.min;

                if (size.x < minx || size.y < miny)
                {
                    bounds.size = new Vector2(Mathf.Max(size.x, minx), Mathf.Max(size.y, miny));
                }

                rect.anchorMax = bounds.max;
                rect.anchorMin = bounds.min;
                rect.sizeDelta = Vector3.zero;

                //rect.anchorMin = new Vector2(pos.x, pos.y);
                //rect.anchoredPosition = new Vector2(0, 0);
            }
            else
            {
                container.gameObject.SetActive(false);
            }
        }

        public void AddCollectibleAnnotation(ARWorldObject worldObject, Collectible collectible, Action onClick = null)
        {
            var container = GetOrCreateContainer(worldObject);

            container.AddCollectibleAnnotation(collectible, onClick);
        }

        public void RemoveCollectibleAnnotation(ARWorldObject worldObject, Collectible collectible)
        {
            ARAnnotationContainer container = null;

            if (m_containers.TryGetValue(worldObject, out container))
            {
                container.RemoveCollectibleAnnotation(collectible);

                if (!container.HasAnnotations)
                {
                    RemoveContainer(worldObject, container);
                }
            }
        }

        private void RemoveContainer(ARWorldObject worldObject, ARAnnotationContainer container)
        {
            m_containers.Remove(worldObject);
            Destroy(container.gameObject);

        }

        public void AddTapAnnotation(ARWorldObject worldObject, string text = null)
        {
            var container = GetOrCreateContainer(worldObject);

            container.AddTapAnnotation(text);
        }

        internal void RemoveTapAnnotation(ARWorldObject worldObject)
        {
            ARAnnotationContainer container = null;

            if (m_containers.TryGetValue(worldObject, out container))
            {
                container.RemoveTapAnnotation();

                if (!container.HasAnnotations)
                {
                    RemoveContainer(worldObject, container);
                }
            }
        }

        void LateUpdate()
        {
            foreach (var kv in m_containers)
            {
                PositionContainer(kv.Key, kv.Value);
            }
        }
    }

}