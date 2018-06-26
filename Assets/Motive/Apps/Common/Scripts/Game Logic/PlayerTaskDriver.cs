// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Scripting;
using Motive.Core.Timing;
using Motive.Gaming.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Gaming
{
    public interface IPlayerTaskDriver
    {
        event EventHandler Started;
        event EventHandler Stopped;

        bool IsGiveTask { get; }
        bool IsTakeTask { get; }
        bool IsConfirmTask { get; }
        bool IsExchangeTask { get; }
        bool IsAccepted { get; }
        PlayerTask Task { get; }
        ResourceActivationContext ActivationContext { get; }
        Timer TimeoutTimer { get; }
        void SetFocus(bool focus);
        void Action();
        void DidComplete();
        void Start();
        void Stop();
        void Accept();
        void OnAccept();
        void Update(PlayerTask task);

        bool ShowInTaskPanel { get; }

        bool IsComplete { get; }
    }

    public abstract class PlayerTaskDriver : IPlayerTaskDriver
    {
        public static Func<IMinigameDriver> CreateDefaultMinigameDriver;

        public event EventHandler Started;

        public event EventHandler Stopped;

        public event EventHandler Updated;

        public abstract bool IsGiveTask { get; }

        public abstract bool IsTakeTask { get; }

        public abstract bool IsExchangeTask { get; }

        public abstract bool IsConfirmTask { get; }

        public virtual bool IsAccepted
        {
            get { return ActivationContext.CheckEvent("accept"); }
        }

        public virtual bool IsComplete
        {
            get { return ActivationContext.CheckEvent("complete"); }
        }

        public ResourceActivationContext ActivationContext { get; private set; }

        public Timer TimeoutTimer { get; protected set; }

        public abstract void Action();

        public abstract void Complete();

        public abstract void DidComplete();

        public abstract void Start();

        public abstract void Stop();

        public virtual void Accept()
        {
            this.ActivationContext.FireEvent("accept");
            this.OnAccept();
        }

        public virtual void OnAccept()
        {
            this.Start();
        }

        public abstract void SetFocus(bool focus);

        public abstract void StartMinigame();

        public abstract void StopMinigame();

        public abstract void Update(PlayerTask task);

        public abstract bool ShowInTaskPanel { get; }

        protected PlayerTask m_task;

        public PlayerTask Task { get { return m_task; } }

        protected virtual void OnStarted()
        {
            if (Started != null)
            {
                Started(this, EventArgs.Empty);
            }
        }

        protected virtual void OnStopped()
        {
            if (Stopped != null)
            {
                Stopped(this, EventArgs.Empty);
            }
        }

        protected virtual void OnUpdated()
        {
            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        protected virtual void OnTimeout()
        {
            if (Task.Action == TaskAction.Wait)
            {
                TaskManager.Instance.Complete(this);
            }
            else
            {
                TaskManager.Instance.ClosePlayerTask(this);
            }
        }

        public PlayerTaskDriver(ResourceActivationContext context, PlayerTask task)
        {
            ActivationContext = context;
            m_task = task;
        }
    }

    public class PlayerTaskDriver<T> : PlayerTaskDriver
        where T : PlayerTask
    {
        public new T Task { get; private set; }

        public PlayerTaskDriver(ResourceActivationContext context, T task)
            : base(context, task)
        {
            Task = task;
        }

        public override void Update(PlayerTask task)
        {
            Task = (T)task;

            OnUpdated();
        }

        public override void StartMinigame()
        {
            // No-op?
        }

        public override void StopMinigame()
        {
            // No-op?
        }

        public override void Start()
        {
            TimeoutTimer = ActivationContext.StartTimeoutTimer(Task.Timeout, OnTimeout);

            if (Task.IsAutoplay)
            {
                StartMinigame();
            }

            OnStarted();
        }

        public override void Stop()
        {
            if (TimeoutTimer != null)
            {
                TimeoutTimer.Cancel();
                TimeoutTimer = null;
            }

            OnStopped();
        }

        public override bool IsGiveTask
        {
            get
            {
                return TaskManager.Instance.IsGiveTask(Task);
            }
        }

        public override bool IsTakeTask
        {
            get
            {
                return TaskManager.Instance.IsTakeTask(Task);
            }
        }

        public override bool IsExchangeTask
        {
            get
            {
                return TaskManager.Instance.IsExchangeTask(Task);
            }
        }

        public override bool IsConfirmTask
        {
            get
            {
                return TaskManager.Instance.IsConfirmTask(Task);
            }
        }

        public Collectible FirstCollectible
        {
            get
            {
                return ValuablesCollection.GetFirstCollectible(Task.ActionItems);
            }
        }

        public override bool ShowInTaskPanel
        {
            get { return true; }
        }

        public override void SetFocus(bool focus)
        {
            // Noop
        }

        public override void Action()
        {
            Complete();
        }

        public override void Complete()
        {
            TaskManager.Instance.Complete(this);
        }

        public override void DidComplete()
        {
        }

        /*
        PlayerTask IPlayerTaskDriver.Task
        {
            get { return Task; }
        }*/
    }

    public class PlayerTaskDriver<T, M> : PlayerTaskDriver<T>
        where T : PlayerTask
        where M : class, IMinigameDriver
    {
        public PlayerTaskDriver(ResourceActivationContext context, T task)
            : base(context, task)
        {
            if (task.Minigame != null)
            {
                MinigameDriver = GetMinigameDriver(task.Minigame);
            }
            else
            {
                MinigameDriver = GetDefaultMinigameDriver();
            }
        }

        protected virtual M GetDefaultMinigameDriver()
        {
            if (CreateDefaultMinigameDriver != null)
            {
                return CreateDefaultMinigameDriver() as M;
            }

            return null;
        }

        protected virtual M GetMinigameDriver(ITaskMinigame minigame)
        {
            if (minigame is MultipleChoiceQuiz)
            {
                var mcQuiz = minigame as MultipleChoiceQuiz;
                return new MultipleChoiceQuizMinigameDriver(this, mcQuiz) as M;
            }
            else if (minigame is FreeResponseQuiz)
            {
                var quiz = minigame as FreeResponseQuiz;
                return new FreeResponseMinigameDriver(this, quiz) as M;
            }

            return GetDefaultMinigameDriver();
        }

        public M MinigameDriver { get; private set; }

        public override void Action()
        {
            if (MinigameDriver != null)
            {
                MinigameDriver.Action();
            }
            else
            {
                base.Action();
            }
        }

        public override void SetFocus(bool focus)
        {
            if (MinigameDriver != null)
            {
                MinigameDriver.SetFocus(focus);
            }
        }

        public override void StartMinigame()
        {
            if (MinigameDriver != null)
            {
                MinigameDriver.Start();
            }

            TaskManager.Instance.StartMinigame(this);
        }

        public override void StopMinigame()
        {
            if (MinigameDriver != null)
            {
                MinigameDriver.Stop();
            }

            TaskManager.Instance.StopMinigame(this);
        }
    }

}