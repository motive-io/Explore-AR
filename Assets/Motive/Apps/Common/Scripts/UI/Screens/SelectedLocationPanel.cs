// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.Core.Models;
using Motive.UI.Framework;
using Motive.Unity.Maps;
using Motive.Unity.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Default selected location panel that takes a MapAnnotation.
    /// </summary>
    public class SelectedLocationPanel : SelectedAnnotationPanel<MapAnnotation>
    {
    }

    /// <summary>
    /// Base class for handling selected location panels that take typed data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelectedLocationPanelBase<T> : Panel<T>
    {
        public Text Title;
        public Text Subtitle;
        public Text Distance;
        public Text Description;
        public Text TimeLeft;

        public GameObject InformationPane;
        public GameObject ButtonPane;
        public Text InfoDescription;
        public RawImage InfoImage;

        public Button Button;
        public GameObject[] ShowWhenOutOfRange;
        public GameObject[] ShowWhenInRange;
        public Text RangeMessage;
        public Action OnAction;
        public int MinActionRange = 0;
        public int MaxActionRange = 100;

        public RotateWithCompass WayfindingCompass;

        protected LocationFence m_fence;

        protected virtual bool ShouldShowButton(out string buttonText)
        {
            buttonText = null;

            return false;
        }

        public void SetTitle(string title)
        {
            if (Title)
            {
                Title.text = title;
            }
        }

        public void SetSubtitle(string subtitle)
        {
            if (Subtitle)
            {
                Subtitle.text = subtitle;
            }
        }

        public void SetDescription(string description)
        {
            if (Description)
            {
                Description.text = description;
            }
        }

        public virtual void ShowButton(bool showButton, string text = null)
        {
            if (Button)
            {
                Button.gameObject.SetActive(showButton);
                Button.interactable = true;
            }

            if (ButtonPane)
            {
                ButtonPane.SetActive(showButton);
            }

            if (Button && showButton && text != null)
            {
                var txtButton = Button.GetComponentInChildren<Text>();

                if (txtButton)
                {
                    txtButton.text = text;
                }
            }
        }

        public virtual void SetInRange(bool isInRange)
        {
            ObjectHelper.SetObjectsActive(ShowWhenInRange, isInRange);
            ObjectHelper.SetObjectsActive(ShowWhenOutOfRange, !isInRange);
        }

        public override void ClearData()
        {
            // By default, display the "in range" elements.
            SetInRange(true);
        }

        public void SetFence(Location location, double minRange, double maxRange, Action onInRange = null, Action onOutOfRange = null)
        {
            if (m_fence != null)
            {
                ForegroundPositionService.Instance.DiscardFence(m_fence);
            }

            m_fence = ForegroundPositionService.Instance.CreateFence(
                location,
                minRange, maxRange, (locations) =>
                {
                    SetInRange(true);

                    if (onInRange != null)
                    {
                        onInRange();
                    }
                },
                () =>
                {
                    SetInRange(false);

                    if (onOutOfRange != null)
                    {
                        onOutOfRange();
                    }
                }
            );
        }

        public void SetButtonFence(Location location, double minRange, double maxRange, string inRangeText = null, string outOfRangeText = null)
        {
            SetFence(location, minRange, maxRange,
                () =>
                {
                    if (Button)
                    {
                        Button.interactable = true;
                    }

                    if (RangeMessage && inRangeText != null)
                    {
                        RangeMessage.text = inRangeText;
                    }
                },
                () =>
                {
                    if (Button)
                    {
                        Button.interactable = false;
                    }

                    if (RangeMessage && outOfRangeText != null)
                    {
                        RangeMessage.text = outOfRangeText;
                    }
                });
        }

        public void SetButtonFence(Location location, DoubleRange range, string inRangeButtonText = null, string outOfRangeButtonText = null)
        {
            SetButtonFence(location, range.Min.GetValueOrDefault(0), range.Max.GetValueOrDefault(double.MaxValue), inRangeButtonText, outOfRangeButtonText);
        }
    }

    /// <summary>
    /// Base class for selected location panels that take a MapAnnotation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelectedAnnotationPanel<T> : SelectedLocationPanelBase<T>
        where T : MapAnnotation
    {
        protected MapAnnotation m_mapAnnotation;

        public void SelectMapAnnotation()
        {
            if (this.Data != null)
            {
                MapController.Instance.SelectAnnotation(this.Data);
            }
        }

        public void FocusAnnotation()
        {
            if (this.Data != null)
            {
                MapController.Instance.FocusAnnotation(this.Data);
            }
        }

        public override void ClearData()
        {
            ObjectHelper.SetObjectsActive(ShowWhenInRange, false);
            ObjectHelper.SetObjectsActive(ShowWhenOutOfRange, false);

            base.ClearData();
        }

        public override void Populate(T data)
        {
            if (m_fence != null)
            {
                ForegroundPositionService.Instance.DiscardFence(m_fence);
                m_fence = null;
            }

            m_mapAnnotation = data;

            if (InformationPane)
            {
                InformationPane.SetActive(false);
            }

            SetTitle(data.Location.Name);
            SetSubtitle(data.Location.Subtitle);

            if (WayfindingCompass)
            {
                WayfindingCompass.SetPointAtLocation(data.Location);
            }

            base.Populate(data);

            // Pass annotation data to any components that use it.
            PopulateComponents(data.GetData());
        }

        public void SetFence(double minRange, double maxRange, Action onInRange = null, Action onOutOfRange = null)
        {
            base.SetFence(m_mapAnnotation.Location, minRange, maxRange, onInRange, onOutOfRange);
        }

        public void SetButtonFence(DoubleRange range)
        {
            base.SetButtonFence(m_mapAnnotation.Location, range);
        }

        public void SetButtonFence(double minRange, double maxRange)
        {
            base.SetButtonFence(m_mapAnnotation.Location, minRange, maxRange);
        }

        public override void DidPop()
        {
            if (m_fence != null)
            {
                ForegroundPositionService.Instance.DiscardFence(m_fence);
                m_fence = null;
            }

            base.DidPop();
        }

        protected virtual void Update()
        {
            if (Distance && m_mapAnnotation != null)
            {
                var d = ForegroundPositionService.Instance.Position.GetDistanceFrom(m_mapAnnotation.Location.Coordinates);

                Distance.text = StringFormatter.FormatDistance(d);
            }
        }

        public virtual void Action()
        {
            if (OnAction != null)
            {
                OnAction();
            }
        }
    }
}