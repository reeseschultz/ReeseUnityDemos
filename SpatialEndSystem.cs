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
                .WithChangeFilter<SpatialEntryBufferElement>()
                .ForEach((ref DynamicBuffer<SpatialEntryBufferElement> entries) => {
                    entries.Clear();
                })
                .WithName("SpatialClearEntriesJob")
                .ScheduleParallel();

            Entities
                .WithAll<SpatialTrigger>()
                .WithChangeFilter<SpatialExitBufferElement>()
                .ForEach((ref DynamicBuffer<SpatialExitBufferElement> exits) => {
                    exits.Clear();
                })
                .WithName("SpatialClearExitsJob")
                .ScheduleParallel();
        }
    }
}
