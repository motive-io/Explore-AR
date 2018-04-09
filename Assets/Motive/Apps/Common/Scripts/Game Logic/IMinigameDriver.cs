// Copyright (c) 2018 RocketChicken Interactive Inc.

namespace Motive.Unity.Gaming
{
    public interface IMinigameDriver
    {
        /// <summary>
        /// What happens when the Task Action is taken.
        /// </summary>
        void Action();

        /// <summary>
        /// On Task Activation, do any setup, etc.
        /// </summary>
        void Start();

        /// <summary>
        /// on Task Deactivation, do any takedown, etc.
        /// </summary>
        void Stop();

        void SetFocus(bool focus);
    }

}