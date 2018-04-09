// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Motive.UI.Framework;
using Motive.Unity.Utilities;
using Motive.Unity.Timing;
using System;
using Motive.Unity.Gaming;
using Motive.Unity.Maps;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Can be used to show notifications for new tasks as they come in.
    /// </summary>
    public class TaskNotifier : SingletonComponent<TaskNotifier>
    {
        public Table Table;
        public TaskNotification NotificationItem;

        public IPlayerTaskDriver TaskDriver { get; private set; }

        protected virtual bool ShouldShowNotification(IPlayerTaskDriver driver)
        {
            return true;
        }

        protected override void Awake()
        {
            base.Awake();

            if (!Table)
            {
                Table = GetComponentInChildren<Table>();
            }

            if (Table)
            {
                Table.Clear();
            }

            gameObject.SetActive(false);
        }

        public void Dismiss()
        {
            gameObject.SetActive(false);
        }

        protected virtual void Select(TaskNotification notification)
        {
            Table.RemoveItem(notification);
        }

        public virtual void AddNewTask(IPlayerTaskDriver driver)
        {
            if (ShouldShowNotification(driver))
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                    {
                        gameObject.SetActive(true);

                        if (driver is LocationTaskDriver)
                        {
                            LocationTaskAnnotationHandler.Instance.SelectTaskAnnotation(driver as LocationTaskDriver);
                        }

                        TaskDriver = driver;

                        var item = Table.AddItem(NotificationItem);

                        item.Populate(driver);

                    /*
					var timer = 
						UnityTimer.Call(item.DisplayDuration, () =>
							{
								Table.RemoveItem(item);
							});
							*/

                        item.OnSelected.AddListener(() =>
                        {
                        //timer.Cancel();
                        Select(item);
                        });
                    });
            }
        }
    }

}