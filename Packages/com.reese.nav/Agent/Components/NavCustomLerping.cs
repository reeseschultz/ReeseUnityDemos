using System;
using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>Exists if the user needs to handle lerping.</summary>
    [Serializable]
    public struct NavCustomLerping : IComponentData { }
}
