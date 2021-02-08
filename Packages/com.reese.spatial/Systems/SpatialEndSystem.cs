using Unity.Entities;
using Unity.Jobs;

namespace Reese.Spatial
{
    /// <summary>Clears the entry and exit buffers.</summary>
    [UpdateAfter(typeof(SpatialStartSystem))]
    public class SpatialEndSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<SpatialTrigger>()
                .WithChangeFilter<SpatialEntry>()
                .ForEach((ref DynamicBuffer<SpatialEntry> entries) => {
                    entries.Clear();
                })
                .WithName("SpatialClearEntriesJob")
                .ScheduleParallel();

            Entities
                .WithAll<SpatialTrigger>()
                .WithChangeFilter<SpatialExit>()
                .ForEach((ref DynamicBuffer<SpatialExit> exits) => {
                    exits.Clear();
                })
                .WithName("SpatialClearExitsJob")
                .ScheduleParallel();
        }
    }
}
