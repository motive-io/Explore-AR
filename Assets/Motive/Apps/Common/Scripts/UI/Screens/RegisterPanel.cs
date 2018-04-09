// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Utilities;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Used to register your users in your app's user domain.
    /// </summary>
    public class RegisterPanel : Panel
    {
        public InputField UserName;
        public InputField Password;
        public InputField RepeatPassword;
        public InputField Email;

        public Text ErrorText;

        public void Register()
        {
            ErrorText.gameObject.SetActive(false);

            if (Password.text != RepeatPassword.text)
            {
                Error("Passwords don't match.");

                return;
            }

            WebServices.Instance.UserManager.RegisterUser(
                UserName.text, Email.text,
                Password.text, RepeatPassword.text, (success) =>
                {
                    if (success)
                    {
                        Motive.Platform.Instance.FireInteractionEvent("createUserAccount");

                        WebServices.Instance.UserManager.Login(UserName.text, Password.text, (user) =>
                        {
                            ThreadHelper.Instance.CallOnMainThread(() =>
                            {
                                Back();
                            });
                        });
                    }
                    else
                    {
                        Error("Could not create user " + UserName.text);
                    }
                });
        }

        void Error(string message)
        {
            ErrorText.gameObject.SetActive(true);
            ErrorText.text = message;
        }
    }

}