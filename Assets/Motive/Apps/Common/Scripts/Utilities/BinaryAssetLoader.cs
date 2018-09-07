using Motive._3D.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityGLTF;
using UnityGLTF.Loader;

namespace Motive.Unity.Utilities
{
    public static class BinaryAssetLoader
    {
        public static void LoadAsset(BinaryAsset asset, Action<GameObject> onLoad)
        {
            if (asset.Url != null)
            {
                var local = WebServices.Instance.MediaDownloadManager.GetPathForItem(asset.Url);
                var localUri = (new Uri(local)).AbsoluteUri;

                Load(localUri, onLoad);
            }
        }

        public static void Load(string path, Action<GameObject> onLoad)
        {
            //GLTF Component Settings
            int MaximumLod = 300;
            bool Multithreaded = true;
            ILoader loader = null;
            Shader shaderOverride = null;
            GLTFSceneImporter sceneImporter = null;

            string directoryPath = URIHelper.GetDirectoryName(path);
            loader = new WebRequestLoader(directoryPath);

            sceneImporter = new GLTFSceneImporter(
                Path.GetFileName(path),
                loader
                );
            
            sceneImporter.Collider = GLTFSceneImporter.ColliderType.None;
            sceneImporter.MaximumLod = MaximumLod;
            sceneImporter.CustomShaderName = shaderOverride ? shaderOverride.name : null;

            Action<GameObject> newOnLoad = (go) =>
                {
                    onLoad(go);
                };
            
            // Call coroutine to load AR object
            ThreadHelper.Instance.StartCoroutine(LoadScene(sceneImporter, Multithreaded, newOnLoad));
        }

        public static IEnumerator LoadScene(GLTFSceneImporter sceneImporter, bool Multithreaded, Action<GameObject> onLoad)
        {
            yield return sceneImporter.LoadScene(-1, Multithreaded);
            if (onLoad != null) onLoad(sceneImporter.CreatedObject);
        }
    }
}