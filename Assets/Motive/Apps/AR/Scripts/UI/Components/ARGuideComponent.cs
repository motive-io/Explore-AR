// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.UI.Framework;
using Motive.Unity.AR;
using Motive.Unity.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    public class ARGuideData
    {
        public ARWorldObject WorldObject { get; set; }
        public string Instructions { get; set; }
        public DoubleRange Range { get; set; }
    }

    /// <summary>
    /// Displays a "guide" on the AR canvas with instructions and arrows to guide
    /// the user to point their device in the right direction.
    /// </summary>
    public class ARGuideComponent : PanelComponent<ARGuideData>
    {
        public GameObject OutOfRange;
        public Text Instructions;
        public GameObject Up;
        public GameObject Down;
        public GameObject Left;
        public GameObject Right;

        public override void Populate(ARGuideData obj)
        {
            if (Instructions)
            {
                Instructions.text = obj.Instructions;
            }

            base.Populate(obj);
        }

        bool IsVisible(Vector3 pos)
        {
            return pos.z > 0 && (pos.x >= 0 && pos.x <= 1) && (pos.y >= 0 && pos.y <= 1);
        }

        void Update()
        {
            bool r, l, u, d;
            r = l = u = d = false;

            if (ARWorld.Instance.IsActive &&
                ARWorld.Instance.MainCamera &&
                Data != null &&
                Data.WorldObject != null &&
                Data.WorldObject.GameObject != null)
            {
                var gameObj = Data.WorldObject.GameObject;

                var cam = ARWorld.Instance.GetWorldObjectCamera(Data.WorldObject);

                var pos = cam.WorldToViewportPoint(gameObj.transform.position);

                var inFront = pos.z >= 0;

                u = pos.y >= 1;
                d = pos.y <= 0 && !u;

                if (inFront)
                {
                    r = pos.x >= 1;
                    l = pos.x <= 0 && !r;
                }
                else
                {
                    r = pos.x <= .5;
                    l = pos.x >= .5 && !r;
                }

                if (Data.Range != null && OutOfRange)
                {
                    var dist = ARWorld.Instance.GetDistance(Data.WorldObject);

                    OutOfRange.SetActive(!Data.Range.IsInRange(dist) && IsVisible(pos));
                }
                else if (OutOfRange)
                {
                    OutOfRange.SetActive(false);
                }
            }
            else
            {
                ObjectHelper.SetObjectActive(OutOfRange, false);
            }

            if (Up)
            {
                Up.gameObject.SetActive(u);
            }

            if (Down)
            {
                Down.gameObject.SetActive(d);
            }

            if (Right)
            {
                Right.gameObject.SetActive(r);
            }

            if (Left)
            {
                Left.gameObject.SetActive(l);
            }
        }
    }

}