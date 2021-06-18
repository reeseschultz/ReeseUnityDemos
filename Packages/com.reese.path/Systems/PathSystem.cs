using Unity.Entities;

namespace Reese.Path
{
    /// </summary>System serving as the single source of truth of navigation settings.</summary>
    public class PathSystem : SystemBase
    {
        /// <summary>Settings that can be updated at runtime.</summary>
        public PathSettings Settings = new PathSettings
        {
            DestinationRateLimitSeconds = 0.8f,
            IterationMax = 1000,
            PathMeshQueryNodeMax = 5000,
            PathSearchMax = 1000
        };

        /// <summary>Includes settings used by the navigation systems.</summary>
        public struct PathSettings
        {
            /// <summary>Duration in seconds before a new destination will take effect after another. Prevents planning from being clogged with destinations which can then block interpolation of agents.</summary>
            public float DestinationRateLimitSeconds;

            /// <summary>Upper limit on the iterations performed in a NavMeshQuery to find a path in the NavPlanSystem.</summary>
            public int IterationMax;

            /// <summary>Upper limit on the path node pool size for each NavMeshQuery created in the NavMeshQuerySystem. May need to be increased if an OutOfNodes error arises while finding a path.</summary>
            public int PathMeshQueryNodeMax;

            /// <summary>Upper limit on the search area size during path planning.</summary>
            public int PathSearchMax;
        }

        protected override void OnUpdate() { }
    }
}
