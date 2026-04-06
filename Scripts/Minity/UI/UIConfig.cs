using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minity.UI
{
    [CreateAssetMenu(fileName = "UIConfig", menuName = "Minity/UI Config")]
    public class UIConfig : ScriptableObject
    {
        [Serializable]
        public class Config
        {
            public string Uri;
            public UIMode Mode;
        }
        public List<Config> List = new List<Config>();
    }
}
