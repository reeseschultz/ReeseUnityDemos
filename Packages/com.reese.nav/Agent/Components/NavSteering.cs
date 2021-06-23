using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Nav
{
    [Serializable]
    public struct NavSteering : IComponentData
    {
        public float3 CollisionAvoidanceSteering;
        public float3 AgentAvoidanceSteering;
        public float3 SeparationSteering;
        public float3 CohesionSteering;
        public float3 AlignmentSteering;
        public float3 CurrentHeading;
    }
}
