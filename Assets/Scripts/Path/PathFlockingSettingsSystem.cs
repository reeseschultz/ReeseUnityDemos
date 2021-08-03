using Unity.Entities;

namespace Reese.Demo
{
    public class PathFlockingSettingsSystem : SystemBase
    {
        public PathFlockingSettings FlockingSettings = new PathFlockingSettings
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

        /// <summary>Includes settings used by the navigation/ flocking systems.</summary>
        public struct PathFlockingSettings
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
