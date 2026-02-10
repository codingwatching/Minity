using System;
using Object = UnityEngine.Object;

namespace Minity.ResourceManager.UsageDetector
{
    public struct ObjectLinkUD : IUsageDetector
    {
        private Object _refer;
        
        public Object GetLinkObject() => _refer;
        
        public void Initialize(object? bind)
        {
            if (bind is not Object obj)
            {
                throw new Exception("Must bind a object");
            }
            _refer = obj;
        }

        public bool IsUsing()
        {
            if (_refer)
            {
                return true;
            }
            _refer = null;
            return false;
        }
        
        public IUsageDetector CombineDetector(IUsageDetector detector)
        {
            var compose = new ComposeUD();
            compose.CombineDetector(this);
            return compose;
        }
    }
}
