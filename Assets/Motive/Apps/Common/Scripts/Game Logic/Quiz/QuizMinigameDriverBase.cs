// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Gaming.Models;
using Motive.Unity.Globalization;

namespace Motive.Unity.Gaming
{
    public abstract class QuizMinigameDriverBase<T> :
        ILocationMinigameDriver where T : QuizBase,
        IMediaItemProvider
    {
        public IPlayerTaskDriver TaskDriver { get; private set; }
        public T Quiz { get; private set; }

        public int AttemptsTried { get; set; }
        
        public string ImageUrl
        {
            get
            {
                return Quiz.ImageUrl;
            }
        }

        protected QuizMinigameDriverBase(IPlayerTaskDriver taskDriver, T quizMinigame)
        {
            this.TaskDriver = taskDriver;
            this.Quiz = quizMinigame;
        }

        /// <summary>
        /// What action takes place when this minigame 'starts'
        /// </summary>
        public abstract void Action();

        public virtual void SetFocus(bool focus)
        {
        }

        public virtual void Start()
        {
        }

        public virtual void Stop()
        {
        }


        public virtual string ActionButtonText
        {
            get { return Localize.GetLocalizedString("Task.InRange", "In Range"); }
        }

        public virtual string OutOfRangeActionButtonText
        {
            get { return Localize.GetLocalizedString("Task.OutOfRange", "Out of Range"); }
        }

        public virtual bool ShowActionButton
        {
            // By default, show action button for any action except "in range"
            get
            {
                return
                    TaskDriver.Task.Action != TaskAction.InRange &&
                    TaskDriver.Task.Action != TaskAction.Wait;
            }
        }

        public virtual bool ShowMapAnnotation
        {
            get { return !TaskDriver.Task.IsHidden; }
        }
    }
}