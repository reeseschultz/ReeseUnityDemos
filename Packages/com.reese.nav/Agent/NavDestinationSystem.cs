using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Reese.Nav
{
    /// <summary>TODO</summary>
    [UpdateAfter(typeof(NavSurfaceSystem))]
    class NavDestinationSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            return Entities
                .WithChangeFilter<NavDestination>()
                .WithAll<LocalToParent>()
                .WithReadOnly(localToWorldFromEntity)
                .ForEach((Entity entity, int entityInQueryIndex, ref NavAgent agent, in NavDestination destination, in Parent surface) =>
                {
                    agent.LocalDestination = NavUtil.MultiplyPoint3x4(
                        math.inverse(localToWorldFromEntity[surface.Value].Value),
                        destination.Value
                    );
                })
                .WithName("NavDestinationJob")
                .Schedule(inputDeps);
        }
    }
}
