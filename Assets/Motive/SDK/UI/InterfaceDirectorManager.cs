using Motive.Core.Scripting;
using Motive.Core.Utilities;
using Motive.UI.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.UI
{
    public interface IInterfaceCommandHandler
    {
        void ProcessInterfaceCommand(ResourceActivationContext context, IScriptObject command, Action onComplete);
    }

    public abstract class InterfaceCommandHandler<T> : MonoBehaviour, IInterfaceCommandHandler
        where T : IScriptObject
    {
        public virtual void ProcessInterfaceCommand(ResourceActivationContext context, IScriptObject command, Action onComplete)
        {
            if (command is T)
            {
                this.ProcessInterfaceCommand(context, (T)command, onComplete);
            }
            else
            {
                if (onComplete != null)
                {
                    onComplete();
                }
            }
        }

        protected void RegisterType(string type)
        {
            InterfaceDirectorManager.Instance.RegisterHandler(type, this);
        }

        public abstract void ProcessInterfaceCommand(ResourceActivationContext context, T command, Action onComplete);
    }

    public class InterfaceDirectorManager : Singleton<InterfaceDirectorManager>
    {
        Dictionary<string, IInterfaceCommandHandler> m_handlers;

        public InterfaceDirectorManager()
        {
            m_handlers = new Dictionary<string, IInterfaceCommandHandler>();
        }

        public void RegisterHandler(string type, IInterfaceCommandHandler handler)
        {
            m_handlers[type] = handler;
        }
        
        public void ProcessInterfaceDirector(ResourceActivationContext context, InterfaceDirector director)
        {
            if (context.IsClosed)
            {
                return;
            }

            context.Open();

            if (director.Commands == null ||
                director.Commands.Length == 0)
            {
                context.Close();

                return;
            }

            BatchProcessor iter = new BatchProcessor(director.Commands.Length, context.Close);

            foreach (var command in director.Commands)
            {
                IInterfaceCommandHandler handler = null;

                if (m_handlers.TryGetValue(command.Type, out handler))
                {
                    handler.ProcessInterfaceCommand(context, command, () => iter++);
                }
                else
                {
                    throw new NotSupportedException("Unsupported interface command type " + command.Type);
                }
            }
        }
    }
}
