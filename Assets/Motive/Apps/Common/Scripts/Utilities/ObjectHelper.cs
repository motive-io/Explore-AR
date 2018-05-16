// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D.Models;
using Motive.Unity.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Helper calls to activate/deactivate sets of objects. Does null checks so
    /// the caller doesn't have to.
    /// </summary>
    public static class ObjectHelper
    {
        public static void SetObjectActive(GameObject obj, bool active)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }

        public static void SetObjectsActive(IEnumerable<GameObject> objects, bool active)
        {
            if (objects == null)
            {
                return;
            }

            foreach (var obj in objects)
            {
                if (obj)
                {
                    obj.SetActive(active);
                }
            }
        }

        public static GameObject ConfigureAsset(
            GameObject gameObj,
            AssetInstance assetInstance,
            Transform parent = null,
            bool applyLayout = true,
            bool addCollider = true)
        {
            var layoutObj = new GameObject("Layout");

            layoutObj.transform.localPosition = Vector3.zero;
            layoutObj.transform.localRotation = Quaternion.identity;
            layoutObj.transform.localScale = Vector3.one;

            gameObj.transform.localPosition = Vector3.zero;
            gameObj.transform.localRotation = Quaternion.identity;

            gameObj.transform.SetParent(layoutObj.transform);

            if (assetInstance.Layout != null)
            {
                LayoutHelper.Apply(layoutObj.transform, assetInstance.Layout);
            }

            var collider = gameObj.GetComponent<Collider>();

            if (!collider)
            {
                gameObj.AddComponent<SphereCollider>();
            }

            if (parent)
            {
                layoutObj.transform.SetParent(parent.transform, false);
            }

            return layoutObj;
        }

        public static GameObject InstantiateAsset(
            GameObject prefab,
            AssetInstance assetInstance,
            out GameObject assetObj,
            Transform parent = null,
            bool applyLayout = true,
            bool addCollider = true)
        {
            var gameObj = GameObject.Instantiate(prefab);

            assetObj = gameObj;

            return ConfigureAsset(gameObj, assetInstance, parent, applyLayout, addCollider);
        }

        internal static void SetObjectActive(Component c, bool active)
        {
            if (c)
            {
                c.gameObject.SetActive(active);
            }
        }
    }

}