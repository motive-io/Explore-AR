// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.UI;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Utility class that can apply a Motive Layout to various objects.
    /// </summary>
    public static class LayoutHelper
    {
        public static Vector3 ToVector3(Vector vector)
        {
            return new Vector3((float)vector.X, (float)vector.Y, (float)vector.Z);
        }

        public static Vector3 ToVector3(Size size)
        {
            return new Vector3((float)size.Width, (float)size.Height, (float)size.Depth);
        }

        public static Vector3 ToVector2(Vector vector)
        {
            return new Vector3((float)vector.X, (float)vector.Y, 1);
        }

        public static Vector3 ToVector2(Size size)
        {
            return new Vector3((float)size.Width, (float)size.Height, 1);
        }
        
        public static void Apply(Transform transform, Layout layout)
        {
            if (layout.Position != null)
            {
                transform.localPosition = ToVector3(layout.Position);
            }

            if (layout.Rotation != null)
            {
                transform.Rotate(ToVector3(layout.Rotation), Space.Self);
            }

            if (layout.Size != null)
            {
                transform.localScale = ToVector3(layout.Size);
            }
        }

        public static void Apply2D(Transform transform, Layout layout)
        {
            if (layout.Position != null)
            {
                transform.localPosition = ToVector2(layout.Position);
            }

            if (layout.Rotation != null)
            {
                transform.Rotate(ToVector2(layout.Rotation), Space.Self);
            }

            if (layout.Size != null)
            {
                transform.localScale = ToVector2(layout.Size);
            }
        }        
    }
}
