using System;
using System.Threading;
using System.Threading.Tasks;

namespace Minity.Framework.Component
{
    public abstract class System : FrameworkComponent
    {
        public async Task<T> CreateModule<T>(CancellationToken ct) where T : Module
        {
            var module = (T)await this.CreateFrameworkComponent<T>(ct);
            Context.Children.Add(module);
            return module;
        }
    }
}
