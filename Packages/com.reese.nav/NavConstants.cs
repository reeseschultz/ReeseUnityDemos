namespace Reese.Nav
{
    public static class NavConstants
    {
        /// <summary>A sphere collider of the specified radius is used to detect the destination surface.</summary>
        public const float DESTINATION_SURFACE_COLLIDER_RADIUS = 1;

        /// <summary>Upper limit on the *duration* spent jumping before the agent is actually considered falling. This limit can be reached when the agent tries to jump too close to the edge of a surface and misses.</summary>
        public const float JUMP_SECONDS_MAX = 5;

        /// <summary>Upper limit on the raycast distance when searching for an obstacle in front of a given NavAgent.</summary>
        public const float OBSTACLE_RAYCAST_DISTANCE_MAX = 1000;

        /// <summary>Upper limit on the raycast distance when searching for a surface below a given NavAgent.</summary>
        public const float SURFACE_RAYCAST_DISTANCE_MAX = 1000;

        /// <summary>The layer for surfaces.</summary>
        public const int SURFACE_LAYER = 28;

        /// <summary>The layer for obstacles.</summary>
        public const int OBSTACLE_LAYER = 29;

        /// <summary>The layer for colliders.</summary>
        public const int COLLIDER_LAYER = 30;

        /// <summary>Upper limit on the iterations performed in a NavMeshQuery to find a path in the NavPlanSystem.</summary>
        public const int ITERATION_MAX = 1000;

        /// <summary>Upper limit on a given jumpable surface buffer. Exceeding this merely results in allocation of heap memory.</summary>
        public const int JUMPABLE_SURFACE_MAX = 30;

        /// <summary>Upper limit on the path node pool size for each NavMeshQuery created in the NavMeshQuerySystem. May need to be increased if an OutOfNodes error arises while finding a path.</summary>
        public const int NAV_MESH_QUERY_NODE_MAX = 5000;

        /// <summary>The initial capacity of the map tracking agents that need a surface.</summary>
        public const int NEEDS_SURFACE_MAP_SIZE = 1000;

        /// <summary>Upper limit on a given path buffer. Exceeding this merely results in allocation of heap memory.</summary>
        public const int PATH_NODE_MAX = 1000;

        /// <summary>Upper limit on the search area size during path planning.</summary>
        public const int PATH_SEARCH_MAX = 1000;

        /// <summary>Upper limit on the number of raycasts to attempt in searching for a surface below the NavAgent. Exceeding this implies that there is no surface below the agent, its then determined to be falling which means that no more raycasts will be performed.</summary>
        public const int SURFACE_RAYCAST_MAX = 100;

        /// <summary>The 'Humanoid' NavMesh agent type as a string.</summary>
        public const string HUMANOID = "Humanoid";
    }
}
