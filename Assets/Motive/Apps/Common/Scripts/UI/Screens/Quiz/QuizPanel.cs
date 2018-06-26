// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Gaming;
using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.UI
{
    public abstract class QuizPanel<T> : Panel<T> where T : IMinigameDriver
    {
        protected Logger m_logger { get; set; }
        protected bool m_isListening;

        protected override void Awake()
        {
            this.m_logger = new Logger(this);
            base.Awake();
        }

        public override void Populate(T data)
        {
            DePopulate();
            base.Populate(data);
        }

        public abstract void DePopulate();

    }
}