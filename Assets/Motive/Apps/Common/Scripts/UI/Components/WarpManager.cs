// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.UI.Framework;
using Motive.Unity.Maps;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using UnityEngine;
using Motive.Core.Models;
using System.Collections.Generic;

namespace Motive.Unity.Apps
{
    /// <summary>
    /// Manages the "warp" feature that lets users tap-and-hold the map to
    /// move around.
    /// </summary>
    public class WarpManager : MonoBehaviour
    {
        public WarpPanel WarpPanel;
        public WarpPanel UnwarpPanel;
        public GameObject UnwarpButton;
        public string WarpDebugSetting;
        
        void Start()
        {
            MapController.Instance.OnHold.AddListener(Warp);

            ARQuestAppUIManager.Instance.QuestUpdated.AddListener(() =>
            {
                if (!CanWarp())
                {
                    Unwarp();
                }
            });
        }

        bool CanWarp()
        {
            bool isPublished =
                ARQuestAppUIManager.Instance.RunningQuest != null &&
                ARQuestAppUIManager.Instance.RunningQuest.PublishingStatus == PublishingStatus.Published;

            return !isPublished;
        }

        void Warp(MapControllerEventArgs args)
        {
            if (CanWarp())
            {
                bool warp = string.IsNullOrEmpty(WarpDebugSetting) ||
                SettingsHelper.IsDebugSet(WarpDebugSetting);

                if (warp)
                {
                    PanelManager.Instance.Push(WarpPanel, args.Coordinates);
                }
            }          
        }

        public void Unwarp()
        {
            ForegroundPositionService.Instance.SetAnchorPosition(null);            
        }

        void Update()
        {
            if (UnwarpButton)
            {
                UnwarpButton.SetActive(UserLocationService.Instance.AnchorPosition != null);
            }
        }
    }
}