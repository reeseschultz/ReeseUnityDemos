using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Nav
{
    ///<summary>The NavAgent which is used for path and jump planning and
    /// interpolation.</summary>
    struct NavAgent : IComponentData
    {
        /// <summary>Informs the nav systems that the agent has a destination.
        /// It is automatically set to true when the WorldDestination or
        /// LocalDestination is set. Outside the nav systems and debugging,
        /// this is only intended to be read, not written.</summary>
        public bool HasDestination;

        /// <summary>Informs the nav systems that the agent has jumped. Outside
        /// the nav systems and debugging, this only intended to be read, not
        /// written.</summary>
        public bool HasJumped;

        /// <summary>Informs the nav systems when path planning has been
        /// enqueued for the agent. Outside the nav systems and debugging, this
        /// is only intended to be read, not written.</summary>
        public bool HasQueuedPathPlanning;

        /// <summary>Informs the nav systems when the agent is falling, meaning
        /// no surface was detected after raycast attempts exceeding
        /// NavConstants.RAYCAST_MAX in the NavSurfaceSystem. 
        /// Outside the nav systems and debugging, this may be read and written
        /// as long as there is a reasonable fall handling system implemented.
        /// NavFallSystem is provided as an example, hence why it's namespaced
        /// in Reese.Demo and not Reese.Nav.</summary>
        public bool IsFalling;

        /// <summary>Informs the nav systems when the agent is jumping (and not
        /// yet determined to be falling, otherwise IsFalling would be true).
        /// Outside the nav systems and debugging, this is only intended to be
        /// read, not written.</summary>
        public bool IsJumping;

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
        /// NavInterpolationSystem to calculate projectile motion. Outside the
        /// nav systems and debugging, this is only intended to be read, not
        /// written.</summary>
        public float JumpSeconds;

        /// <summary>Writing to this is *required* when spawning an agent. It's
        /// artifical gravity used specifically for the NavInterpolationSystem
        /// to calculate projectile motion. 200 is a reasonable value to try.
        /// </summary>
        public float JumpGravity;

        /// <summary>Writing to this is *required* when spawning an agent (
        /// unless you want the agent to not move from have a default speed
        /// of zero). This is the translation speed of the agent. 20 is a
        /// reasonable value to try.</summary>
        public float TranslationSpeed;

        /// <summary>This is a *de facto* private variable storing the value of
        /// LocalDestination. This is *not* intended to be read nor written
        /// directly by *any* system.</summary>
        private float3 localDestination;

        /// <summary>Writing to this *or* the WorldDestination is *required*
        /// when spawning an agent. If you write to this, then you *must*
        /// set the DestinationSurface as well! After all, this is a
        /// destination that is relative to the provided DestinationSurface.
        /// This is how random positions are set as destinations in the
        /// MovingJumpDemo scene with the NavDestinationSystem.
        /// </summary>
        public float3 LocalDestination
        {
            get
            {
                return localDestination;
            }
            set
            {
                this.HasDestination = true;
                localDestination = value;
            }
        }

        /// <summary>This is a *de facto* private variable storing the value of
        /// WorldDestination. This is *not* intended to be read nor written
        /// directly by *any* system.</summary>
        private float3 worldDestination;

        /// <summary>Writing to this *or* the LocalDestination is *required*
        /// when spawning an agent. An example of usage is in the
        /// NavPointAndClick demo with the NavPointAndClickDestinationSystem.
        /// </summary>
        public float3 WorldDestination
        {
            get
            {
                return worldDestination;
            }
            set
            {
                this.HasDestination = true;
                worldDestination = value;
            }
        }

        /// <summary>You should *probably* write to this when spawning an
        /// agent. If you don't get this right, then raycasts below the agent
        /// may entirely overshoot the surface, which will eventually mean that
        /// IsFalling is set to true, which wouldn't be what you want.
        /// This is the offset of the agent from the surface. Now, you
        /// may find it odd that this is a float3 and not simply a float
        /// representing the y-component from the surface, but the idea here
        /// is to provide flexibility. While you may usually only set the y-
        /// component here, there could be situations where you want to set
        /// x or z. There are several examples of usage in the demo spawners.
        /// </summary>
        public float3 Offset;

        /// <summary>This is the index of the path buffer that the
        /// interpolation system is presently moving the agent toward, unless
        /// there is no path. Only the NavPlanSystem ever writes to this.
        /// Outside the nav systems and debugging, this is not intended to be
        /// read nor written.</summary>
        public int PathBufferIndex;

        /// <summary>Writing to this is *required* when spawning an agent. This
        /// is the type of agent, in terms of the NavMesh system. See
        /// examples of use in the demo spawners. There is also a helper method
        /// for setting the type in NavUtil called GetAgentType.</summary>
        public int TypeID;

        /// <summary>Writing to this is *required* if and only if you also
        /// write to the LocalDestination. This is intended to be an Entity
        /// with a NavSurface component. The set LocalDestination is thus
        /// locally relative to this surface, meaning the matrix math to
        /// figure that out is done for you. This is how random positions are
        /// set as destinations in the MovingJumpDemo scene with the
        /// NavDestinationSystem.</summary>
        public Entity DestinationSurface;

        /// <summary>This is the currently detected surface underneath the
        /// agent. The surface is detected with the NavSurfaceSystem. Outside
        /// the nav systems and debugging, this is only intended to be read,
        /// not written. An example of such a read is in the
        /// NavDestinationSystem example. **Using this surface is how you
        /// determine which other surfaces are "jumpable" for a given
        /// NavAgent, by querying *this* surface's jumpable buffer.**</summary>
        public Entity Surface;
    }
}
