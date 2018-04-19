// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System.Collections;
using Motive.Core.Utilities;
using System.Collections.Generic;
using Motive.Core.Scripting;
using System.Linq;
using System;
using Motive.Gaming.Models;
using Motive.AR.Models;
using Motive.Core.Storage;
using Motive.Core.Json;
using Motive.UI.Framework;
using Motive.Unity.Scripting;
using Motive.Unity.Gaming;
using Motive.Unity.UI;
using Motive.Unity.Utilities;
using Motive.Unity.Storage;

namespace Motive.Unity.Gaming
{
    public static class TaskAction
    {
        public const string InRange = "in_range";
        public const string Put = "put";
        public const string Take = "take";
        public const string Wait = "wait";
        public const string Exchange = "exchange";
        public const string Confirm = "confirm";
    }

    class TaskContainer
    {
        public PlayerTask Task { get; set; }
        public ResourceActivationContext Context { get; set; }
    }

    /// <summary>
    /// Manages all tasks and assignments.
    /// </summary>
    public class TaskManager : Singleton<TaskManager>
    {
        class CompleteObjectiveState
        {
            public TaskObjective Objective { get; set; }
            public int Count { get; set; }
        }

        class TaskManagerState
        {
            public CompleteObjectiveState[] CompletedObjectives;
            public string[] ActivatedObjectives;
        }

        /// <summary>
        /// If true, automatically accepts tasks.
        /// </summary>
        public bool AutoAcceptTask { get; set; }

        Dictionary<string, IPlayerTaskDriver> m_drivers;
        Dictionary<string, AssignmentDriver> m_assignments;
        SetDictionary<string, IPlayerTaskDriver> m_outcomeDrivers;
        Dictionary<string, CompleteObjectiveState> m_completedObjectives;
        HashSet<string> m_activatedObjectives;

        public event EventHandler Updated;
        public event EventHandler ObjectivesUpdated;

        private IStorageAgent m_fileAgent;

        public Func<ResourceActivationContext, VisualMarkerTask, IPlayerTaskDriver> CreateVisualMarkerTaskDriver;
        public Func<ResourceActivationContext, ARTask, IPlayerTaskDriver> CreateARTaskDriver;
        public Func<ResourceActivationContext, LocationTask, IPlayerTaskDriver> CreateLocationTaskDriver;
        public Func<ResourceActivationContext, CharacterTask, IPlayerTaskDriver> CreateCharacterTaskDriver;
        public Func<ResourceActivationContext, PlayerTask, IPlayerTaskDriver> CreatePlayerTaskDriver;
        public Func<ResourceActivationContext, Assignment, AssignmentDriver> CreateAssignmentDriver;

        public IEnumerable<IPlayerTaskDriver> AllTaskDrivers
        {
            get
            {
                return m_drivers.Values.ToArray();
            }
        }

        public IEnumerable<IPlayerTaskDriver> ActiveTaskDrivers
        {
            get
            {
                return m_drivers.Values.Where(t => !t.ActivationContext.IsClosed && !t.IsComplete).ToArray();
            }
        }

        public IEnumerable<AssignmentDriver> ActiveAssignments
        {
            get
            {
                return m_assignments.Values
                    .Where(d => !d.ActivationContext.IsClosed)
                    .OrderBy(d => d.ActivationContext.ActivationTime)
                    .ToArray();
            }
        }

        public AssignmentDriver FirstAssignment
        {
            get
            {
                return ActiveAssignments.FirstOrDefault();
            }
        }

        public IEnumerable<AssignmentObjective> VisibleIncompleteObjectives
        {
            get
            {
                return m_assignments.Values.Where(d => !d.IsComplete && !d.ActivationContext.IsClosed && d.Assignment.Objectives != null)
                    .SelectMany(d => d.Assignment.Objectives)
                    .Where(o => o.Objective != null && !IsObjectiveComplete(o.Objective.Id) && IsObjectiveActive(o));
            }
        }

        public TaskManager()
        {
            AutoAcceptTask = true;

            m_assignments = new Dictionary<string, AssignmentDriver>();
            m_drivers = new Dictionary<string, IPlayerTaskDriver>();
            m_outcomeDrivers = new SetDictionary<string, IPlayerTaskDriver>();
            ScriptManager.Instance.ScriptsReset += Instance_ScriptsReset;
            m_completedObjectives = new Dictionary<string, CompleteObjectiveState>();
            m_activatedObjectives = new HashSet<string>();

            m_fileAgent = StorageManager.GetGameStorageAgent("taskManager.json");

            var state = JsonHelper.Deserialize<TaskManagerState>(m_fileAgent);

            if (state != null)
            {
                if (state.CompletedObjectives != null)
                {
                    foreach (var o in state.CompletedObjectives)
                    {
                        m_completedObjectives[o.Objective.Id] = o;
                    }
                }

                if (state.ActivatedObjectives != null)
                {
                    foreach (var objId in state.ActivatedObjectives)
                    {
                        m_activatedObjectives.Add(objId);
                    }
                }
            }
        }

        void Instance_ScriptsReset(object sender, EventArgs e)
        {
            var drivers = m_drivers.Values.ToList();
            m_drivers.Clear();

            foreach (var driver in drivers)
            {
                driver.Stop();
            }

            m_completedObjectives.Clear();
            m_activatedObjectives.Clear();

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        public bool CanComplete(IPlayerTaskDriver driver)
        {
            if (driver != null && !driver.ActivationContext.IsClosed)
            {
                var task = driver.Task;

                if (driver.IsGiveTask)
                {
                    if (task.ActionItems != null)
                    {
                        if (task.ActionItems.CollectibleCounts != null)
                        {
                            bool isMet = true;

                            foreach (var cc in task.ActionItems.CollectibleCounts)
                            {
                                var count = Inventory.Instance.GetCount(cc.CollectibleId);

                                if (count < cc.Count)
                                {
                                    isMet = false;
                                    break;
                                }
                            }

                            if (!isMet)
                            {
                                return isMet;
                            }
                        }

                        if (task.ActionItems.CurrencyCounts != null)
                        {
                            bool isMet = true;

                            foreach (var cc in task.ActionItems.CurrencyCounts)
                            {
                                var count = Wallet.Instance.GetCount(cc.Currency);

                                if (count < cc.Count)
                                {
                                    isMet = false;
                                    break;
                                }
                            }

                            if (!isMet)
                            {
                                return isMet;
                            }
                        }
                    }
                }

                // Other actions can be completed?
                return true;
            }

            return false;
        }

        public virtual bool IsGiveTask(PlayerTask task)
        {
            return task.Action == TaskAction.Put ||
                task.Action == TaskAction.Exchange;
        }

        public virtual bool IsWaitTask(PlayerTask task)
        {
            return task.Action == TaskAction.Wait;
        }

        public virtual bool IsTakeTask(PlayerTask task)
        {
            return task.Action == TaskAction.Take;
        }

        public virtual bool IsExchangeTask(PlayerTask task)
        {
            return task.Action == TaskAction.Exchange;
        }

        public virtual bool IsConfirmTask(PlayerTask task)
        {
            return task.Action == TaskAction.Confirm;
        }

        void Save()
        {
            JsonHelper.Serialize(m_fileAgent, new TaskManagerState
            {
                CompletedObjectives = m_completedObjectives.Values.ToArray(),
                ActivatedObjectives = m_activatedObjectives.ToArray()
            });
        }

        public IEnumerable<IPlayerTaskDriver> GetActiveObjectiveTasks(AssignmentDriver adriver)
        {
            if (adriver.Assignment.Objectives != null)
            {
                return adriver.Assignment.Objectives
                    .Where(o => !IsObjectiveComplete(o.Objective))
                    .Select(o => GetTaskDriverForObjective(o.Objective))
                    .Where(d => d != null);
            }

            return new IPlayerTaskDriver[0];
        }

        public virtual void ActivateObjective(TaskObjective objective)
        {
            AnalyticsHelper.FireEvent("ActivateObjective", new Dictionary<string, object>()
                {
                    { "Id", objective.Id },
                    { "Title", objective.Title }
                });

            lock (m_activatedObjectives)
            {
                m_activatedObjectives.Add(objective.Id);
            }

            Save();

            if (ObjectivesUpdated != null)
            {
                ObjectivesUpdated(this, EventArgs.Empty);
            }

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        public virtual void CompleteObjective(TaskObjective objective, bool commit = true)
        {
            AnalyticsHelper.FireEvent("CompleteObjective", new Dictionary<string, object>()
                {
                    { "Id", objective.Id },
                    { "Title", objective.Title }
                });

            lock (m_completedObjectives)
            {
                CompleteObjectiveState os = null;

                if (!m_completedObjectives.TryGetValue(objective.Id, out os))
                {
                    os = new CompleteObjectiveState();

                    m_completedObjectives[objective.Id] = os;
                }

                os.Objective = objective;
                os.Count++;
            }

            if (commit)
            {
                Save();

                UpdateAssignments();

                if (ObjectivesUpdated != null)
                {
                    ObjectivesUpdated(this, EventArgs.Empty);
                }

                if (Updated != null)
                {
                    Updated(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Completes the task and optionally shows the reward screen for "take" items.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="showRewardScreenForTakeItems"></param>
        public virtual void Complete(IPlayerTaskDriver driver, bool showRewardScreenForTakeItems = true)
        {
            if (CanComplete(driver))
            {
                var args = new Dictionary<string, object>()
                {
                    { "Id", driver.Task.Id },
                    { "Title", driver.Task.Title }
                };

                if (driver is LocationTaskDriver)
                {
                    var location = ((LocationTaskDriver)driver).ActionLocation;

                    if (location != null)
                    {
                        args["LocationName"] = location.Name;
                        args["Coordinates"] = location.Coordinates.ToString();

                        if (location.LocationTypes != null)
                        {
                            args["LocationTypes"] = string.Join(",", location.LocationTypes);
                        }

                        if (location.StoryTags != null)
                        {
                            args["LocationStoryTags"] = string.Join(",", location.StoryTags);
                        }
                    }
                }

                AnalyticsHelper.FireEvent("CompleteTask", args);

                // Unless overridden, performing "action" completes this task
                driver.ActivationContext.FireEvent("complete");

                var task = driver.Task;

                Action doClose = () =>
                {
                    if (task.Outcomes != null)
                    {
                        foreach (var o in task.Outcomes)
                        {
                            CompleteObjective(o, false);
                        }

                        // Update any assignments..
                        Save();

                        UpdateAssignments();

                        if (ObjectivesUpdated != null)
                        {
                            ObjectivesUpdated(this, EventArgs.Empty);
                        }
                    }

                    if (!driver.Task.IsReplayable)
                    {
                        ClosePlayerTask(driver);
                    }

                    driver.DidComplete();

                    if (Updated != null)
                    {
                        Updated(this, EventArgs.Empty);
                    }
                };

                Action doReward = () =>
                {
                    if (task.Reward != null)
                    {
                        RewardManager.Instance.RewardValuables(task.Reward, doClose);
                    }
                    else
                    {
                        doClose();
                    }
                };

                if (driver.IsGiveTask)
                {
                    TransactionManager.Instance.RemoveValuables(task.ActionItems);

                    doReward();
                }
                else if (driver.IsTakeTask)
                {
                    if (task.ActionItems != null)
                    {
                        // By default we show a reward screen any time a user 
                        // collects something. Set to false to add items
                        // directly to the inventory.
                        if (showRewardScreenForTakeItems)
                        {
                            RewardManager.Instance.RewardValuables(task.ActionItems, doClose);
                        }
                        else
                        {
                            TransactionManager.Instance.AddValuables(task.ActionItems);
                        }
                    }
                    else
                    {
                        doReward();
                    }
                }
                else
                {
                    doReward();
                }
            }
        }

        private void UpdateAssignments()
        {
            foreach (var driver in m_assignments.Values)
            {
                driver.CheckComplete();
            }
        }

        public bool IsObjectiveActive(AssignmentObjective objective)
        {
            return !objective.IsHidden ||
                (objective.Objective != null && m_activatedObjectives.Contains(objective.Objective.Id));
        }

        public bool IsObjectiveComplete(string objectiveId)
        {
            return m_completedObjectives.ContainsKey(objectiveId);
        }

        public bool IsObjectiveComplete(TaskObjective objective)
        {
            if (objective != null)
            {
                return IsObjectiveComplete(objective.Id);
            }

            return false;
        }

        public IPlayerTaskDriver GetDriverByTaskId(string taskId)
        {
            return m_drivers.Values.Where(d => d.Task.Id == taskId).FirstOrDefault();
        }

        public IPlayerTaskDriver GetDriver(string instanceId)
        {
            if (m_drivers.ContainsKey(instanceId))
            {
                return m_drivers[instanceId];
            }

            return null;
        }

        public void CloseAssignment(AssignmentDriver driver)
        {
            driver.ActivationContext.Close();

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        public void CompleteAssignment(AssignmentDriver driver)
        {
            driver.ActivationContext.FireEvent("complete");

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        public void ClosePlayerTask(IPlayerTaskDriver driver)
        {
            if (driver != null)
            {
                driver.ActivationContext.Close();
            }

            StopTask(driver, update: true);
        }

        public virtual void ActivatePlayerTaskDriver(ResourceActivationContext context, IPlayerTaskDriver driver)
        {
            if (!context.IsClosed)
            {
                context.Open();

                if (driver.Task.Outcomes != null)
                {
                    foreach (var oc in driver.Task.Outcomes)
                    {
                        m_outcomeDrivers.Add(oc.Id, driver);
                    }
                }

                m_drivers[context.InstanceId] = driver;

                // todo auto accept every task based on config option
                if (TaskManager.Instance.AutoAcceptTask)
                {
                    driver.Accept(); // calls start
                }

                //else if (driver.IsAccepted)
                //{
                //    driver.Start();
                //}

                if (context.IsFirstActivation)
                {
                    if (TaskNotifier.Instance)
                    {
                        TaskNotifier.Instance.AddNewTask(driver);
                    }
                }
            }
            else
            {
                // Track closed drivers, but don't do anything else with them.
                // This enables task views that store a "completed" task state.
                if (driver.Task.Outcomes != null)
                {
                    foreach (var oc in driver.Task.Outcomes)
                    {
                        m_outcomeDrivers.Add(oc.Id, driver);
                    }
                }

                m_drivers[context.InstanceId] = driver;
            }

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        public virtual void UpdateTask(ResourceActivationContext context, PlayerTask task)
        {
            var driver = GetDriver(context.InstanceId);

            if (driver != null)
            {
                driver.Update(task);
            }
        }

#if MOTIVE_VUFORIA
    public void ActivateVisualMarkerTask(ResourceActivationContext context, VisualMarkerTask task)
    {
        IPlayerTaskDriver driver = null;

        if (CreateVisualMarkerTaskDriver != null)
        {
            driver = CreateVisualMarkerTaskDriver(context, task);
        }
        else
        {
            driver = new VisualMarkerTaskDriver(context, task);
        }

        ActivatePlayerTaskDriver(context, driver);
    }
#endif

        public virtual void ActivateARTask(ResourceActivationContext context, ARTask task)
        {
            IPlayerTaskDriver driver = null;

            if (CreateARTaskDriver != null)
            {
                driver = CreateARTaskDriver(context, task);
            }
            else
            {
                driver = new ARTaskDriver(context, task);
            }

            ActivatePlayerTaskDriver(context, driver);
        }

        public virtual void ActivateLocationTask(ResourceActivationContext context, LocationTask task)
        {
            IPlayerTaskDriver driver = null;

            if (CreateLocationTaskDriver != null)
            {
                driver = CreateLocationTaskDriver(context, task);
            }
            else
            {
                driver = new LocationTaskDriver(context, task);
            }

            ActivatePlayerTaskDriver(context, driver);
        }

        public void ActivateCharacterTask(ResourceActivationContext context, CharacterTask task)
        {
            IPlayerTaskDriver driver = null;

            if (CreateCharacterTaskDriver != null)
            {
                driver = CreateCharacterTaskDriver(context, task);
            }
            else
            {
                driver = new PlayerTaskDriver<CharacterTask, IMinigameDriver>(context, task);
            }

            ActivatePlayerTaskDriver(context, driver);
        }

        public void ActivatePlayerTask(ResourceActivationContext context, PlayerTask task)
        {
            IPlayerTaskDriver driver = null;

            if (CreatePlayerTaskDriver != null)
            {
                driver = CreatePlayerTaskDriver(context, task);
            }
            else
            {
                driver = new PlayerTaskDriver<PlayerTask, IMinigameDriver>(context, task);
            }

            ActivatePlayerTaskDriver(context, driver);
        }

        public void DeactivateTask(string instanceId)
        {
            if (m_drivers.ContainsKey(instanceId))
            {
                DeactivateTask(m_drivers[instanceId]);
            }
        }

        void StopTask(IPlayerTaskDriver driver, bool update = true)
        {
            driver.Stop();

            if (update)
            {
                if (Updated != null)
                {
                    Updated(this, EventArgs.Empty);
                }
            }
        }

        public void DeactivateTask(IPlayerTaskDriver driver)
        {
            if (m_drivers.ContainsKey(driver.ActivationContext.InstanceId))
            {
                m_drivers.Remove(driver.ActivationContext.InstanceId);

                StopTask(driver, update: false);

                if (driver.Task.Outcomes != null)
                {
                    foreach (var oc in driver.Task.Outcomes)
                    {
                        m_outcomeDrivers.Remove(oc.Id, driver);
                    }
                }
            }

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        public void ActivateAssignment(ResourceActivationContext context, Assignment assignment)
        {
            var driver = new AssignmentDriver(context, assignment);

            m_assignments.Add(context.InstanceId, driver);

            driver.CheckComplete();

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        public void DeactivateAssignment(ResourceActivationContext context, Assignment assignment)
        {
            m_assignments.Remove(context.InstanceId);

            if (Updated != null)
            {
                Updated(this, EventArgs.Empty);
            }
        }

        public IPlayerTaskDriver GetTaskDriverForObjective(TaskObjective objective)
        {
            if (objective != null)
            {
                return GetTaskDriverForObjective(objective.Id);
            }

            return null;
        }

        public IPlayerTaskDriver GetTaskDriverForObjective(string objectiveId)
        {
            var drivers = m_outcomeDrivers[objectiveId];

            if (drivers != null)
            {
                return drivers.FirstOrDefault();
            }

            return null;
        }

        internal bool CheckObjective(string objId)
        {
            lock (m_completedObjectives)
            {
                return m_completedObjectives.ContainsKey(objId);
            }
        }

        internal IEnumerable<AssignmentObjective> GetActiveObjectivesForAssignment(Assignment assignment)
        {
            if (assignment.Objectives == null)
            {
                return null;
            }

            return assignment.Objectives.Where(
                ao => ao.Objective != null &&
                    (!ao.IsHidden || m_activatedObjectives.Contains(ao.Objective.Id)));
        }

        internal virtual void StartMinigame(IPlayerTaskDriver playerTaskDriver)
        {
        }

        internal virtual void StopMinigame(IPlayerTaskDriver playerTaskDriver)
        {
        }

        public List<Collectible> GetPlayerRewardCollectibles(IPlayerTaskDriver itemDriver)
        {
            var list = new List<Collectible>();


            // TAKE TASK
            if (itemDriver.IsTakeTask &&
                ((itemDriver.Task.ActionItems != null && itemDriver.Task.ActionItems.CollectibleCounts != null) ||
                 (itemDriver.Task.Reward != null && itemDriver.Task.Reward.CollectibleCounts != null)))
            {
                if (itemDriver.Task.ActionItems != null &&
                    itemDriver.Task.ActionItems.CollectibleCounts != null &&
                    itemDriver.Task.ActionItems.CollectibleCounts.Any())
                {
                    list.AddRange(itemDriver.Task.ActionItems.CollectibleCounts.Select(cc => cc.Collectible));
                }
                if (itemDriver.Task.Reward != null &&
                    itemDriver.Task.Reward.CollectibleCounts != null &&
                    itemDriver.Task.Reward.CollectibleCounts.Any())
                {
                    list.AddRange(itemDriver.Task.Reward.CollectibleCounts.Select(cc => cc.Collectible));
                }

            }
            // GIVE TASK or EXCHANGE TASK
            else if ((itemDriver.IsGiveTask) && itemDriver.Task.Reward != null && itemDriver.Task.Reward.CollectibleCounts != null)
            {
                if (itemDriver.Task.Reward.CollectibleCounts != null &&
                    itemDriver.Task.Reward.CollectibleCounts.Any())
                {
                    list.AddRange(itemDriver.Task.Reward.CollectibleCounts.Select(cc => cc.Collectible));
                }
            }
            else
            {
                if (itemDriver.Task.Reward != null && itemDriver.Task.Reward.CollectibleCounts != null &&
                    itemDriver.Task.Reward.CollectibleCounts.Any())
                {
                    list.AddRange(itemDriver.Task.Reward.CollectibleCounts.Select(cc => cc.Collectible));
                }
            }

            return list;
        }
    }

}