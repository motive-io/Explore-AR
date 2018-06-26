// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Globalization;

namespace Motive.Unity.Gaming
{
    public abstract class LocationMinigameDriverBase : ILocationMinigameDriver
    {
        public LocationTaskDriver TaskDriver { get; private set; }
        public abstract bool ShowMapAnnotation { get; }
        public abstract bool ShowActionButton { get; }

        public virtual string ActionButtonText
        {
            get { return Localize.GetLocalizedString("Task.InRange", "In Range"); }
        }

        public virtual string OutOfRangeActionButtonText
        {
            get { return Localize.GetLocalizedString("Task.OutOfRange", "Out of Range"); }
        }

        protected LocationMinigameDriverBase(LocationTaskDriver taskDriver)
        {
            this.TaskDriver = taskDriver;
        }

        public virtual void Action()
        {
            TaskManager.Instance.Complete(this.TaskDriver);
        }

        public virtual void SetFocus(bool focus)
        {
        }

        public virtual void Start()
        {
        }

        public virtual void Stop()
        {
        }
    }

}