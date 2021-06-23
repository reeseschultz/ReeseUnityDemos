using Unity.Entities;

namespace Reese.Nav
{
    /// </summary>System serving as the single source of truth of navigation settings.</summary>
    public class NavSystem : SystemBase
    {
        /// <summary>Settings that can be updated at runtime.</summary>
        public NavSettings Settings = new NavSettings
        {
            DestinationRateLimitSeconds = 0.8f,
            DestinationSurfaceColliderRadius = 1,
            JumpSecondsMax = 5,
            ObstacleRaycastDistanceMax = 1000,
            SurfaceRaycastDistanceMax = 1000,
            StoppingDistance = 1,
            SurfaceLayer = 28,
            ObstacleLayer = 29,
            ColliderLayer = 30,
            IterationMax = 1000,
            NavMeshQueryNodeMax = 5000,
            NeedsSurfaceMapSize = 1000,
            PathSearchMax = 1000,
            SurfaceRaycastMax = 100
        };
        
        public NavFlockingSettings FlockingSettings = new NavFlockingSettings
        {
            SeparationWeight = 2.0f,
            AlignmentWeight = 1.5f,
            CohesionWeight = 1f,
            FollowWeight = 2.5f,
            AgentCollisionAvoidanceStrength = 0.5f,
            ObstacleCollisionAvoidanceStrength = 0.5f,
            CollisionCastingAngle = 65f,
            QuadrantCellSize = 5,
            QuadrantZMultiplier = 1000
        }; 

        /// <summary>Includes settings used by the navigation systems.</summary>
        public struct NavSettings
        {
            /// <summary>Duration in seconds before a new destination will take effect after another. Prevents planning from being clogged with destinations which can then block interpolation of agents.</summary>
            public float DestinationRateLimitSeconds;

            /// <summary>A sphere collider of the specified radius is used to detect the destination surface.</summary>
            public float DestinationSurfaceColliderRadius;

            /// <summary>Upper limit on the *duration* spent jumping before the agent is actually considered falling. This limit can be reached when the agent tries to jump too close to the edge of a surface and misses.</summary>
            public float JumpSecondsMax;

            /// <summary>Upper limit on the raycast distance when searching for an obstacle in front of a given NavAgent.</summary>
            public float ObstacleRaycastDistanceMax;

            /// <summary>Upper limit on the raycast distance when searching for a surface below a given NavAgent.</summary>
            public float SurfaceRaycastDistanceMax;

            /// <summary>Stopping distance of an agent from its destination.</summary>
            public float StoppingDistance;

            /// <summary>The layer for surfaces.</summary>
            public int SurfaceLayer;

            /// <summary>The layer for obstacles.</summary>
            public int ObstacleLayer;

            /// <summary>The layer for colliders.</summary>
            public int ColliderLayer;

            /// <summary>Upper limit on the iterations performed in a NavMeshQuery to find a path in the NavPlanSystem.</summary>
            public int IterationMax;

            /// <summary>Upper limit on the path node pool size for each NavMeshQuery created in the NavMeshQuerySystem. May need to be increased if an OutOfNodes error arises while finding a path.</summary>
            public int NavMeshQueryNodeMax;

            /// <summary>The initial capacity of the map tracking agents that need a surface.</summary>
            public int NeedsSurfaceMapSize;

            /// <summary>Upper limit on the search area size during path planning.</summary>
            public int PathSearchMax;

            /// <summary>Upper limit on the number of raycasts to attempt in searching for a surface below the NavAgent. Exceeding this implies that there is no surface below the agent, its then determined to be falling which means that no more raycasts will be performed.</summary>
            public int SurfaceRaycastMax;
        }

        /// <summary>Includes settings used by the navigation/ flocking systems.</summary>
        public struct NavFlockingSettings
        {
            /// <summary>
            /// Works like collision, so that units do not share the same space
            /// </summary>
            public float SeparationWeight;
            /// <summary>
            /// How much should agents align with other agents in a flock
            /// </summary>
            public float AlignmentWeight;
            /// <summary>
            /// How strongly should agents look to join other agents (grouping behavior)
            /// </summary>
            public float CohesionWeight;
            /// <summary>
            /// To be included, maybe rewrite the NavFollow system to add weighted steering to the NavSteeringSystem?
            /// </summary>
            public float FollowWeight; 
            /// <summary>
            /// Weight of collision avoidance so agents steer away from each other
            /// </summary>
            public float AgentCollisionAvoidanceStrength;
            /// <summary>
            /// Weight of collision avoidance so agents steer away from obstacles
            /// </summary>
            public float ObstacleCollisionAvoidanceStrength;
            /// <summary>
            /// The (half) angle of the collision raycasting of the entity. The direction is the entities forward vector.
            /// If you plug in 180 here, the raycasting will surround the player. (360 deg)
            /// </summary>
            public float CollisionCastingAngle;
            
            public int QuadrantCellSize; 
            public int QuadrantZMultiplier; 
        }

        protected override void OnUpdate() { }
    }
}
