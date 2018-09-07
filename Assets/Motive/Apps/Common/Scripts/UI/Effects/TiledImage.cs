// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using Motive.UI;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.UI
{
    public class TiledImage : MonoBehaviour
    {
        public Shader Shader;

        public int MaxTextureSize = 4096;
        
        public void SetTexture(Texture2D tex, Color? color = null)
        {
            foreach (Transform child in transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            if (tex.width <= MaxTextureSize &&
                tex.height <= MaxTextureSize)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);

                var renderer = tile.GetComponentInChildren<Renderer>();
                renderer.material.shader = Shader;
                renderer.material.mainTexture = tex;

                tile.transform.SetParent(this.transform);
                tile.transform.localPosition = Vector2.zero;
                tile.transform.localScale = Vector2.one;

                if (color != null)
                {
                    renderer.material.color = color.Value;
                }
            }
            else
            {
                /*...don't tempt fate for now
                this.gameObject.transform.localScale = new Vector2(1f / tex.width, 1f / tex.height);

                for (int x = 0; x < tex.width; x += MaxTextureSize)
                {
                    var w = Math.Min(tex.width - x, MaxTextureSize);

                    for (int y = 0; y < tex.height; y += MaxTextureSize)
                    {
                        var h = Math.Min(tex.height - y, MaxTextureSize);

                        var tiletex = new Texture2D(w, h, tex.format, false);

                        var tile = GameObject.CreatePrimitive(PrimitiveType.Quad);

                        // Now load the tex bits into tiletex
                        for (int xp = x; xp < w; xp++)
                        {
                            for (int yp = y; yp < h; yp++) 
                            {
                                var c = tex.GetPixel(x + xp, y + yp);
                                tiletex.SetPixel(xp, yp, c);
                            }
                        }

                        tiletex.Apply();

                        var renderer = tile.GetComponentInChildren<Renderer>();
                        renderer.material.shader = Shader;
                        renderer.material.mainTexture = tiletex;

                        tile.transform.SetParent(transform);

                        tile.transform.localPosition = new Vector2(x + w / 2, y + h / 2);
                        tile.transform.localScale = new Vector2(w, h);
                    }
                }
                */
            }
        }

        public void LoadMedia(MediaElement media)
        {
            if (media != null && media.MediaUrl != null)
            {
                ImageLoader.LoadTexture(media.MediaUrl, (tex) =>
                {
                    SetTexture(tex, ColorHelper.ToUnityColor(media.Color));
                });
            }
        }
    }
}