using System;
using Minity.Variable;
using UnityEngine;

namespace Minity.UI
{
    [Serializable]
    public class VarBinding<T> : BindingBase, ISerializationCallbackReceiver 
    {
        [SerializeField]
        private MinityVariable<T> Variable;
        
        public T Value
        {
            set 
            {
                if (!Variable)
                {
                    throw new Exception("Variable not set");
                }
                Variable.SetValue(value);
            }
            get
            {
                if (!Variable)
                {
                    throw new Exception("Variable not set");
                }
                return (T)Variable.GetValue();
            }
        }
        
        internal override object GetValue()
        {
            if (!Variable)
            {
                throw new Exception("Variable not set");
            }
            return Variable.GetValue();
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            if (!Variable)
            {
                return;
            }
            Variable.InternalChangedEvent += RaiseUpValueChangedEvent;
        }
    }
}
