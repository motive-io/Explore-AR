// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.Webcam
{
    class CameraManager : SingletonComponent<CameraManager>
    {

        public CameraProvider _camera;

        protected override void Awake()
        {
            base.Awake();

            if (!_camera)
            {
                _camera = GetComponent<CameraProvider>();
            }
        }

        // Performs a CCW Rotation of image by 90deg
        public static Texture2D RotateImage(int size, Texture2D original)
        {
            var rotated = new Texture2D(size, size, TextureFormat.ARGB32, false);

            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    rotated.SetPixel(x, y, original.GetPixel(size - y - 1, x));
                }
            }


            // We don't need the original anymore.
            Texture2D.DestroyImmediate(original);

            return rotated;
        }

        //// Rotates the texture in parallel. 
        /// 
        /// Nolan:: This doesn't work because the get and set pixel calls can't happen off the main thread...
        /// Unity doesn't let us access this off the main thread :'(
        /// TODO: Read into byte array, slice it and rotate that in parallel.. 
        /// Would it even be faster at that point? 
        /// 
        //public static Texture2D RotateMxNImageParallel(Texture2D original)
        //{
        //    var m = original.width;
        //    var n = original.height;

        //    // Our mxn matrix will be an nxm after.
        //    var rotated = new Texture2D(n, m, TextureFormat.ARGB32, false);

        //    var threads = new List<Thread>();


        //    // Slice horizontally.
        //    for (var i = 0; i < m; i++)
        //    {
        //        // Now for each remaining column, we will process up.
        //        var i1 = i;
        //        var t = new Thread(() =>
        //        {
        //            for (var j = 0; j < n; j++)
        //            {
        //                var toSet = original.GetPixel(i1, j);
        //                rotated.SetPixel(j, m - i1 - 1, toSet);
        //            }
        //        });

        //        t.Start();
        //        threads.Add(t);
        //    }

        //    foreach (var thread in threads)
        //    {
        //        thread.Join();
        //    }

        //    return rotated;
        //}

        public static Texture2D RotateMByNImage(Texture2D original)
        {

                var rotated = new Texture2D(original.height, original.width, TextureFormat.ARGB32,  false);

                for (var i = 0; i < original.width; i++)
                {
                    for (var j = 0; j < original.height; j++)
                    { 
                        rotated.SetPixel(j, original.width - i - 1, original.GetPixel(i, j));
                    }
                }

            // We don't need the original anymore.
            Texture2D.DestroyImmediate(original);
            return rotated;
        }

        public static Texture2D MirrorImage(Texture2D original)
        {
            var mirror = new Texture2D(original.width, original.height, TextureFormat.ARGB32, false);

            for (var i = 0; i < original.height; i++)
            {
                for (var j = 0; j < original.width / 2; j++)
                {
                    var lhs_x = j;
                    var rhs_x = original.width - 1 - j;

                    var lhs = original.GetPixel(lhs_x, i);
                    var rhs = original.GetPixel(rhs_x, i);

                    mirror.SetPixel(lhs_x, i, rhs);
                    mirror.SetPixel(rhs_x, i, lhs);
                }
            }

            // We don't need the original anymore.
            Texture2D.DestroyImmediate(original);

            return mirror;

        }


        public bool IsCameraOn()
        {
            return !_camera.CameraIsStopped;
        }


        /// <summary>
        /// Orientates the device camera, focuses, and takes a photograph using it.
        /// </summary>
        /// <returns></returns>
        public void TakePicture(int width, int height, Action<Texture2D> onImage)
        {
            if (_camera == null)
            {
                throw new NullReferenceException("No camera is attached to the camera manager!");
            }

            _camera.OrientateCamera();
            _camera.Focus();

			_camera.TakePicture(width, height, onImage);
        }
    }
}
