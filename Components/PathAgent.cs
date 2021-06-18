using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Path
{
    ///<summary>The agent for which path planning is convened.</summary>
    [Serializable]
    public struct PathAgent : IComponentData
    {
        /// <summary>This is the *point* in time when the agent's last
        /// destination was set. Outside the nav systems and debugging,
        /// this is only intended to be read, not written.</summary>
        public float DestinationSeconds;

        /// <summary>
        /// This is the offset of the agent from the surface. Now, you
        /// may find it odd that this is a float3 and not simply a float
        /// representing the y-component from the surface, but the idea here
        /// is to provide flexibility. While you may usually only set the y-
        /// component here, there could be situations where you want to set
        /// x or z.
        /// </summary>
        public float3 Offset;

        /// <summary>Writing to this is *required* when spawning an agent. This
        /// is the type of agent, in terms of the NavMesh system. There is also
        /// a helper method for setting the type in PathUtil called
        /// GetAgentType.</summary>
        public int TypeID;

        /// <summary>This is the agent's world destination.
        /// Outside the nav systems and debugging, this is not intended to be
        /// read nor written.</summary>
        public float3 WorldDestination;
    }
}
