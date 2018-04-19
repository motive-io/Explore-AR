// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Json;
using Motive.Core.Storage;
using Motive.Core.WebServices;
using Motive.UI.Framework;
using Motive.Unity.Apps;
using Motive.Unity.Storage;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive
{
    public class ConfigInfoModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
    }

    public class SpaceInfoModel
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public ConfigInfoModel[] Configs { get; set; }
    }

    public class GetSpacesModel
    {
        public SpaceInfoModel[] Spaces { get; set; }
    }

    internal class DynamConfigState
    {
        public SpaceInfoModel SelectedSpace { get; set; }
        public ConfigInfoModel SelectedConfig { get; set; }
        public DynamicConfig.State State { get; set; }
    }

    /// <summary>
    /// Allows user to set Motive Config settings during run-time.
    /// </summary>
    class DynamicConfig : SingletonComponent<DynamicConfig>
    {
        public enum DynamicConfigLoginSetting
        {
            Yes,
            No,
            OnlyInDebugMode
        }

        public static bool UseDynamicConfig
        {
            get
            {
                if (Instance != null)
                {
                    return Instance.UseDynamicConfigLogin;
                }

                return false;
            }
        }

        [Tooltip("If true, Dynamic Config Login will be triggered to allow user to sign into their Motive account and select a project space to load")]
        public DynamicConfigLoginSetting Setting;

        public bool UseDynamicConfigLogin
        {
            get
            {
                return Setting == DynamicConfigLoginSetting.Yes ||
                      (Setting == DynamicConfigLoginSetting.OnlyInDebugMode && BuildSettings.IsDebug);
            }
        }

        private Logger m_logger;

        private IStorageAgent m_fileAgent { get; set; }

        private IStorageAgent FileAgent
        {
            get
            {
                return m_fileAgent ?? (m_fileAgent = new FileStorageAgent(StorageManager.GetFilePath("config", "dynamicConfig.json")));
            }

            set { m_fileAgent = value; }
        }

        //private bool SpaceAlreadyLoaded;
        public List<SpaceInfoModel> UserSpaces { get; private set; }
        public SpaceInfoModel SelectedSpace { get; private set; }
        public ConfigInfoModel SelectedConfig { get; private set; }
        public State CurrentState { get; private set; }
        public string ErrorMessage { get; private set; }

        public enum State
        {
            LoggedOut,
            PublicSpace,
            LoggedInNoSpace,
            LoggedInSpaceSelected,
            LoggedInSpaceError
        }

        public void LoadSavedState()
        {
            var state = JsonHelper.Deserialize<DynamConfigState>(FileAgent);

            if (state != null && state.SelectedSpace != null)
            {
                SelectedSpace = state.SelectedSpace;
                SelectedConfig = state.SelectedConfig;
                CurrentState = state.State;
            }
        }

        public void Save()
        {
            if (SelectedSpace == null)
            {
                JsonHelper.Serialize(FileAgent, new DynamConfigState() { SelectedSpace = null, SelectedConfig = null, });
            }
            else
            {
                JsonHelper.Serialize(FileAgent, new DynamConfigState() { SelectedSpace = SelectedSpace, SelectedConfig = SelectedConfig, State = CurrentState });
            }
        }

        public void SetStateLoggedOut(bool save = true)
        {
            CurrentState = State.LoggedOut;
            SelectedSpace = null;
            if (save) Save();
        }

        public void SetStateLoggedInNoSpace(bool save = true)
        {
            CurrentState = State.LoggedInNoSpace;
            SelectedSpace = null;
            if (save) Save();
        }

        public void SetStateLoggedInSpaceSelected(SpaceInfoModel _space, ConfigInfoModel _config, bool save = true)
        {
            CurrentState = State.LoggedInSpaceSelected;
            SelectedSpace = _space;
            SelectedConfig = _config;

            if (save) Save();

            PopulateWebServiceFields();

            if (SpaceSelected != null)
            {
                SpaceSelected(this, EventArgs.Empty);
            }
        }

        public void SetStateLoggedInSpaceError(string message, bool save = true)
        {
            ErrorMessage = message;
            CurrentState = State.LoggedInSpaceError;
        }

        public void SetStatePublicSpace(SpaceInfoModel _space, ConfigInfoModel _config, bool save = true)
        {
            CurrentState = State.PublicSpace;
            SelectedSpace = _space;
            SelectedConfig = _config;

            if (save) Save();

            PopulateWebServiceFields();

            if (SpaceSelected != null)
            {
                SpaceSelected(this, EventArgs.Empty);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            m_logger = new Logger(this);
            UserSpaces = new List<SpaceInfoModel>();

            if (!UseDynamicConfigLogin) return;
        }

        protected void DetermineCurrentState(bool save = true)
        {
            var isAuthenticated = MotiveAuthenticator.Instance.IsUserAuthenticated;


            if (CurrentState == State.PublicSpace && this.SpaceSelected != null && this.SelectedConfig != null)
            {
                SetStatePublicSpace(SelectedSpace, SelectedConfig, save);
            }
            else if (!isAuthenticated)
            {
                SetStateLoggedOut(save);
            }
            else if (SelectedSpace == null || SelectedConfig == null)
            {
                SetStateLoggedInNoSpace(save);
            }
            else
            {
                SetStateLoggedInSpaceSelected(SelectedSpace, SelectedConfig, save);
            }
        }

        public void Initialize()
        {
            if (UseDynamicConfigLogin)
            {
                AppManager.Instance.LaunchFailed += Instance_LaunchFailed;
                LoadSavedState();
                DetermineCurrentState(false);

                if (CurrentState == State.LoggedInSpaceSelected || CurrentState == State.PublicSpace)
                {
                    return;
                }

                var dynLogPanel = PanelManager.Instance.Push<DynamicConfigLoginPanel>(animate: false);
                if (dynLogPanel == null)
                {
                    m_logger.Error("Cannot use DynamicConfig class without a DynamicConfigLoginPanel game object.");
                }
            }
            else
            {
                if (SpaceSelected != null) SpaceSelected(this, EventArgs.Empty);
            }
        }

        public void Retry()
        {
            PanelManager.Instance.Push<DynamicConfigLoginPanel>(animate: true);
        }

        void Instance_LaunchFailed(object sender, EventArgs e)
        {
            SetStateLoggedInSpaceError("Error launching scripts.");

            //PanelManager.Instance.Push<DynamicConfigLoginPanel>(animate: false);
        }

        public void Login(string userName, string password, Action<bool> onComplete = null)
        {
            Action<bool> onCompleteWrapper = (success) =>
            {
                if (success)
                {
                    SetStateLoggedInNoSpace();
                }
                else
                {
                    SetStateLoggedOut();
                }

                if (onComplete != null) onComplete(success);
            };
            MotiveAuthenticator.Instance.AuthenticateUser("motive.portal", userName, password, onCompleteWrapper);
        }

        public void Logout()
        {
            WebServices.Instance.UserManager.Logout();
            SetStateLoggedOut();
            Save();
        }

        /// <summary>
        /// Make GET call to receive Project Spaces for current signed in user.
        /// Assigns spaces to this.UserSpaces.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSpaces(Action<bool> onComplete)
        {
            var uriString = WebServices.Instance.MotiveUrl + "/api/account/projectSpaces";
            var call = new ServiceCall(new Uri(uriString));

            Action<AuthenticatorCallStatus> onCompleteWrapper = (status) =>
            {
                var success = (status == AuthenticatorCallStatus.Success);

                if (success)
                {
                    UserSpaces.Clear();

                    var spacesWrapper = JsonHelper.GetReader(call.ResponseText).Deserialize<GetSpacesModel>();
                    if (spacesWrapper != null)
                    {
                        UserSpaces.AddRange(spacesWrapper.Spaces);
                    }
                }

                if (onComplete != null) onComplete(success);

            };

            MotiveAuthenticator.Instance.MakeUserCall(call, onCompleteWrapper);

            return null;
        }

        public event EventHandler SpaceSelected;

        internal void PopulateWebServiceFields()
        {
            if (SelectedSpace == null)
            {
                throw new Exception("No space selected");
            }

            WebServices.Instance.SetSpaceName(SelectedSpace.Name);
            WebServices.Instance.SetConfig(SelectedConfig.Name);
        }
    }
}