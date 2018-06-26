// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Models;
using Motive.UI.Framework;
using Motive.Unity.Apps;
using Motive.Unity.Scripting;
using Motive.Unity.Utilities;
using System;
using System.Linq;

namespace Motive.Unity.UI
{
    /// <summary>
    /// A panel that shows a list of Script Runners for the user to launch.
    /// Can be used for "one at a time" script runners (useful for tourism apps)
    /// or DLC-style apps that can launch multiple runners at a time.
    /// </summary>
    public class ScriptRunnerSelectPanel : TablePanel<ScriptRunnerSelectPanel>
    {
        public ScriptRunnerSelectItem ItemPrefab;

        public OptionsDialogPanel StartOrResetDialog;
        public OptionsDialogPanel StopCurrentDigalog;

        public bool ShowInDevelopment;
        public bool ShowDraft;

        ScriptDirectoryItem GetCurrentRunner()
        {
            var currEpisodes = ScriptRunnerManager.Instance.GetRunningItems();

            if (currEpisodes != null)
            {
                return currEpisodes.FirstOrDefault();
            }

            return null;
        }

        void LaunchScript(ScriptDirectoryItem dirItem)
        {
            Action launch = () =>
            {
                if (StartOrResetDialog)
                {
                    if (ScriptRunnerManager.Instance.HasState(dirItem))
                    {
                        StartOrResetDialog.Show(new string[] { "startOver", "resume" }, (opt) =>
                            {
                                switch (opt)
                                {
                                    case "startOver":
                                        StartSessionAndLaunchScript(dirItem, true);
                                        break;
                                    case "resume":
                                        if (!ScriptRunnerManager.Instance.IsRunning(dirItem))
                                        {
                                            StartSessionAndLaunchScript(dirItem, false);
                                        }
                                        else
                                        {
                                            Back();
                                        }
                                    // Script is already running, so don't do anything
                                    break;
                                }
                            });
                    }
                    else
                    {
                        StartSessionAndLaunchScript(dirItem, true);
                    }
                }
                else
                {
                    StartSessionAndLaunchScript(dirItem, true);
                }
            };

            if (!ScriptRunnerManager.Instance.AllowMultiple && StopCurrentDigalog)
            {
                var currTour = GetCurrentRunner();

                if (currTour != null && currTour != dirItem)
                {
                    StopCurrentDigalog.Show(new string[] { "stop", "cancel" }, (opt) =>
                    {
                        if (opt == "stop")
                        {
                            ScriptRunnerManager.Instance.Stop(currTour, launch);
                        }
                    });

                    return;
                }
            }

            launch();
        }

        private void StartSessionAndLaunchScript(ScriptDirectoryItem dirItem, bool reset)
        {
            if (reset)
            {
                ScriptRunnerManager.Instance.Stop(dirItem, () =>
                    {
                        ScriptRunnerManager.Instance.Launch(dirItem);
                    }, true);
            }
            else
            {
                ScriptRunnerManager.Instance.Launch(dirItem);
            }

            Back();
        }

        public override void Populate()
        {
            base.Populate();

            Table.Clear();

            var runners = ScriptRunnerManager.Instance.GetScriptRunners();

            if (runners != null)
            {
                var dirItems = runners.ToList();

                dirItems.Sort((a, b) => a.Order.CompareTo(b.Order));

                foreach (var dirItem in dirItems)
                {
                    if (dirItem.ScriptReference != null)
                    {
                        var script = ScriptManager.Instance.GetScript(dirItem.ScriptReference.ObjectId);

                        if (script != null)
                        {
                            // For closure
                            var _dirItem = dirItem;

                            var item = AddSelectableItem(ItemPrefab, (_item) =>
                                {
                                    LaunchScript(_dirItem);

                                    _item.UpdateState();
                                });

                            item.Text.text = dirItem.Title ?? script.Name;

                            if (item.Description)
                            {
                                item.Description.text = dirItem.Description;
                            }

                            if (item.Image)
                            {
                                if (item.Image && dirItem.BackgroundImageUrl != null)
                                {
                                    item.Image.gameObject.SetActive(true);
                                    ImageLoader.LoadImageOnThread(dirItem.BackgroundImageUrl, item.Image);
                                }
                                else
                                {
                                    item.Image.gameObject.SetActive(false);
                                }
                            }

                            if (item.DurationBlock)
                            {
                                if (dirItem.EstimatedDuration.HasValue)
                                {
                                    item.DurationBlock.SetActive(true);

                                    if (dirItem.EstimatedDuration.Value.Hours > 0)
                                    {
                                        if (dirItem.EstimatedDuration.Value.Minutes > 0)
                                        {
                                            item.DurationText.text = string.Format("{0}h {1:00}min",
                                                dirItem.EstimatedDuration.Value.Hours,
                                                dirItem.EstimatedDuration.Value.Minutes);
                                        }
                                        else
                                        {
                                            item.DurationText.text = string.Format("{0} hour{1}",
                                                dirItem.EstimatedDuration.Value.Hours,
                                                dirItem.EstimatedDuration.Value.Hours == 1 ? "" : "s");
                                        }
                                    }
                                    else
                                    {
                                        item.DurationText.text = string.Format("{0} minutes",
                                            dirItem.EstimatedDuration.Value.Minutes);
                                    }
                                }
                                else
                                {
                                    item.DurationBlock.SetActive(false);
                                }
                            }

                            if (item.DisplayWhenActive)
                            {
                                item.DisplayWhenActive.gameObject.SetActive(ScriptRunnerManager.Instance.IsRunning(dirItem));
                            }
                        }
                    }
                }
            }
        }
    }
}