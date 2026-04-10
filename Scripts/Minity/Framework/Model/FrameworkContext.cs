using System.Collections.Generic;
using Minity.Framework.Component;

namespace Minity.Framework.Model
{
    public class FrameworkContext
    {
        public readonly List<IFrameworkComponent> Children = new List<IFrameworkComponent>();
        public IFrameworkComponent Parent;
    }
}
