// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using Motive.UI.Framework;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Panel that lets the user log into Motive and select a config to run.
    /// </summary>
    public class DynamicConfigLoginPanel : LoginPanel
    {
        [Space(10)]
        [Header("Dynamic Config Login Options")]
        [Space(20)]
        public Button ClosePanelButton;

        [Header("Select a Space")]
        public GameObject SelectASpacePanel;
        public Dropdown SpacesDropdown;
        public Dropdown ConfigsDropdown;
        public Button ConfirmSpaceSelection;
        public Button CancelSpaceSelection;

        [Header("Logged in and Space Selected")]
        public GameObject ShowSelectedSpacePanel;
        public Text SelectedSpaceText;

        [Header("Logged in, Selected Space Load Error")]
        public GameObject LoadSpaceErrorPanel;
        public Text LoadSpaceErrorDetails;

        private SpaceInfoModel m_selectedSpace;
        private ConfigInfoModel m_selectedConfig;

        private string k_selectASpaceString = "-- SELECT A PROJECT --";
        private string k_selectAConfigString = "-- SELECT A CONFIG --";

        protected override void Awake()
        {
            base.Awake();

            SpacesDropdown.onValueChanged.AddListener(SpaceChanged);
            ConfigsDropdown.onValueChanged.AddListener(ConfigChanged);
        }

        void SpaceChanged(int idx)
        {
            idx--; // accounts for default in dropdown

            ConfigsDropdown.ClearOptions();

            ConfirmSpaceSelection.interactable = false;
            m_selectedSpace = null;
            m_selectedConfig = null;

            if (idx >= 0 && idx < DynamicConfig.Instance.UserSpaces.Count)
            {
                m_selectedSpace = DynamicConfig.Instance.UserSpaces[idx];

                PopulateConfigsDropdown(m_selectedSpace);
            }
        }

        void ConfigChanged(int idx)
        {
            idx--; // accounts for default in dropdown

            m_selectedConfig = null;

            if (m_selectedSpace != null &&
                idx >= 0 && idx < m_selectedSpace.Configs.Length)
            {
                m_selectedConfig = m_selectedSpace.Configs[idx];

                ConfirmSpaceSelection.interactable = true;
            }
            else
            {
                ConfirmSpaceSelection.interactable = false;
            }
        }

        public override void Login()
        {
            ClearError();

            Action<bool> onSignIn = (success) =>
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                    {
                        if (LoginButton)
                        {
                            LoginButton.interactable = true;
                        }

                        if (!success)
                        {
                            Error("Login Failed.");
                        }
                        else
                        {
                            PopulateSpacesDropdown(CheckLoginState);
                        }
                    });
            };

            if (LoginButton)
            {
                LoginButton.interactable = false;
            }

            DynamicConfig.Instance.Login(this.UserName.text, this.Password.text, onSignIn);
        }

        public override void Logout()
        {
            DynamicConfig.Instance.Logout();
            CheckLoginState();
        }

        private void PopulateConfigsDropdown(SpaceInfoModel space)
        {
            ConfigsDropdown.ClearOptions();

            var currConfig = DynamicConfig.Instance.SelectedConfig;

            if (space.Configs != null && space.Configs.Length > 0)
            {
                var opts = space.Configs.Select(c => new Dropdown.OptionData(c.Title)).ToList();

                var optsAndDefault = opts;

                optsAndDefault.Insert(0, new Dropdown.OptionData()
                {
                    text = k_selectAConfigString
                });

                ConfigsDropdown.AddOptions(optsAndDefault);

                var selectedConfigIdx = 1; // set to '1' to co-operate with having a default item in list of dropdown opts

                if (currConfig != null &&
                    m_selectedSpace != null &&
                    m_selectedSpace.Configs != null)
                {
                    for (var cidx = 0; cidx < m_selectedSpace.Configs.Length; cidx++)
                    {
                        var config = m_selectedSpace.Configs[cidx];

                        if (config.Name == currConfig.Name)
                        {
                            selectedConfigIdx = cidx + 1;
                            m_selectedConfig = config;

                            break;
                        }
                    }
                }

                if (opts.Count > 0)
                {
                    // Listener only fires if we actually change this value
                    if (ConfigsDropdown.value != selectedConfigIdx)
                    {
                        ConfigsDropdown.value = selectedConfigIdx;
                    }
                    else
                    {
                        ConfigChanged(selectedConfigIdx);
                    }
                }
            }
        }

        /// <summary>
        /// Hits API endpoint to get spaces for logged-in user and populates dropdown
        /// </summary>
        internal void PopulateSpacesDropdown(Action onComplete = null)
        {
            m_selectedSpace = null;
            m_selectedConfig = null;

            var currSpace = DynamicConfig.Instance.SelectedSpace;

            DynamicConfig.Instance.GetSpaces((success) =>
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    if (!success)
                    {
                    // todo What happens if can't hit endpoint for get userspaces? logout?
                    return;
                    }

                    var spaces = DynamicConfig.Instance.UserSpaces;
                    if (spaces == null)
                    {
                    // todo what do if no spaces returned? (but successfully hit endpoint)
                    return;
                    }

                    var spacesOptions = new List<Dropdown.OptionData>();

                    var selectedSpaceIdx = -1;

                    if (spaces.Count > 0)
                    {
                        var sidx = 0;

                        foreach (var s in spaces)
                        {
                            if (currSpace != null && s.Name == currSpace.Name)
                            {
                                m_selectedSpace = s;
                            // Make room for space select
                            selectedSpaceIdx = sidx + 1;
                            }

                            spacesOptions.Add(new Dropdown.OptionData(s.Title));

                            sidx++;
                        }
                    }

                    ConfigsDropdown.ClearOptions();
                    SpacesDropdown.ClearOptions();

                    var spaceOptAndDefault = spacesOptions;

                    spaceOptAndDefault.Insert(0, new Dropdown.OptionData()
                    {
                        text = k_selectASpaceString
                    });
                    SpacesDropdown.AddOptions(spaceOptAndDefault);

                    if (selectedSpaceIdx > 0)
                    {
                    // Listener only fires if we actually change this value
                    if (SpacesDropdown.value != selectedSpaceIdx)
                        {
                            SpacesDropdown.value = selectedSpaceIdx;
                        }
                        else
                        {
                            SpaceChanged(selectedSpaceIdx);
                        }
                    }

                    if (onComplete != null) onComplete(); // check login state
            });
            });
        }

        public override void Populate()
        {
            //var isAuthenticated = MotiveAuthenticator.Instance.IsUserAuthenticated;
            if (DynamicConfig.Instance.CurrentState != DynamicConfig.State.LoggedOut)
            {
                PopulateSpacesDropdown();
            }

            base.Populate();
        }

        public void SelectSpaceButtonHandler()
        {
            if (m_selectedSpace == null || m_selectedConfig == null)
            {
                //return;
            }

            /*
            var selectedInt = SpacesDropdown.value;
            if (selectedInt == 0) return;

            var space = DynamicConfig.Instance.UserSpaces[selectedInt - 1];

            // bugfix, dropdown list doesn't delete itself after first pass, causes issues
            var dropdownTr = SpacesDropdown.transform.Find("Dropdown List");
            if (dropdownTr)
                Destroy(dropdownTr.gameObject);
            */

            Close();
            DynamicConfig.Instance.SetStateLoggedInSpaceSelected(m_selectedSpace, m_selectedConfig);
        }

        internal override void CheckLoginState()
        {
            base.CheckLoginState();
            CheckSpaceSelectedState();
        }

        public void CheckSpaceSelectedState()
        {
            ConfirmSpaceSelection.interactable = false;

            LoadSpaceErrorPanel.SetActive(false);
            switch (DynamicConfig.Instance.CurrentState)
            {
                case DynamicConfig.State.LoggedOut:
                    ClosePanelButton.gameObject.SetActive(false);
                    SelectASpacePanel.SetActive(false);
                    ShowSelectedSpacePanel.SetActive(false);
                    break;
                case DynamicConfig.State.LoggedInNoSpace:
                case DynamicConfig.State.LoggedInSpaceSelected:
                    ClosePanelButton.gameObject.SetActive(true);
                    SelectASpacePanel.SetActive(true);
                    ShowSelectedSpacePanel.SetActive(false);

                    ConfirmSpaceSelection.interactable = (m_selectedConfig != null && m_selectedSpace != null);
                    break;
                /*
            case DynamicConfig.State.LoggedInSpaceSelected:
                ClosePanelButton.gameObject.SetActive(true);
                SelectedSpaceText.text = DynamicConfig.Instance.SelectedSpace.Title;
                SelectASpacePanel.SetActive(false);
                ShowSelectedSpacePanel.SetActive(true);
                ConfirmSpaceSelection.enabled = true;
                break;
                 */
                case DynamicConfig.State.LoggedInSpaceError:
                    LoadSpaceErrorPanel.SetActive(true);
                    ClosePanelButton.gameObject.SetActive(false);
                    SelectASpacePanel.SetActive(true);
                    ShowSelectedSpacePanel.SetActive(false);
                    break;
            }
        }

        public void ShowDownloadErrorPopup(object sender, EventArgs e)
        {
            PanelManager.Instance.Push<DynamicConfigLoginPanel>();
            DynamicConfig.Instance.SetStateLoggedInSpaceError(null);
        }
    }

}