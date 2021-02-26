using System;
using Unity.Entities;

namespace Reese.Utility
{
    [Serializable]
    public struct Root : IComponentData
    {
        public Entity Value;
    }
}
