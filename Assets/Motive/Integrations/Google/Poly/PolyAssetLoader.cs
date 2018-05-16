// Copyright (c) 2018 RocketChicken Interactive Inc.

using PolyToolkit;
using System;
using UnityEngine;

namespace Motive.Google
{
    public static class PolyAssetLoader
    {
        public static void LoadAsset(PolyAsset asset, Action<GameObject> onLoad)
        {
            PolyApi.GetAsset("assets/" + asset.AssetId, (result) =>
            {
                GetAssetCallback(result, onLoad);
            });
        }

        // Callback invoked when the featured assets results are returned.
        private static void GetAssetCallback(PolyStatusOr<PolyToolkit.PolyAsset> result, Action<GameObject> onImport)
        {
            if (!result.Ok)
            {
                Debug.LogError("Failed to get assets. Reason: " + result.Status);
                //statusText.text = "ERROR: " + result.Status;
                return;
            }
            Debug.Log("Successfully got asset!");

            // Set the import options.
            PolyImportOptions options = PolyImportOptions.Default();
            // We want to rescale the imported mesh to a specific size.
            options.rescalingMode = PolyImportOptions.RescalingMode.CONVERT;
            // The specific size we want assets rescaled to (fit in a 5x5x5 box):
            options.desiredSize = 5.0f;
            // We want the imported assets to be recentered such that their centroid coincides with the origin:
            options.recenter = true;

            //statusText.text = "Importing...";
            PolyApi.Import(result.Value, options, (asset, importResult) =>
            {
                if (result.Ok)
                {
                    onImport(importResult.Value.gameObject);
                }
            });
        }
    }
}
