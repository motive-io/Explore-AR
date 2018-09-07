using Motive._3D.Models;
using Motive.Core.Scripting;
using Motive.Google;
using Motive.Unity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Motive.Unity.Utilities
{
    public static class AssetLoader
    {
        public static void PreloadAsset(IScriptObject asset)
        {
            if (asset is UnityAsset)
            {
                UnityAssetLoader.LoadAsset<GameObject>((UnityAsset)asset, (obj) => { });
            }
        }

        public static void LoadAsset<T>(IScriptObject asset, Action<T> onLoad)
            where T : UnityEngine.Object
        {
            /*
            var poly = new PolyAsset
            {
                AssetId = "7E-xyB6aiQ2"
            };

            PolyAssetLoader.LoadAsset(poly, (obj) =>
            {
                onLoad(obj as T);
            });*/

            if (asset is UnityAsset)
            {
                UnityAssetLoader.LoadAsset<T>(asset as UnityAsset, (prefab) =>
                {
                    var obj = GameObject.Instantiate(prefab);

                    onLoad(obj);
                });
            }
#if MOTIVE_GOOGLE_POLY
            else if (asset is PolyAsset)
            {
                PolyAssetLoader.LoadAsset(asset as PolyAsset, (obj) =>
                {
                    onLoad(obj as T);
                });
            }
#endif
            else if (asset is BinaryAsset) 
            {
                BinaryAssetLoader.LoadAsset(asset as BinaryAsset, (obj) =>
                {
                    onLoad(obj as T);
                });
            }

            else
            {
                SystemErrorHandler.Instance.ReportError("Unknown asset type {0}", asset.Type);

                throw new NotSupportedException("Unknown asset type " + asset.GetType());
            }
        }
    }
}
