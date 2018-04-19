// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Motive.Gaming.Models;
using Motive.AR.Models;
using Motive.UI.Framework;
using Motive.Unity.Maps;
using System.Linq;
using Motive.Unity.Utilities;
using Motive.Unity.Apps;
using Motive.Unity.Gaming;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Displays a list of tasks for the user to do.
    /// </summary>
    public class TaskPanel : TablePanel
    {

        public GameObject ShowWhenNoTasks;
        public GameObject ShowWhenTasks;

        public TaskItem GiveTaskItem;
        public TaskItem TakeTaskItem;
        public TaskItem WaitTaskItem;
        public TaskActionItem ActionItemPrefab;

        public bool ShowAllTasks;

        protected virtual T AddTaskItem<T>(T prefab, IPlayerTaskDriver driver) where T : TaskItem
        {
            var item = Table.AddItem<T>(prefab);

            item.ActionItemPrefab = ActionItemPrefab;
            item.OnSelected.AddListener(() =>
            {
                OnItemSelected(item);
            });

            item.OnAction.AddListener(() =>
                {
                    OnItemAction(item);
                });
            
            item.Populate(driver);
            ConfigureTaskItem(item, driver);

            return item;
        }

        protected virtual void OnItemAction(TaskItem item)
        {

        }

        protected virtual void OnItemSelected(TaskItem item)
        {
            MapAnnotation annotation = null;

            // Hack for now
            if (item.Driver is LocationTaskDriver)
            {
                var lt = item.Driver as LocationTaskDriver;

                annotation = LocationTaskAnnotationHandler.Instance.GetNearestAnnotation(lt);
            }
            else if (item.Driver is ARTaskDriver)
            {
                var at = item.Driver as ARTaskDriver;

                annotation = ARTaskAnnotationHandler.Instance.GetNearestAnnotation(at);
            }

            Close();

            if (annotation != null)
            {
                MapController.Instance.FocusAnnotation(annotation);
            }
            else
            {
                TodoManager.Instance.OverrideTodoTask(item.Driver);
            }
        }

        protected virtual TaskItem GetPrefabForTask(PlayerTask task)
        {
            if (TaskManager.Instance.IsWaitTask(task) && WaitTaskItem)
            {
                return WaitTaskItem;
            }
            else if (TaskManager.Instance.IsGiveTask(task) && GiveTaskItem)
            {
                return GiveTaskItem;
            }

            // Take Task Item is the default
            return TakeTaskItem;
        }

        protected virtual void ConfigureCharacterTaskItem(TaskItem item)
        {
            var task = item.Driver.Task as CharacterTask;

            if (task.ImageUrl != null)
            {
                ImageLoader.LoadImageOnMainThread(task.ImageUrl, item.Image);
            }
            else
            {
                // We can use the character image instead
                var character = task.Character;

                if (character != null && character.ImageUrl != null)
                {
                    ImageLoader.LoadImageOnMainThread(character.ImageUrl, item.Image);
                }
            }

            ShowHideActionButtons(item);
        }

        protected virtual void ConfigureDefaultTaskItem(TaskItem item)
        {
            item.Populate(item.Driver);
            ShowHideActionButtons(item);
        }

        protected virtual void ConfigureLocationTaskItem(TaskItem item)
        {
            var task = item.Driver.Task as LocationTask;

            if (task.IsHidden)
            {
                item.CompleteButton.gameObject.SetActive(false);
                item.AcceptButton.gameObject.SetActive(false);
            }
            else if (task.Locations != null && task.Locations.Length > 0)
            {
                var location = task.Locations[0];

                if (item.CompleteButton)
                {
                    item.CompleteButton.gameObject.SetActive(true);

                    item.CompleteButton.GetComponentInChildren<Text>().text =
                       location.Name;

                    item.CompleteButton.onClick.RemoveAllListeners();

                    item.CompleteButton.onClick.AddListener(() =>
                    {
                        Close();

                        MapController.Instance.SelectLocation(location);
                    });
                }
            }

            if (task.ImageUrl != null)
            {
                item.PopulateImage(item.Image, task.ImageUrl);
            }
            else if (task.Character != null)
            {
                // We can use the character image instead

                var character = task.Character;

                if (character != null && character.ImageUrl != null)
                {
                    item.PopulateImage(item.Image, character.ImageUrl);
                }
            }

            ShowHideActionButtons(item);
        }

        protected virtual void ShowHideActionButtons(TaskItem item)
        {
            if (item.AcceptButton)
            {
                item.AcceptButton.gameObject.SetActive(!item.Driver.IsAccepted);
            }

            if (item.CompleteButton)
            {
                item.CompleteButton.gameObject.SetActive(item.Driver.IsAccepted);
            }
        }

        public virtual IEnumerable<IPlayerTaskDriver> GetDrivers()
        {
            if (ShowAllTasks)
            {
                return TaskManager.Instance.AllTaskDrivers.Where(d => d.ShowInTaskPanel)
                    .OrderByDescending(d => d.IsComplete);

            }
            else
            {
                return TaskManager.Instance.ActiveTaskDrivers.Where(d => d.ShowInTaskPanel);
            }
        }

        public override void Populate()
        {
            Table.Clear();

            var drivers = GetDrivers();
            var hasDrivers = (drivers.Count() > 0);

            if (ShowWhenNoTasks) ShowWhenNoTasks.SetActive(!hasDrivers);
            if (ShowWhenTasks) ShowWhenTasks.SetActive(hasDrivers);

            if (hasDrivers)
            {
                drivers = drivers.OrderBy(d => d.IsAccepted ? 0 : 1)
                                    .ThenBy(d => d.ActivationContext.ActivationTime)
                                    .ThenBy(d => d.ActivationContext.OpenedTime);
                foreach (var driver in drivers)
                {
                    var task = driver.Task;

                    //var item =
                    AddTaskItem(GetPrefabForTask(task), driver);
                }
            }
        }

        protected virtual void ConfigureTaskItem(TaskItem item, IPlayerTaskDriver driver)
        {
            if (item.Driver.Task is LocationTask)
            {
                ConfigureLocationTaskItem(item);
            }
            else if (item.Driver.Task is CharacterTask)
            {
                ConfigureCharacterTaskItem(item);
            }
            else
            {
                ConfigureDefaultTaskItem(item);
            }
        }

        public override void DidPush()
        {
            TaskManager.Instance.Updated += TaskManager_Updated;
            base.DidPush();
        }

        void TaskManager_Updated(object sender, System.EventArgs e)
        {
            ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    Populate();
                });
        }

        public override void DidPop()
        {
            TaskManager.Instance.Updated -= TaskManager_Updated;
            base.DidPop();
        }
    }

}