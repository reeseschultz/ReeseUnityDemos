namespace Reese.Nav
{
    static class NavConstants
    {
        /// <summary>Upper limit on the raycast distance when search for an
        /// obstacle in front of a given NavAgent.</summary>
        public const float OBSTACLE_RAYCAST_DISTANCE_MAX = 1000;

        /// <summary>Upper limit on the raycast distance when searching for a
        /// surface below a given NavAgent.</summary>
        public const float SURFACE_RAYCAST_DISTANCE_MAX = 1000;

        /// <summary>Upper limit when manually batching jobs.</summary>
        public const int BATCH_MAX = 50;

        /// <summary>Upper limit on the iterations performed in a NavMeshQuery
        /// to find a path in the NavPlanSystem.</summary>
        public const int ITERATION_MAX = 1000;

        /// <summary>Upper limit on a given jumpable surfaces buffer.
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
        public const int RAYCAST_MAX = 100;

        /// <summary>The 'Humanoid' NavMesh agent type as a string.</summary>
        public const string HUMANOID = "Humanoid";
    }
}
