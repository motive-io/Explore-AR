// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using Motive.Core.Utilities;
using Motive.Unity.Models;
using Motive.Unity.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Helper class for loading Unity assets from asset bundles.
    /// </summary>
    public class UnityAssetLoader
    {
        static Dictionary<string, UnityEngine.AssetBundle> m_loadedBundles =
            new Dictionary<string, UnityEngine.AssetBundle>();

        static SetDictionary<string, Action<UnityEngine.AssetBundle>> m_loadOperations =
            new SetDictionary<string, Action<UnityEngine.AssetBundle>>();

        /** Not currently used
        public static void InstantiateAssetInstance<T>(AssetInstance assetInstance, Action<T> onLoad, bool applyLayout = true)
            where T : UnityEngine.Object
        {
            ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    var unityAsset = assetInstance.Asset as UnityAsset;

                    if (unityAsset != null)
                    {
                        LoadAsset<T>(unityAsset, (T assetObj) =>
                        {
                            onLoad(assetObj);
                        });
                    }
                });
        }

        public static void InstantiateAssetInstance(AssetInstance assetInstance, Action<GameObject> onLoad, bool applyLayout = true)
        {
            InstantiateAssetInstance<GameObject>(assetInstance, onLoad, applyLayout);
        }*/

        static void LoadAssetBundle(Motive.Unity.Models.AssetBundle motiveBundle, Action<UnityEngine.AssetBundle> onReady, bool loadAllAssets = false)
        {
            if (motiveBundle.Url == null) return;

            UnityEngine.AssetBundle unityBundle;

            if (!m_loadedBundles.TryGetValue(motiveBundle.Url, out unityBundle))
            {
                if (m_loadOperations.ContainsKey(motiveBundle.Url))
                {
                    m_loadOperations.Add(motiveBundle.Url, onReady);
                }
                else
                {
                    m_loadOperations.Add(motiveBundle.Url, onReady);

                    var local = WebServices.Instance.MediaDownloadManager.GetPathForItem(motiveBundle.Url);
                    //var localUri = (new Uri(local)).AbsoluteUri;

                    var bundleLoad = UnityEngine.AssetBundle.LoadFromFileAsync(local);

                    bundleLoad.completed += (asyncObj) =>
                    {
                        unityBundle = bundleLoad.assetBundle;

                        if (unityBundle == null)
                        {
                            return;
                        }

                        if (loadAllAssets)
                        {
                            unityBundle.LoadAllAssetsAsync();
                        }

                        m_loadedBundles[motiveBundle.Url] = unityBundle;

                        foreach (var call in m_loadOperations[motiveBundle.Url])
                        {
                            call(unityBundle);
                        }

                        m_loadOperations.RemoveAll(motiveBundle.Url);
                    };
                }
            }
            else
            {
                onReady(unityBundle);
            }
        }

        public static void LoadAsset<T>(UnityAsset asset, Action<T> onLoad)
            where T : UnityEngine.Object
        {
            if (asset == null)
            {
                onLoad(null);

                return;
            }

            if (asset.AssetBundle == null || asset.AssetBundle.Url == null)
            {
                if (asset.AssetName != null)
                {
                    var obj = Resources.Load(asset.AssetName) as T;

                    onLoad(obj);
                }

                return;
            }

            Action<UnityEngine.AssetBundle> onReady = (bundle) =>
            {
                if (bundle != null)
                {
                    var resp = bundle.LoadAssetAsync<T>(asset.AssetName);

                    resp.completed += (op) =>
                    {
                        onLoad(resp.asset as T);
                    };

                    //onLoad(bundle.LoadAsset<T>(asset.AssetName));
                }
                else
                {
                    onLoad(null);
                }
            };

            LoadAssetBundle(asset.AssetBundle, onReady);
        }

        internal static void PreloadBundles(IEnumerable<Motive.Unity.Models.AssetBundle> bundles)
        {
            foreach (var bundle in bundles)
            {
                LoadAssetBundle(bundle, (_b) => { });
            }
        }

        internal static void LoadAsset(Motive._3D.Models.AssetInstance assetInstance, GameObject parent)
        {
            var unityAsset = assetInstance.Asset as UnityAsset;

            if (unityAsset == null)
            {
                return;
            }

            LoadAsset<GameObject>(unityAsset, (prefab) =>
                {
                    if (prefab != null)
                    {
                        var obj = GameObject.Instantiate(prefab);

                        obj.transform.SetParent(parent.transform);

                        obj.transform.localPosition = Vector3.zero;
                        obj.transform.localRotation = Quaternion.identity;

                        if (assetInstance.Layout != null)
                        {
                            LayoutHelper.Apply(obj.transform, assetInstance.Layout);
                        }
                    }
                });
        }
    }

}