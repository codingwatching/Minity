using System;
using System.Collections.Generic;
using System.Linq;
using Minity.Logger;
using Minity.General;
using Minity.ResourceManager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Minity.UI
{
    public enum UIMode
    {
        Default, Singleton
    }

    public enum BuiltinUI
    {
        AnonymousUI
    }
    
    public class UI
    {
        internal EnumIdentifier Identifier;
        internal Type ParameterType = null;
        internal Type ReturnValueType = null;
        internal Type TypeDefinition;
        internal string PrefabUri;
        internal UIMode Mode = UIMode.Default;

        internal GameObject Instance;
        
        internal UI()
        {
            
        }
        
        internal GameObject Create()
        {
            var prefab = ResManager.Instance.Load<GameObject>(PrefabUri, ResUsage.Temp());
            if (Mode == UIMode.Singleton)
            {
                if (!Instance)
                {
                    Instance = Object.Instantiate(prefab);
                }
                Object.DontDestroyOnLoad(Instance);
                return Instance;
            }

            return Object.Instantiate(prefab);
        }

        [Obsolete("Use FromUIConfig() instead")]
        public static IEnumerable<UI> FromUIList(string uiListPath)
        {
            throw new NotImplementedException("Use FromUIConfig() instead");
        }
        
        public static IEnumerable<UI> FromUIConfig(string uiConfigUri)
        {
            var list = ResManager.Instance.Load<UIConfig>(uiConfigUri, ResUsage.Temp());
            if (!list)
            {
                throw new Exception($"UIConfig '{uiConfigUri}' not found.");
            }
            return list.List.Select(x =>
            {
                var ui = FromPrefab(BuiltinUI.AnonymousUI, x.Uri);
                ui.Mode = x.Mode;
                return ui;
            });
        }
        
        public static UI FromResources(string prefabPath)
            => FromPrefab(BuiltinUI.AnonymousUI, "resources:///" + prefabPath);
        
        [Obsolete("Use prefab uri instead")]
        public static UI FromPrefab<T>(GameObject prefab)
            => throw new NotImplementedException("Use prefab uri instead");
        
        public static UI FromResources<T>(T identifier, string prefabPath) where T : Enum
            => FromPrefab(identifier, "resources:///" + prefabPath);
        
        public static UI FromPrefab<T>(T identifier, string prefabUri) where T : Enum
        {
            var prefab = ResManager.Instance.Load<GameObject>(prefabUri, ResUsage.Temp());
            if (!prefab)
            {
                throw new Exception($"UI prefab '{prefabUri}' not found.");
            }
            
            if (!prefab.TryGetComponent<ManagedUI>(out var ui))
            {
                throw new Exception($"UI '{identifier}'({prefab.name}) must have a ManagedUI component.");
            }
            
            var type = ui.GetType();
            while (type != null && type.BaseType != null)
            {
                if (type.IsGenericType && type.BaseType == typeof(ManagedUI))
                {
                    break;
                }
                type = type.BaseType;
            }
            
            if (identifier is BuiltinUI id && id == BuiltinUI.AnonymousUI && type == typeof(SimpleManagedUI))
            {
                throw new Exception($"SimpleManagedUI '{identifier}'({prefab.name}) must have an identifier.");
            }
            
            var data = new UI()
            {
                Identifier = EnumIdentifier.Wrap(identifier),
                PrefabUri = prefabUri,
                TypeDefinition = type
            };

            var genericType = type.GetGenericTypeDefinition();
            var args = type.GetGenericArguments();
            
            if (genericType == typeof(ManagedUI<,>))
            {
                data.ParameterType = args[1];
            }
            else if (genericType == typeof(ManagedUI<,,>))
            {
                data.ParameterType = args[1];
                data.ReturnValueType = args[2];
            }
            else if (genericType == typeof(ManagedUIReturnValueOnly<,>))
            {
                data.ReturnValueType = args[1];
            }
            
            return data;
        }

        public UI SingletonMode()
        {
            Mode = UIMode.Singleton;
            return this;
        }
    }
}
