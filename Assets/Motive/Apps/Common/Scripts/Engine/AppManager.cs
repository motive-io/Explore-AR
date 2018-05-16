// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Utilities;
using Motive.UI.Framework;
using System;
using System.Collections.Generic;
using Motive.Core.WebServices;
using Motive.Core.Scripting;
using Motive;
using Motive.Unity.UI;
using Motive.Unity.Scripting;
using Motive.Unity.Storage;

namespace Motive.Unity.Apps
{
    /// <summary>
    /// Manages the App lifecycle.
    /// </summary>
    public class AppManager : Singleton<AppManager>
    {
        /// <summary>
        /// Raised when the app starts.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Raised when the Script Manager starts running scripts.
        /// </summary>
        public event EventHandler ScriptsStarted;
        public event EventHandler Reloading;
        public event EventHandler Resetting;

        public event EventHandler Initialized;
        public event EventHandler LaunchFailed;

        private List<Action<Action>> m_onLoadActions = new List<Action<Action>>();

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Resets the app, deleting any saved game data.
        /// </summary>
        public void Reset()
        {
            Reset(false);
        }

        /// <summary>
        /// Reloads reloads the app data from the Motive server without
        /// resetting.
        /// </summary>
        public void Reload()
        {
            var loadingPanel = PanelManager.Instance.Push<LoadingPanel>();

            ScriptManager.Instance.StopAllProcessors(() =>
            {
                ReloadFromServer(loadingPanel);
            });
        }

        void ReloadFromServer(LoadingPanel loadingPanel)
        {
            if (Reloading != null)
            {
                Reloading(this, EventArgs.Empty);
            }

            WebServices.Instance.ReloadFromServer(
                () =>
                {
                    Action onComplete = () =>
                    {
                        Action runScripts = () =>
                        {
                            try
                            {
                                ScriptManager.Instance.RunScripts();

                                if (ScriptsStarted != null)
                                {
                                    ScriptsStarted(this, EventArgs.Empty);
                                }
                            }
                            catch (Exception x)
                            {
                                if (LaunchFailed != null)
                                {
                                    LaunchFailed(this, new UnhandledExceptionEventArgs(x, false));
                                }
                            }
                        };

                        if (loadingPanel)
                        {
                            loadingPanel.OnClose = () =>
                            {
                                runScripts();
                            };

                            loadingPanel.MediaReady();
                        }
                        else
                        {
                            runScripts();
                        }

                    };

                    if (m_onLoadActions.Count > 0)
                    {
                        BatchProcessor iter = new BatchProcessor(m_onLoadActions.Count);

                        iter.OnComplete(onComplete);

                        foreach (var action in m_onLoadActions)
                        {
                            action(() => { iter++; });
                        }
                    }
                    else
                    {
                        onComplete();
                    }

                    Platform.Instance.DownloadsComplete();
                },
                (reason) =>
                {
                    loadingPanel.DownloadError(reason);
                });
        }

        public void Initialize()
        {
            if (!IsInitialized)
            {
                WebServices.Instance.DownloadError += (sender, args) =>
                {
                    if (LaunchFailed != null)
                    {
                        LaunchFailed(this, args);
                    }
                };

                IsInitialized = true;

                if (Initialized != null)
                {
                    Initialized(this, EventArgs.Empty);
                }
            }
        }

        public void Start()
        {
            LoadingPanel loadingPanel = null;

            if (PanelManager.Instance)
            {
                loadingPanel = PanelManager.Instance.Push<LoadingPanel>();
            }

            ReloadFromServer(loadingPanel);

            if (Started != null)
            {
                Started(this, EventArgs.Empty);
            }

            Platform.Instance.FireInteractionEvent("appStart");
        }

        public void OnLoadComplete(Action handler)
        {
            OnLoadComplete((callback) =>
            {
                handler();
                callback();
            });
        }

        public void OnLoadComplete(Action<Action> handler)
        {
            m_onLoadActions.Add(handler);
        }

        public void Reset(bool skipReload)
        {
            if (Resetting != null)
            {
                Resetting(this, EventArgs.Empty);
            }

            LoadingPanel loadingPanel = null;

            if (PanelManager.Instance)
            {
                loadingPanel = PanelManager.Instance.Push<LoadingPanel>();
            }

            ScriptManager.Instance.Reset(() =>
            {
                if (skipReload)
                {
                    ScriptManager.Instance.RunScripts();
                }
                else
                {
                    ReloadFromServer(loadingPanel);
                }
            });
        }

        public void Nuke()
        {
            ScriptManager.Instance.Abort();

            StorageManager.DeleteGameFolder();

            Application.Quit();
        }
    }
}
