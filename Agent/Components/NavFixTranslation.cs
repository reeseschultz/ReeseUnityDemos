using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the agent's translation needs to be fixed.</summary>
    [Serializable]
    public struct NavFixTranslation : IComponentData { }
}
