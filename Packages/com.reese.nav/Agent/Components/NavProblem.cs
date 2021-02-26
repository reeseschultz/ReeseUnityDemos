using System;
using Unity.Entities;
using UnityEngine.Experimental.AI;

namespace Reese.Nav
{
    /// <summary>Exists if the agent has a problem.</summary>
    [Serializable]
    public struct NavProblem : IComponentData
    {
        /// <summary>The problematic status preventing further path planning. See: https://docs.unity3d.com/ScriptReference/Experimental.AI.PathQueryStatus.html</summary>
        public PathQueryStatus Value;
    }
}
