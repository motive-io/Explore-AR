// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.AR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class ARTargetIndicator : MonoBehaviour
    {
        public Augmented3dAssetObject ArWorldObject;

        public Text Distance;
        public Image Sprite;

        public void SetSpriteColor(bool isInRange)
        {
            Sprite.color = isInRange ? Color.green : Color.red;
        }
    }
}
