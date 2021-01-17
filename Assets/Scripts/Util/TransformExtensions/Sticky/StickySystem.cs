using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using BuildPhysicsWorld = Unity.Physics.Systems.BuildPhysicsWorld;
using Reese.Nav;

namespace Reese.Demo
{
    public class StickySystem : SystemBase
    {
        BuildPhysicsWorld buildPhysicsWorld => World.GetOrCreateSystem<BuildPhysicsWorld>();

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            var physicsWorld = buildPhysicsWorld.PhysicsWorld;

            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);

            Dependency = JobHandle.CombineDependencies(Dependency, buildPhysicsWorld.GetOutputDependency());

            Entities
                .WithAll<LocalToWorld>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(physicsWorld)
                .ForEach((Entity entity, int entityInQueryIndex, ref Sticky sticky) =>
                {
                    if (sticky.StickAttempts-- <= 0)
                    {
                        commandBuffer.RemoveComponent<Sticky>(entityInQueryIndex, entity);
                        commandBuffer.AddComponent<StickyFailed>(entityInQueryIndex, entity);
                        return;
                    }

                    var worldPosition = localToWorldFromEntity[entity].Position;

                    var collider = SphereCollider.Create(
                        new SphereGeometry()
                        {
                            Center = worldPosition,
                            Radius = sticky.Radius
                        },
                        sticky.Filter
                    );

                    unsafe
                    {
                        var castInput = new ColliderCastInput()
                        {
                            Collider = (Collider*)collider.GetUnsafePtr(),
                            Orientation = quaternion.LookRotationSafe(sticky.WorldDirection, math.up())
                        };

                        if (!physicsWorld.CastCollider(castInput, out ColliderCastHit hit)) return;

                        commandBuffer.AddComponent(entityInQueryIndex, entity, new Parent
                        {
                            Value = hit.Entity
                        });

                        // var localPosition = NavUtil.MultiplyPoint3x4( // TODO : Transform extensions package.
                        //     math.inverse(localToWorldFromEntity[hit.Entity].Value),
                        //     worldPosition
                        // ) + hit.SurfaceNormal * sticky.Offset;

                        // commandBuffer.AddComponent(entityInQueryIndex, entity, new Translation
                        // {
                        //     Value = localPosition
                        // });

                        commandBuffer.RemoveComponent<Sticky>(entityInQueryIndex, entity);
                    }
                })
                .WithName("StickyJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
