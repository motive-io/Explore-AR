﻿// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.Unity.Utilities
{
    public static class ColorHelper
    {
        public static Color ToUnityColor(Motive.Core.Models.Color color)
        {
            if (color == null)
            {
                return Color.white;
            }

            return new Color(color.R, color.G, color.B, color.A);
        }
    }
}