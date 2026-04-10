using System;
using System.Threading;
using System.Threading.Tasks;
using Minity.Framework.Model;

namespace Minity.Framework.Component
{
    public abstract class Module : FrameworkComponent
    {
        public async Task<T> CreateController<T>(CancellationToken ct) where T : Controller
        {
            var module = (T)await this.CreateFrameworkComponent<T>(ct);
            Context.Children.Add(module);
            return module;
        }
    }
}
