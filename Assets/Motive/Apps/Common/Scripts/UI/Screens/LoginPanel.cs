// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.WebServices;
using Motive.UI.Framework;
using Motive.Unity.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Handles user login flow.
    /// </summary>
    public class LoginPanel : Panel
    {
        public Button LoginButton;
        public GameObject LoginPane;
        public GameObject UserInfoPane;

        public InputField UserName;
        public InputField Password;

        public Text UserInfoName;

        public Text ErrorText;

        protected virtual void Start()
        {
            CheckLoginState();
        }

        public virtual void Login()
        {
            ClearError();

            if (LoginButton)
            {
                LoginButton.interactable = false;
            }

            WebServices.Instance.UserManager.Login(UserName.text, Password.text, (user) =>
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    if (LoginButton)
                    {
                        LoginButton.interactable = true;
                    }

                    if (user == null)
                    {
                        Error("Login failed.");
                    }
                    else
                    {
                        Motive.Platform.Instance.FireInteractionEvent("login");

                    //CheckLoginState();
                    // This panel should take you back to a panel
                    // that shows the account info.
                    Back();
                    }
                });
            });
        }

        public override void DidShow()
        {
            if (LoginButton)
            {
                LoginButton.interactable = true;
            }

            CheckLoginState();
        }

        public virtual void Logout()
        {
            WebServices.Instance.UserManager.Logout();

            CheckLoginState();
        }

        internal virtual void CheckLoginState()
        {
            bool isAuthenticated = MotiveAuthenticator.Instance.IsUserAuthenticated;

            ClearError();
            LoginPane.SetActive(!isAuthenticated);
            UserInfoPane.SetActive(isAuthenticated);

            if (isAuthenticated)
            {
                UserInfoName.text = MotiveAuthenticator.Instance.UserName;
            }
        }

        internal void ClearError()
        {
            if (ErrorText)
            {
                ErrorText.gameObject.SetActive(false);
            }
        }

        internal void Error(string message)
        {
            if (ErrorText)
            {
                ErrorText.gameObject.SetActive(true);
                ErrorText.text = message;
            }
        }
    }

}