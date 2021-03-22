namespace Reese.Spatial
{
    /// </summary>Compile-time constants.</summary>
    public static class SpatialConstants
    {
        /// <summary>Capacity of the spatial entry buffer. Setting it greater than zero will result in allocation of the stack, instead of the heap, if utilized. Avoid stack allocation due to its 16KB limit per entity for all cumulative components and buffers (i.e., the archetype). For dynamic buffers, heap allocation has negligible, if noticeable, performance impact.</summary>
        public const int SPATIAL_ENTRY_BUFFER_CAPACITY = 0;

        /// <summary>Capacity of the spatial exit buffer. Setting it greater than zero will result in allocation of the stack, instead of the heap, if utilized. Avoid stack allocation due to its 16KB limit per entity for all cumulative components and buffers (i.e., the archetype). For dynamic buffers, heap allocation has negligible, if noticeable, performance impact.</summary>
        public const int SPATIAL_EXIT_BUFFER_CAPACITY = 0;

        /// <summary>Capacity of the spatial overlap buffer. Setting it greater than zero will result in allocation of the stack, instead of the heap, if utilized. Avoid stack allocation due to its 16KB limit per entity for all cumulative components and buffers (i.e., the archetype). For dynamic buffers, heap allocation has negligible, if noticeable, performance impact.</summary>
        public const int SPATIAL_OVERLAP_BUFFER_CAPACITY = 0;

        /// <summary>Capacity of the spatial tag buffer. Setting it greater than zero will result in allocation of the stack, instead of the heap, if utilized. Avoid stack allocation due to its 16KB limit per entity for all cumulative components and buffers (i.e., the archetype). For dynamic buffers, heap allocation has negligible, if noticeable, performance impact.</summary>
        public const int SPATIAL_TAG_BUFFER_CAPACITY = 0;
    }
}
