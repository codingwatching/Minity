using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Minity.Framework.Model;
using Minity.General;
using UnityEngine;

namespace Minity.Framework.Component
{
    public abstract class FrameworkComponent : IFrameworkComponent
    {
        internal readonly FrameworkContext Context = new FrameworkContext();
        
        private Dictionary<EnumIdentifier, MessageHandler> _commandHandler = new Dictionary<EnumIdentifier, MessageHandler>();
        private Dictionary<EnumIdentifier, MessageHandler> _updateHandler = new Dictionary<EnumIdentifier, MessageHandler>();
        
        protected void HandleCommands(params (EnumIdentifier, MessageHandler)[] commands)
        {
            _commandHandler = commands.ToDictionary(x => x.Item1, x => x.Item2);
        }
        
        protected void HandleUpdates(params (EnumIdentifier, MessageHandler)[] updates)
        {
            _updateHandler = updates.ToDictionary(x => x.Item1, x => x.Item2);
        }

        void IFrameworkComponent.SendCommandInternal(EnumIdentifier command, IMessageContext ctx, bool firstDepth)
        {
            if (Context.Parent == null)
            {
                return;
            }

            if (!firstDepth)
            {
                if (_commandHandler.TryGetValue(command, out var handler) && handler.Invoke(ctx))
                {
                    return;
                }
            }
            
            Context.Parent.SendCommandInternal(command, ctx, false);
        }

        public void SendCommand<T>(T command, IMessageContext ctx) where T : Enum
        {
            var wrapCommand = EnumIdentifier.Wrap(command);
            ((IFrameworkComponent)this).SendCommandInternal(wrapCommand, ctx, true);
        }
        
        bool IFrameworkComponent.SendUpdateInternal(EnumIdentifier update, IMessageContext ctx, bool firstDepth)
        {
            if (!firstDepth)
            {
                if (_updateHandler.TryGetValue(update, out var handler) && handler.Invoke(ctx))
                {
                    return true;
                }
            }
            
            for (var i = 0; i < Context.Children.Count; i++)
            {
                if (Context.Children[i].SendUpdateInternal(update, ctx, false))
                {
                    return true;
                }
            }

            return false;
        }

        async Task IFrameworkComponent.InitializeInternal(CancellationToken ct)
        {
            await Initialize(ct);
        }

        void IFrameworkComponent.ShutdownInternal(bool firstDepth)
        {
            for (var i = 0; i < Context.Children.Count; i++)
            {
                Context.Children[i].ShutdownInternal(false);
            }
            Shutdown();
            
            // Avoid duplicated & useless children removing
            if (firstDepth && Context.Parent != null)
            {
                Context.Parent.RemoveChildInternal(this);
            }
        }

        void IFrameworkComponent.RemoveChildInternal(IFrameworkComponent child)
        {
            Context.Children.Remove(child);
        }

        public void SendUpdate<T>(T update, IMessageContext ctx) where T : Enum
        {
            var wrapUpdate = EnumIdentifier.Wrap(update);
            ((IFrameworkComponent)this).SendUpdateInternal(wrapUpdate, ctx, true);
        }

        public void Unload()
        {
            ((IFrameworkComponent)this).ShutdownInternal(true);
        }
        
        public abstract Task Initialize(CancellationToken ct);
        public abstract void Shutdown();
    }
}
