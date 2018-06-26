// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Motive.Unity.Maps
{
    /// <summary>
    /// Base class for handling selected location panels. A selected location panel
    /// is displayed when the user selects a location on the map.
    /// </summary>
    public class SelectedLocationPanelHandler : SingletonComponent<SelectedLocationPanelHandler>
    {
        public Panel CurrentPanel { get; protected set; }

        public virtual void HideSelectedLocationPanel(Panel toHide, object data)
        {
            if (toHide.Data == data &&
                toHide == CurrentPanel)
            {
                PanelManager.Instance.Pop(CurrentPanel);
            }
        }

        public virtual void ShowSelectedLocationPanel(Panel toShow, object data, Action onClose = null)
        {
            if (data != null)
            {
                Action _close = () =>
                {
                    // Only clear the annotation if this was the one assigned to this panel
                    if (MapController.Instance.SelectedAnnotation == toShow.Data)
                    {
                        MapController.Instance.SelectAnnotation(null);
                    }

                    if (CurrentPanel == toShow)
                    {
                        CurrentPanel = null;
                    }

                    if (onClose != null)
                    {
                        onClose();
                    }
                };

                if (toShow && toShow == CurrentPanel)
                {
                    //toShow.Populate(mapAnnotation);
                    PanelManager.Instance.Push(toShow, data, _close);
                }
                else
                {
                    if (CurrentPanel != null)
                    {
                        PanelManager.Instance.Pop(CurrentPanel);
                    }

                    CurrentPanel = toShow;

                    if (toShow != null)
                    {
                        PanelManager.Instance.Push(toShow, data, _close);
                    }
                }
            }
            else
            {
                if (CurrentPanel != null)
                {
                    PanelManager.Instance.Pop(CurrentPanel);
                    CurrentPanel = null;
                }
            }
        }        
    }
}
