using System;
using System.Threading;
using System.Threading.Tasks;
using Minity.Framework.Model;
using Minity.General;

namespace Minity.Framework.Component
{
    public interface IFrameworkComponent
    {
        internal void SendCommandInternal(EnumIdentifier command, IMessageContext ctx, bool firstDepth);
        internal bool SendUpdateInternal(EnumIdentifier update, IMessageContext ctx, bool firstDepth);

        internal Task InitializeInternal(CancellationToken ct);
        internal void ShutdownInternal(bool firstDepth);
        
        internal void RemoveChildInternal(IFrameworkComponent child);
    }
}
