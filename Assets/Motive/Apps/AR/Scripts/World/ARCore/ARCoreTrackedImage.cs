// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

#if MOTIVE_ARCORE
using GoogleARCore;
#endif

namespace Motive.Unity.AR
{
    public class ARCoreTrackedImage : MonoBehaviour
    {
#if MOTIVE_ARCORE
        public AugmentedImage Image;
        public Anchor Anchor;

        private void Update()
        {
            transform.localScale = new Vector3(Image.ExtentX, 1, Image.ExtentZ);
        }
#endif
    }
}
