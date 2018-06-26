// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.UI.Framework
{
    /// <summary>
    ///     Class that represents an embedded container for panels.
    ///     This allows for one to inject panels into other panels.
    /// </summary>
    public class EmbeddedPanelContainer : PanelContainer
    {
        public List<Type> EnabledPanels;

        // An embedded panel container should be able to push a panel to its own stack
        // and to set the parent of that object to itself.
        // It should only operate on prefabs.
        public Panel ParentPanel;

        public Panel PushPanel(Panel toPush)
        {
            // Instantiate a copy of the panel.
            var pref = Instantiate(toPush);

            pref.SetActive(true);

            // Set this as the parent.
            pref.transform.SetParent(transform, false);

            pref.gameObject.transform.localScale = gameObject.transform.localScale;
            var p_rt = pref.GetComponent<RectTransform>();
            var rt = GetComponent<RectTransform>();

            p_rt.anchorMax = rt.anchorMax;
            p_rt.anchorMin = rt.anchorMin;
            p_rt.anchoredPosition = rt.anchoredPosition;
            p_rt.position = rt.position;
            p_rt.sizeDelta = rt.sizeDelta;

            AnalyticsHelper.FireEvent("Pushing embedded panel", new Dictionary<string, object>
            {
                {"Name", toPush.GetType().ToString()}
            });

            PushSubPanel(ParentPanel, pref, null, () => { Close(pref); });

            toPush.SetActive(false);

            return pref;
        }
    }
}