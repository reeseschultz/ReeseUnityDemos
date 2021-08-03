using System;
using Unity.Entities;

namespace Reese.Demo
{
    [Serializable]
    public struct PathFlocking : IComponentData
    {
        /// <summary>
        /// The perception radius for the cohesion behavior. Differs from agent to agent because of body size. You should *probably* write
        /// to this when spawning an agent and you want to use the flocking system.
        /// Setting this to 1 or 2 is a good starting point.
        /// </summary>
        public float CohesionPerceptionRadius;

        /// <summary>
        /// The perception radius for the alignment behavior. Differs from agent to agent because of body size. You should *probably* write
        /// to this when spawning an agent and you want to use the flocking system.
        /// Setting this to 1 or 2 is a good starting point.
        /// </summary>
        public float AlignmentPerceptionRadius;

        /// <summary>
        /// The perception radius for the separation behavior. Keep it relatively low, it is the distance from where agents start to push each other away.
        /// Differs from agent to agent because of body size. You should *probably* write
        /// to this when spawning an agent and you want to use the flocking system.
        /// </summary>
        public float SeparationPerceptionRadius;

        /// <summary>
        /// The distance from which agents start to steer away from each other when using the flocking system. Differs from agent to agent because of body size.
        /// You should *probably* write to this when spawning an agent and you want to use the flocking system.
        /// </summary>
        public float AgentAversionDistance;

        /// <summary>
        /// The distance from which agents start to steer away from obstacles when using the flocking system. Differs from agent to agent because of body size.
        /// You should *probably* write to this when spawning an agent and you want to use the flocking system.
        /// </summary>
        public float ObstacleAversionDistance;
    }
}
