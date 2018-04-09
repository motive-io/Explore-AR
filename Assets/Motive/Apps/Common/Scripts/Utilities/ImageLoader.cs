// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using Motive.Core.Utilities;
using Motive.Unity.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// This class handles loading images downloaded from the Motive server.
    /// </summary>
    public static class ImageLoader
    {
        class CachedTexture
        {
            public string Url;
            public Texture2D Texture;
        }

        static ListDictionary<string, Action<Texture2D>> g_pendingRequests = new ListDictionary<string, Action<Texture2D>>();

        static LinkedList<CachedTexture> g_cache = new LinkedList<CachedTexture>();
        static int g_cacheSize = 10;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            g_cache.Clear();
            g_pendingRequests.Clear();
        }

        static Texture2D GetTextureFromCache(string url)
        {
            lock (g_cache)
            {
                return g_cache.Where(c => c.Url == url).Select(c => c.Texture).FirstOrDefault();
            }
        }

        static void TouchCache(string url)
        {
            LinkedListNode<CachedTexture> item = null;

            lock (g_cache)
            {
                for (var node = g_cache.First; node != null; node = node.Next)
                {
                    if (node.Value.Url == url)
                    {
                        item = node;
                        break;
                    }
                }
            }

            if (item != null)
            {
                g_cache.Remove(item);
                g_cache.AddFirst(item);
            }
        }

        static bool AddPendingRequest(string url, Action<Texture2D> callback)
        {
            if (url == null)
            {
                return false;
            }

            bool needsCall = false;

            lock (g_pendingRequests)
            {
                var reqs = g_pendingRequests[url];

                needsCall = (reqs == null);

                g_pendingRequests.Add(url, callback);
            }

            return needsCall;
        }

        static void AddTextureToCache(string url, Texture2D texture)
        {
            IEnumerable<Action<Texture2D>> calls = null;

            lock (g_pendingRequests)
            {
                calls = g_pendingRequests[url];
                g_pendingRequests.RemoveAll(url);
            }

            lock (g_cache)
            {
                while (g_cache.Count >= g_cacheSize)
                {
                    g_cache.RemoveLast();
                }

                g_cache.AddFirst(new CachedTexture { Url = url, Texture = texture });
            }

            if (calls != null)
            {
                foreach (var call in calls)
                {
                    call(texture);
                }
            }
        }

        static void ApplyImage(Texture2D texture, GameObject obj, bool scaleToFit, Action onComplete)
        {
            var renderer = obj.GetComponent<Renderer>();

            obj.SetActive(true);

            if (renderer)
            {
                if (scaleToFit)
                {
                    var aspect = (float)texture.width / (float)texture.height;

                    if (aspect > 1)
                    {
                        // Wider than tall, reduce y scale
                        renderer.transform.localScale =
                            new Vector3(renderer.transform.localScale.x, renderer.transform.localScale.y / aspect, renderer.transform.localScale.z);
                    }
                    else
                    {
                        // Wider than tall, reduce x scale
                        renderer.transform.localScale =
                            new Vector3(renderer.transform.localScale.x * aspect, renderer.transform.localScale.y, renderer.transform.localScale.z);
                    }
                }

                renderer.material.mainTexture = texture;
            }

            if (onComplete != null)
            {
                onComplete();
            }
        }

        static void ApplyImage(string url, Texture2D texture, GameObject obj, bool scaleToFit, Action onComplete)
        {
            // Game object could have been destroyed by this time
            if (!obj)
            {
                if (onComplete != null)
                {
                    onComplete();
                }

                return;
            }

            obj.SetActive(true);

            ApplyImage(texture, obj, scaleToFit, onComplete);

            TouchCache(url);
        }

        static void ApplyImage(Texture2D texture, RawImage image, Action onComplete)
        {
            if (!image)
            {
                if (onComplete != null)
                {
                    onComplete();
                }

                return;
            }

            if (texture)
            {
                image.gameObject.SetActive(true);
            }

            var oldtex = image.texture;

            image.texture = texture;

            var fitter = image.GetComponent<AspectRatioFitter>();

            if (fitter != null)
            {
                fitter.aspectRatio = (float)texture.width / (float)texture.height;
            }

            if (oldtex)
            {
                //GameObject.Destroy(oldtex);
            }

            if (onComplete != null)
            {
                onComplete();
            }
        }

        static void ApplyImage(string url, Texture2D texture, RawImage image, Action onComplete)
        {
            if (image)
            {
                ApplyImage(texture, image, onComplete);

                TouchCache(url);
            }
            else
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            }
        }

        static void ApplyImage(string url, byte[] responseData, RawImage image, Action onComplete)
        {
            // Pass it off to the texture. 
            var texture = new Texture2D(2, 2);
            texture.LoadImage(responseData);

            ApplyImage(url, texture, image, onComplete);
        }

        static bool ApplyCachedTexture(string url, GameObject gameObject, bool scaleToFit, Action onComplete)
        {
            var cached = GetTextureFromCache(url);

            if (cached != null)
            {
                ApplyImage(url, cached, gameObject, scaleToFit, onComplete);

                TouchCache(url);

                return true;
            }

            if (onComplete != null)
            {
                onComplete();
            }

            return false;
        }

        static bool ApplyCachedTexture(string url, RawImage image, Action onComplete)
        {
            var cached = GetTextureFromCache(url);

            if (cached != null)
            {
                ApplyImage(cached, image, onComplete);

                return true;
            }

            if (onComplete != null)
            {
                onComplete();
            }

            return false;
        }

        static void LoadTextureOnThread(string url, Action<Texture2D> onLoad)
        {
            var loadImage = new ThreadStart(() =>
            {
                var local = WebServices.Instance.MediaDownloadManager.GetPathForItem(url);

                var data = File.ReadAllBytes(local);

                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    var texture = new Texture2D(2, 2);
                    texture.LoadImage(data);
                    onLoad(texture);
                });
            });

            // Start the actual thread. 
            var LoadImage = new Thread(loadImage);
            LoadImage.Start();
        }

        public static void LoadWebImageOnMainThread(string url, RawImage image, Action onComplete = null)
        {
            ThreadHelper.Instance.StartCoroutine(LoadWebImage(url, image, onComplete));
        }

        public static IEnumerator LoadWebImage(string url, RawImage image, Action onComplete = null)
        {
            if (!image)
            {
                if (onComplete != null)
                {
                    onComplete();
                }

                yield break;
            }

            var www = new WWW(url);
            yield return www;

            if (image)
            {
                var texture = new Texture2D(4, 4, TextureFormat.DXT1, false);
                www.LoadImageIntoTexture(texture);

                image.texture = texture;
            }

            if (onComplete != null)
            {
                onComplete();
            }

            Resources.UnloadUnusedAssets();
        }

        public static void ClearCache()
        {
            lock (g_cache)
            {
                g_cache.Clear();
            }

            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Loads an image on a background thread. Attempts to minimize the amount of time that it is blocking the 
        /// main unity thread. 
        /// </summary>
        /// <param name="url">The url of the image to load. </param>
        /// <param name="image">The image to load. </param>
        /// <param name="onComplete"></param>
        public static void LoadImageOnThread(string url, RawImage image, Action onComplete = null)
        {
            /*
            LoadImageOnMainThread(url, image, onComplete);

            Resources.UnloadUnusedAssets();
             */
            if (!image)
            {
                if (onComplete != null)
                {
                    onComplete();
                }

                return;
            }

            if (ApplyCachedTexture(url, image, onComplete))
            {
                return;
            }

            image.gameObject.SetActive(false);

            bool needsCall = AddPendingRequest(url, (texture) => ApplyImage(texture, image, onComplete));

            if (needsCall)
            {
                LoadTextureOnThread(url, (texture) =>
                {
                    AddTextureToCache(url, texture);
                });
            }

            Resources.UnloadUnusedAssets();
        }

        public static void LoadImageOnThread(string url, GameObject obj, bool scaleToFit = true, Action onComplete = null)
        {
            if (!obj)
            {
                return;
            }

            var renderer = obj.gameObject.GetComponentInChildren<Renderer>();

            if (url == null || renderer == null)
            {
                if (onComplete != null)
                {
                    onComplete();
                }

                return;
            }

            if (ApplyCachedTexture(url, obj, scaleToFit, onComplete))
            {
                return;
            }

            bool needsCall = AddPendingRequest(url, (texture) => ApplyImage(texture, obj, scaleToFit, onComplete));

            if (needsCall)
            {
                LoadTextureOnThread(url, (texture) =>
                {
                    AddTextureToCache(url, texture);
                });
            }

            Resources.UnloadUnusedAssets();
        }

        public static void LoadImageOnMainThread(string url, RawImage image, Action onComplete = null)
        {
            ThreadHelper.Instance.StartCoroutine(LoadImage(url, image, onComplete));
        }

        public static IEnumerator LoadImage(string url, RawImage image, Action onComplete = null)
        {
            if (image == null)
            {
                if (onComplete != null)
                {
                    onComplete();
                }

                yield break;
            }

            if (ApplyCachedTexture(url, image, onComplete))
            {
                yield break;
            }

            image.gameObject.SetActive(false);

            if (url == null)
            {
                if (image.texture)
                {
                    GameObject.Destroy(image.texture);
                    image.texture = null;
                }

                if (onComplete != null)
                {
                    onComplete();
                }

                yield break;
            }

            bool needsCall = AddPendingRequest(url, (texture) => ApplyImage(texture, image, onComplete));

            if (needsCall)
            {
                var local = WebServices.Instance.MediaDownloadManager.GetPathForItem(url);
                var localUri = (new Uri(local)).AbsoluteUri;

                var www = new WWW(localUri);

                yield return www;

                var texture = new Texture2D(4, 4, TextureFormat.DXT1, false);
                www.LoadImageIntoTexture(texture);

                AddTextureToCache(url, texture);
            }
        }

        public static IEnumerator LoadImage(string url, GameObject obj, bool scaleToFit = true, Action onComplete = null)
        {
            var renderer = obj.gameObject.GetComponentInChildren<Renderer>();

            if (url == null || renderer == null)
            {
                if (onComplete != null)
                {
                    onComplete();
                }

                yield break;
            }

            if (ApplyCachedTexture(url, obj, scaleToFit, onComplete))
            {
                yield break;
            }

            renderer.gameObject.SetActive(false);

            bool needsCall = AddPendingRequest(url, (texture) => ApplyImage(texture, obj, scaleToFit, onComplete));

            if (needsCall)
            {
                var local = WebServices.Instance.MediaDownloadManager.GetPathForItem(url);
                var localUri = (new Uri(local)).AbsoluteUri;

                var www = new WWW(localUri);

                yield return www;

                var texture = new Texture2D(4, 4, TextureFormat.DXT1, false);
                www.LoadImageIntoTexture(texture);

                renderer.gameObject.SetActive(true);

                AddTextureToCache(url, texture);
            }
        }
    }


}