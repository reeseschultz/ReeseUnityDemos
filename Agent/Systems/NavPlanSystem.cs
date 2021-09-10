using Reese.Math;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

namespace Reese.Nav
{
    /// <summary>Plans paths and "jumpable" positions using UnityEngine.Experimental.AI. Each entity gets its own NavMeshQuery by thread index. This depends on the third-party PathUtils.</summary>
    unsafe public class NavPlanSystem : SystemBase
    {
        NavSystem navSystem => World.GetOrCreateSystem<NavSystem>();

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var jumpingFromEntity = GetComponentDataFromEntity<NavJumping>(true);
            var pathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>();
            var jumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>();
            var navMeshQueryPointerArray = World.GetExistingSystem<NavMeshQuerySystem>().PointerArray;
            var settings = navSystem.Settings;

            Entities
                .WithNone<NavProblem>()
                .WithAll<NavPlanning, LocalToParent>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(jumpingFromEntity)
                .WithNativeDisableParallelForRestriction(pathBufferFromEntity)
                .WithNativeDisableParallelForRestriction(jumpBufferFromEntity)
                .WithNativeDisableParallelForRestriction(navMeshQueryPointerArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref NavAgent agent, in Parent surface, in NavDestination destination) =>
                {
                    if (
                        surface.Value.Equals(Entity.Null) ||
                        agent.DestinationSurface.Equals(Entity.Null) ||
                        !localToWorldFromEntity.HasComponent(surface.Value) ||
                        !localToWorldFromEntity.HasComponent(agent.DestinationSurface)
                    ) return;

                    var agentPosition = localToWorldFromEntity[entity].Position;
                    var worldPosition = agentPosition;
                    var worldDestination = agent.LocalDestination.ToWorld(localToWorldFromEntity[agent.DestinationSurface]);

                    var jumping = jumpingFromEntity.HasComponent(entity);

                    if (jumping)
                    {
                        worldPosition = worldDestination;
                        worldDestination = agentPosition;
                    }

                    var navMeshQueryPointer = navMeshQueryPointerArray[nativeThreadIndex];
                    UnsafeUtility.CopyPtrToStructure(navMeshQueryPointer.Value, out NavMeshQuery navMeshQuery);

                    var one = new float3(1);

                    var status = navMeshQuery.BeginFindPath(
                        navMeshQuery.MapLocation(worldPosition, one * settings.PathSearchMax, agent.TypeID),
                        navMeshQuery.MapLocation(worldDestination, one * settings.PathSearchMax, agent.TypeID),
                        NavMesh.AllAreas
                    );

                    while (NavUtil.HasStatus(status, PathQueryStatus.InProgress)) status = navMeshQuery.UpdateFindPath(
                        settings.IterationMax,
                        out int iterationsPerformed
                    );

                    var customLerp = destination.CustomLerp;

                    if (!NavUtil.HasStatus(status, PathQueryStatus.Success))
                    {
                        commandBuffer.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);

                        commandBuffer.RemoveComponent<NavDestination>(entityInQueryIndex, entity);

                        commandBuffer.AddComponent(entityInQueryIndex, entity, new NavProblem
                        {
                            Value = status
                        });

                        return;
                    }

                    navMeshQuery.EndFindPath(out int pathLength);

                    var polygonIdArray = new NativeArray<PolygonId>(
                        NavConstants.PATH_NODE_MAX,
                        Allocator.Temp
                    );

                    navMeshQuery.GetPathResult(polygonIdArray);

                    var len = pathLength + 1;
                    var straightPath = new NativeArray<NavMeshLocation>(len, Allocator.Temp);
                    var straightPathFlags = new NativeArray<StraightPathFlags>(len, Allocator.Temp);
                    var vertexSide = new NativeArray<float>(len, Allocator.Temp);
                    var straightPathCount = 0;

                    status = PathUtils.FindStraightPath(
                        navMeshQuery,
                        worldPosition,
                        worldDestination,
                        polygonIdArray,
                        pathLength,
                        ref straightPath,
                        ref straightPathFlags,
                        ref vertexSide,
                        ref straightPathCount,
                        NavConstants.PATH_NODE_MAX
                    );

                    var jumpBuffer = !jumpBufferFromEntity.HasComponent(entity) ? commandBuffer.AddBuffer<NavJumpBufferElement>(entityInQueryIndex, entity) : jumpBufferFromEntity[entity];
                    var pathBuffer = !pathBufferFromEntity.HasComponent(entity) ? commandBuffer.AddBuffer<NavPathBufferElement>(entityInQueryIndex, entity) : pathBufferFromEntity[entity];

                    if (jumping)
                    {
                        var lastValidPoint = float3.zero;
                        for (var i = 0; i < straightPath.Length; ++i)
                            if (navMeshQuery.IsValid(straightPath[i].polygon)) lastValidPoint = straightPath[i].position;
                            else break;

                        jumpBuffer.Add((lastValidPoint + agent.Offset).ToLocal(localToWorldFromEntity[agent.DestinationSurface]));

                        if (jumpBuffer.Length > 0)
                        {
                            commandBuffer.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);

                            if (customLerp) commandBuffer.AddComponent<NavCustomLerping>(entityInQueryIndex, entity);
                            else commandBuffer.AddComponent<NavJumping>(entityInQueryIndex, entity);
                        }
                    }
                    else if (status == PathQueryStatus.Success)
                    {
                        if (pathBuffer.Length > 0) pathBuffer.RemoveAt(pathBuffer.Length - 1);

                        for (var i = straightPathCount - 1; i > 0; --i) pathBuffer.Add(
                            ((float3)straightPath[i].position + agent.Offset).ToLocal(localToWorldFromEntity[surface.Value])
                        );

                        if (pathBuffer.Length > 0)
                        {
                            commandBuffer.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);

                            if (customLerp) commandBuffer.AddComponent<NavCustomLerping>(entityInQueryIndex, entity);
                            else
                            {
                                commandBuffer.AddComponent<NavWalking>(entityInQueryIndex, entity);
                                commandBuffer.AddComponent<NavSteering>(entityInQueryIndex, entity);
                            }
                        }
                    }

                    polygonIdArray.Dispose();
                    straightPath.Dispose();
                    straightPathFlags.Dispose();
                    vertexSide.Dispose();
                })
                .WithName("NavPlanJob")
                .ScheduleParallel();

            NavMeshWorld.GetDefaultWorld().AddDependency(Dependency);
            barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
