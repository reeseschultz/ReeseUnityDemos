using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Nav
{
    ///<summary>The NavAgent which is used for path and jump planning and
    /// interpolation.</summary>
    [Serializable]
    public struct NavAgent : IComponentData
    {
        /// <summary>This is the *point* in time when the agent's last
        /// destination was set. Outside the nav systems and debugging,
        /// this is only intended to be read, not written.</summary>
        public float DestinationSeconds;

        /// <summary>This is the *point* in time when the agent started
        /// falling, *not* the duration. This is written to by the nav systems
        /// to help you figure out how long the agent has been falling. See the
        /// NavFallSystem as an example. Outside the nav systems and debugging,
        /// this is only intended to be read, not written.</summary>
        public float FallSeconds;

        /// <summary>Writing to this is *required* when spawning an agent. It's
        /// the jump angle in degrees. 45 is a reasonable value to try.
        /// </summary>
        public float JumpDegrees;

        /// <summary>This is the *point* in time when the agent started jumping,
        /// *not* the duration. This is written to specifically by the
        /// NavLerpSystem to calculate projectile motion. Outside the
        /// nav systems and debugging, this is only intended to be read, not
        /// written.</summary>
        public float JumpSeconds;

        /// <summary>Jump speed of the agent along the horizontal axis.</summary>
        public float JumpSpeedMultiplierX;

        /// <summary>Jump speed of the agent along the vertical axis.</summary>
        public float JumpSpeedMultiplierY;

        /// <summary>Writing to this is *required* when spawning an agent. It's
        /// artifical gravity used specifically for the NavLerpSystem
        /// to calculate projectile motion. 200 is a reasonable value to try.
        /// </summary>
        public float JumpGravity;

        /// <summary>Normal of point below the agent.</summary>
        public float3 SurfacePointNormal;

        /// <summary>Writing to this is *required* when spawning an agent (
        /// unless you want the agent to not move from have a default speed
        /// of zero). This is the translation speed of the agent. 20 is a
        /// reasonable value to try.</summary>
        public float TranslationSpeed;

        /// <summary>Writing to this is *required* when spawning an agent (
        /// unless you want the agent to not rotate towards the target 
        /// direction). This is the rotation speed of the agent. 0.3f is a
        /// reasonable value to try.</summary>
        public float RotationSpeed;

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
        /// The distance from which agents start to steer away from each other. Differs from agent to agent because of body size.
        /// You should *probably* write to this when spawning an agent and you want to use the flocking system.
        /// </summary>
        public float ObstacleAversionDistance;

        /// <summary>You should *probably* write to this when spawning an
        /// agent. If you don't get this right, then raycasts below the agent
        /// may entirely overshoot the surface, which will eventually mean that
        /// the NavFalling component is added, which wouldn't be what you want.
        /// This is the offset of the agent from the surface. Now, you
        /// may find it odd that this is a float3 and not simply a float
        /// representing the y-component from the surface, but the idea here
        /// is to provide flexibility. While you may usually only set the y-
        /// component here, there could be situations where you want to set
        /// x or z. There are several examples of usage in the demo spawners.
        /// </summary>
        public float3 Offset;

        /// <summary>For knowing how many times raycasting has been conducted
        /// from the negative y-component of the agent (while jumping) to detect
        /// a surface below in the NavSurfaceSystem. If no surface is detected
        /// and NavConstants.SURFACE_RAYCAST_MAX is exceeded for a given NavAgent,
        /// then the falling component is added. Raycasting then stops. An example
        /// of how to handle falling is in NavFallSystem, but feel free to use
        /// whatever implementation you want, hence why NavFallSystem is
        /// namespaced in Reese.Demo and not Reese.Nav. Outside the nav systems
        /// and debugging, this is only intended to be read, not written. 
        /// </summary>
        public int SurfaceRaycastCount;

        /// <summary>Writing to this is *required* when spawning an agent. This
        /// is the type of agent, in terms of the NavMesh system. See
        /// examples of use in the demo spawners. There is also a helper method
        /// for setting the type in NavUtil called GetAgentType.</summary>
        public int TypeID;

        /// <summary>This is the destination entity's surface.</summary>
        public Entity DestinationSurface;

        /// <summary>This is the local destination that the agent moves toward.
        /// Outside the nav systems and debugging, this is not intended to be
        /// read nor written.</summary>
        public float3 LocalDestination;
    }
}
