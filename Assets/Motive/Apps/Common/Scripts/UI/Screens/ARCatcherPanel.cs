// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.UI.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Motive.Unity.AR;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Panel used by the ARCatcher minigame.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Motive.UI.Framework.Panel{T}" />
    public class ARCatcherPanel<T> : Panel<T>
    {
        public RectTransform Target;

        public float MinARObjectDistance;
        public float MaxARObjectDistance;
        public bool UseCatchRange;
        public float CatchRange = 25f;
        public bool TapObjectToCatch = true;

        public GameObject OffTarget;
        public GameObject InTarget;

        protected LocationARWorldObject m_worldObj;
        protected LocationARWorldObject m_distanceObj;

        public ARTargetIndicator TargetIndicator;
        public Image RangeProgress;
        public GameObject RangeGameObject;
        public Text RangeText;

        public bool DidCatch { get; protected set; }
        public bool IsInTarget { get; private set; }

        protected virtual void Update()
        {
            if (m_worldObj != null && Target)
            {
                var d = ForegroundPositionService.Instance.Position.GetDistanceFrom(m_worldObj.Location.Coordinates);

                var cam = ARWorld.Instance.GetWorldObjectCamera(m_worldObj);

                var screenPoint = cam.WorldToScreenPoint(m_worldObj.GameObject.transform.position);

                var inTarget = RectTransformUtility.RectangleContainsScreenPoint(Target, screenPoint);

                if (inTarget && UseCatchRange)
                {
                    if (d > CatchRange)
                    {
                        inTarget = false;
                    }
                }

                UpdateOffOnTarget(d, inTarget);

                Target.gameObject.SetActive(true);

                SetInTarget(inTarget);
            }
        }

        //Method that gets called e
        protected void UpdateOffOnTarget(double d, bool inTarget)
        {
            if (!(d < 60))
            {
                InTarget.SetActive(false);
                OffTarget.SetActive(false);
                TargetIndicator.gameObject.SetActive(true);
                RangeGameObject.SetActive(false);
                return;
            }

            TargetIndicator.gameObject.SetActive(false);

            if (inTarget)
            {
                InTarget.SetActive(true);
            }
            else
            {
                OffTarget.SetActive(true);
            }

            if (d > CatchRange)
            {
                OffTarget.SetActive(true);
                InTarget.SetActive(false);
            }

            RangeGameObject.SetActive(true);

            var percent = (float)(CatchRange * 2 - d) / CatchRange;

            if (percent < 1f)
            {
                RangeText.text = Math.Round(Math.Abs(CatchRange - d)) + "m out of range.";

            }
            else if (d < CatchRange && inTarget)
            {
                RangeText.text = "Target Locked! \n Tap to collect!";
            }
            else
            {
                RangeText.text = "";
            }

            RangeProgress.fillAmount = percent;
        }

        protected void AddARObject(LocationARWorldObject worldObject)
        {
            if (m_worldObj != null)
            {
                ARWorld.Instance.RemoveWorldObject(m_worldObj);
            }

            m_worldObj = worldObject;

            ARWorld.Instance.AddWorldObject(m_worldObj);
        }

        protected ARWorldObject AddARObject(Location location, Augmented3dAssetObject arObject)
        {
            var worldObj = new LocationARWorldObject
            {
                Location = location,
                Options = LocationAugmentedOptions.GetLinearDistanceOptions(1, 3, 125, true),
                GameObject = arObject.gameObject
            };

            arObject.WorldObject = worldObj;

            worldObj.Clicked += worldObj_Selected;

            AddARObject(worldObj);

            if (TargetIndicator)
            {
                var distanceWorldObj = new LocationARWorldObject
                {
                    Options = LocationAugmentedOptions.GetFixedDistanceOptions(10, true),
                    GameObject = TargetIndicator.gameObject,
                    Location = m_worldObj.Location
                };

                m_distanceObj = distanceWorldObj;
                m_distanceObj.Elevation = m_worldObj.Elevation;

                m_distanceObj.GameObject.transform.parent = m_worldObj.GameObject.transform;

                ARWorld.Instance.AddWorldObject(m_distanceObj);
            }

            return worldObj;
        }

        public virtual void CatchObject(LocationARWorldObject obj)
        {
            DidCatch = true;
            ARWorld.Instance.RemoveWorldObject(obj);
            m_worldObj = null;
        }

        public virtual void TryCatch()
        {
            var d = ForegroundPositionService.Instance.Position.GetDistanceFrom(m_worldObj.Location.Coordinates);

            var cam = ARWorld.Instance.GetWorldObjectCamera(m_worldObj);

            var screenPoint = cam.WorldToScreenPoint(m_worldObj.GameObject.transform.position);

            var inTarget = 
                screenPoint.z >= 0 &&
                RectTransformUtility.RectangleContainsScreenPoint(Target, screenPoint);

            if (inTarget && d < CatchRange)
            {
                CatchObject(m_worldObj);
            }
        }

        protected virtual void ObjectSelected(ARWorldObject obj)
        {
            if (TapObjectToCatch &&
                obj == m_worldObj &&
                IsInTarget)
            {
                CatchObject(m_worldObj);
            }
        }

        void worldObj_Selected(object sender, System.EventArgs e)
        {
            ObjectSelected((ARWorldObject)sender);
        }

        public override void DidHide()
        {
            if (m_worldObj != null)
            {
                ARWorld.Instance.RemoveWorldObject(m_worldObj);
                m_worldObj = null;
            }

            base.DidHide();
        }

        protected virtual void SetInTarget(bool inTarget)
        {
            if (InTarget)
            {
                InTarget.SetActive(inTarget);
            }

            if (OffTarget)
            {
                OffTarget.SetActive(!inTarget);
            }

            if (TargetIndicator)
            {
                var d = ForegroundPositionService.Instance.Position.GetDistanceFrom(m_worldObj.Location.Coordinates);
                TargetIndicator.Distance.text = Math.Round(d) + "m";

                TargetIndicator.SetSpriteColor(inTarget);
            }
        }

    }
}
