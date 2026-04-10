using System;
using System.Threading;
using System.Threading.Tasks;
using Minity.Framework.Component;

namespace Minity.Framework
{
    public static class MilFramework
    {
        internal static async Task<FrameworkComponent> CreateFrameworkComponent<T>(this FrameworkComponent parent, CancellationToken ct) where T : FrameworkComponent
        {
            var component = Activator.CreateInstance<T>();
            component.Context.Parent = parent;
            IFrameworkComponent internalComponent = component;
            await internalComponent.InitializeInternal(ct);
            return component;
        }

        public static async Task<T> CreateSystem<T>(CancellationToken ct) where T : Component.System
        {
            return (T)await CreateFrameworkComponent<T>(null, ct);
        }
    }
}
