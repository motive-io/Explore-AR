// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Motive.Unity.Apps
{
    /// <summary>
    /// Drop this on a game object to control various elements of the quest app.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class ARQuestAppControls : MonoBehaviour
    {
        /// <summary>
        /// Raised when the currently running quest changes.
        /// </summary>
        public UnityEvent OnQuestUpdated;

        public Text TourName;

        public GameObject[] ActivateWhenQuestRunning;
        public GameObject[] ActivateWhenQuestNotRunning;

        public GameObject[] ActivateInARMode;
        public GameObject[] ActivateInMapMode;

        public UnityEvent OnARModeActivated;
        public UnityEvent OnMapModeActivated;

        void Awake()
        {
            if (OnQuestUpdated == null)
            {
                OnQuestUpdated = new UnityEvent();
            }
        }

        void ConfigureMode()
        {
            ObjectHelper.SetObjectsActive(ActivateInARMode, ARQuestAppUIManager.Instance.CurrentMode == ARQuestAppUIManager.Mode.AR);
            ObjectHelper.SetObjectsActive(ActivateInMapMode, ARQuestAppUIManager.Instance.CurrentMode == ARQuestAppUIManager.Mode.Map);
        }

        void Start()
        {
            ARQuestAppUIManager.Instance.QuestUpdated.AddListener(QuestUpdated);
            ARQuestAppUIManager.Instance.OnModeChanged.AddListener(ConfigureMode);

            ARQuestAppUIManager.Instance.OnModeChanged.AddListener(ModeChanged);

            ConfigureMode();
            QuestUpdated();
        }

        void ModeChanged()
        {
            if (ARQuestAppUIManager.Instance.CurrentMode == ARQuestAppUIManager.Mode.AR)
            {
                if (OnARModeActivated != null)
                {
                    OnARModeActivated.Invoke();
                }
            }
            else
            {
                if (OnMapModeActivated != null)
                {
                    OnMapModeActivated.Invoke();
                }
            }
        }

        void QuestUpdated()
        {
            bool isRunning = ARQuestAppUIManager.Instance.RunningQuest != null;

            if (TourName)
            {
                if (isRunning)
                {
                    TourName.text = ARQuestAppUIManager.Instance.RunningQuest.Title.ToUpper();
                }
                else
                {
                    TourName.text = null;
                }
            }

            ObjectHelper.SetObjectsActive(ActivateWhenQuestRunning, isRunning);
            ObjectHelper.SetObjectsActive(ActivateWhenQuestNotRunning, !isRunning);

            if (OnQuestUpdated != null)
            {
                OnQuestUpdated.Invoke();
            }
        }

        /// <summary>
        /// Stops the current tour.
        /// </summary>
        public void StopCurrentQuest()
        {
            ARQuestAppUIManager.Instance.StopCurrentQuest();
        }

        /// <summary>
        /// Sets AR mode.
        /// </summary>
        public void SetARMode()
        {
            ARQuestAppUIManager.Instance.SetARMode();
        }

        /// <summary>
        /// Toggles between AR and Map mode.
        /// </summary>
        public void ToggleMode()
        {
            if (ARQuestAppUIManager.Instance.CurrentMode == ARQuestAppUIManager.Mode.AR)
            {
                SetMapMode();
            }
            else
            {
                SetARMode();
            }
        }

        /// <summary>
        /// Sets Map mode.
        /// </summary>
        public void SetMapMode()
        {
            ARQuestAppUIManager.Instance.SetMapMode();
        }
    }

}