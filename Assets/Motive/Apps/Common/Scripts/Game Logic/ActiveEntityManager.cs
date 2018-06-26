// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Utilities;
using Motive.Gaming.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Motive.Unity.Gaming
{
    public class ActiveEntityManager : Singleton<ActiveEntityManager>
    {
        public bool AreRequirementsMet(EntityActionActivationOption option)
        {
            if (option == null || option.Requirements == null)
            {
                return true;
            }

            return TransactionManager.Instance.HasValuables(option.Requirements);
        }

        public bool IsCostMet(EntityActionActivationOption option)
        {
            if (option == null || option.Cost == null)
            {
                return true;
            }

            return TransactionManager.Instance.HasValuables(option.Cost);
        }

        public bool CanTakeAction(IActiveEntity entity, string action, out IEnumerable<EntityActionActivationOption> options)
        {
            options = null;

            return false;
        }

        public bool CanExerciseOption(EntityActionActivationOption actorOption, EntityActionActivationOption acteeOption)
        {
            if (AreRequirementsMet(actorOption) && AreRequirementsMet(acteeOption) &&
                IsCostMet(actorOption) && IsCostMet(acteeOption))
            {
                if (acteeOption.Attributes != null)
                {
                    if (actorOption.Attributes != null)
                    {
                        foreach (var ao in acteeOption.Attributes)
                        {
                            foreach (var bo in actorOption.Attributes)
                            {
                                if (ao.Attribute == bo.Attribute)
                                {
                                    // Actee attribute value > actor attribute value,
                                    // not met
                                    if (ao.Value > bo.Value)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }

                        // Didn't find any attribute options that *aren't* met,
                        // so player can exercise this option.
                        return true;
                    }
                }
                else
                {
                    // No attributes, so any option matches
                    return true;
                }
            }

            return false;
        }

        public bool EmitsAction(IActiveEntity actor, string action)
        {
            return actor.EmittedActions != null && actor.EmittedActions.Any(a => a.Action == action);
        }

        public EntityActionBehaviour GetEmittedAction(IActiveEntity actor, string action)
        {
            if (actor.EmittedActions != null)
            {
                return actor.EmittedActions.Where(a => a.Action == action).FirstOrDefault();
            }

            return null;
        }

        public bool ReceivesAction(IActiveEntity actor, string action)
        {
            return actor.ReceivedActions != null && actor.ReceivedActions.Any(a => a.Action == action);
        }

        public bool CanActOn(IActiveEntity actor, IActiveEntity actee, string action, out IEnumerable<EntityActionActivationOption> actorOptions, out IEnumerable<EntityActionActivationOption> acteeOptions)
        {
            actorOptions = null;
            acteeOptions = null;

            // We only consider the first behaviour for the given action.
            var actorBehaviours = actor.EmittedActions == null ? null : (actor.EmittedActions.Where(a => a.Action == action).FirstOrDefault());
            var acteeBehaviours = actee.ReceivedActions == null ? null : (actee.ReceivedActions.Where(a => a.Action == action).FirstOrDefault());

            if (acteeBehaviours != null && actorBehaviours != null)
            {
                if (acteeBehaviours.ActivationOptions == null ||
                    acteeBehaviours.ActivationOptions.Count() == 0)
                {
                    // Actee has no defined options, so any actor behaviours will work
                    actorOptions = actorBehaviours.ActivationOptions;
                }
                else if (actorBehaviours.ActivationOptions != null)
                {
                    var actorOpts = new List<EntityActionActivationOption>();
                    var acteeOpts = new List<EntityActionActivationOption>();

                    actorOptions = actorOpts;
                    acteeOptions = acteeOpts;

                    foreach (var a in acteeBehaviours.ActivationOptions)
                    {
                        foreach (var b in actorBehaviours.ActivationOptions)
                        {
                            if (CanExerciseOption(a, b))
                            {
                                actorOpts.Add(a);
                                acteeOpts.Add(b);
                            }
                        }
                    }

                    return (actorOpts.Count > 0 && acteeOpts.Count > 0);
                }
            }

            return false;
        }

        public bool CanActOn(IActiveEntity actor, IActiveEntity actee, string action)
        {
            IEnumerable<EntityActionActivationOption> actorOptions = null;
            IEnumerable<EntityActionActivationOption> acteeOptions;

            return CanActOn(actor, actee, action, out actorOptions, out acteeOptions);
        }

        public IEnumerable<T> FilterActors<T>(IEnumerable<T> actors, IActiveEntity actee, string action)
            where T : IActiveEntity
        {
            return null;
        }

        public bool TakeAction(IActiveEntity entity)
        {
            return false;
        }

        public bool TakeAction(IActiveEntity actor, IActiveEntity actee, string action)
        {
            return false;
        }
    }

}