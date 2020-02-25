namespace Reese.Nav
{
    public static class NavConstants
    {
        /// <summary>Whether NavAgent avoidance is enabled upon creation of the
        /// NavAvoidanceSystem. If you don't care about agent avoidance, set
        /// this to false for performance gains.</summary>
        public const bool AVOIDANCE_ENABLED_ON_CREATE = true;

        /// <summary>The cell radius for NavAgent avoidance.</summary>
        public const float AVOIDANCE_CELL_RADIUS = 3;

        /// <summary>Upper limit on the *duration* spent jumping before the
        /// agent is actually considered falling. This limit can be reached 
        /// when the agent tries to jump too close to the edge of a surface
        /// and misses.</summary>
        public const float JUMP_SECONDS_MAX = 5;

        /// <summary>Upper limit on the raycast distance when searching
        /// for an obstacle in front of a given NavAgent.</summary>
        public const float OBSTACLE_RAYCAST_DISTANCE_MAX = 1000;

        /// <summary>Upper limit on the raycast distance when searching for a
        /// surface below a given NavAgent.</summary>
        public const float SURFACE_RAYCAST_DISTANCE_MAX = 1000;

        /// <summary>Upper limit on the NavAgents the NavAvoidanceSystem will
        /// attempt to process per cell. Keeping this low drastically improves
        /// performance. If there's 1000 agents in a single cell, do you really
        /// want to make them all avoid each other? No, because they're already
        /// colliding anyway.</summary>
        public const int AGENTS_PER_CELL_MAX = 25;

        /// <summary>Upper limit when manually batching jobs.</summary>
        public const int BATCH_MAX = 50;

        /// <summary>Upper limit on the iterations performed in a NavMeshQuery
        /// to find a path in the NavPlanSystem.</summary>
        public const int ITERATION_MAX = 1000;

        /// <summary>Upper limit on a given jumpable surface buffer.
        /// Exceeding this will merely result in heap memory blocks being
        /// allocated.</summary>
        public const int JUMPABLE_SURFACE_MAX = 30;

        /// <summary>Upper limit on a given path buffer. Exceeding this will
        /// merely result in heap memory blocks being allocated.</summary>
        public const int PATH_NODE_MAX = 1000;

        /// <summary>Upper limit on the search area size during path planning.
        /// </summary>
        public const int PATH_SEARCH_MAX = 1000;

        /// <summary>Upper limit on the number of raycasts to attempt in
        /// searching for a surface below the NavAgent. Exceeding this implies
        /// that there is no surface below the agent, its then determined to be
        /// falling which means that no more raycasts will be performed.
        /// </summary>
        public const int SURFACE_RAYCAST_MAX = 100;

        /// <summary>The 'Humanoid' NavMesh agent type as a string.</summary>
        public const string HUMANOID = "Humanoid";
    }
}
