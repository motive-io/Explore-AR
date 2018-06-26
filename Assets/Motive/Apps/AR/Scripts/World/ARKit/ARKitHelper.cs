// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Motive.Unity.AR
{
    public static class ARKitHelper
    {
        [DllImport("__Internal")]
        private static extern bool ARKit_IsSupported();

        public static bool IsSupported()
        {
            #if MOTIVE_ARKIT
            return ARKit_IsSupported();
            #else
            return false;
            #endif
        }
    }
}
