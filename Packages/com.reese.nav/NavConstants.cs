namespace Reese.Nav
{
    /// </summary>Compile-time constants. See the NavSystem for how runtime settings are managed.</summary>
    public static class NavConstants
    {
        /// <summary>Upper limit on a given jumpable surface buffer. Exceeding this merely results in allocation of heap memory.</summary>
        public const int JUMPABLE_SURFACE_MAX = 30;

        /// <summary>Upper limit on a given path buffer. Exceeding this merely results in allocation of heap memory.</summary>
        public const int PATH_NODE_MAX = 1000;

        /// <summary>The 'Humanoid' NavMesh agent type as a string.</summary>
        public const string HUMANOID = "Humanoid";
    }
}
