using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

namespace Reese.Nav
{
    /// <summary>Plans paths and "jumpable" positions using
    /// UnityEngine.Experimental.AI. Each entity gets its own NavMeshQuery by
    /// thread index. NavMeshQuery orchestration here appears to be exemplary
    /// usage. Note that it depends on the third-party PathUtils.</summary>
    [UpdateBefore(typeof(NavDestinationSystem))]
    unsafe public class NavPlanSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var translationFromEntity = GetComponentDataFromEntity<Translation>(true);
            var jumpingFromEntity = GetComponentDataFromEntity<NavJumping>(true);
            var pathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>();
            var jumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>();
            var navMeshQueryPointerArray = World.GetExistingSystem<NavMeshQuerySystem>().PointerArray;

            Entities
                .WithAll<NavPlanning, LocalToParent>()
                .WithReadOnly(localToWorldFromEntity)
                .WithReadOnly(jumpingFromEntity)
                .WithNativeDisableParallelForRestriction(pathBufferFromEntity)
                .WithNativeDisableParallelForRestriction(jumpBufferFromEntity)
                .WithNativeDisableParallelForRestriction(navMeshQueryPointerArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref NavAgent agent, in Parent surface) =>
                {
                    if (
                        surface.Value.Equals(Entity.Null) ||
                        agent.DestinationSurface.Equals(Entity.Null) ||
                        !localToWorldFromEntity.Exists(surface.Value) ||
                        !localToWorldFromEntity.Exists(agent.DestinationSurface)
                    ) return;

                    var agentPosition = localToWorldFromEntity[entity].Position;
                    var worldPosition = agentPosition;
                    var worldDestination = NavUtil.MultiplyPoint3x4(
                        localToWorldFromEntity[agent.DestinationSurface].Value,
                        agent.LocalDestination
                    );

                    var jumping = jumpingFromEntity.Exists(entity);

                    if (jumping)
                    {
                        worldPosition = worldDestination;
                        worldDestination = agentPosition;
                    }

                    var navMeshQueryPointer = navMeshQueryPointerArray[nativeThreadIndex];
                    UnsafeUtility.CopyPtrToStructure(navMeshQueryPointer.Value, out NavMeshQuery navMeshQuery);

                    var status = navMeshQuery.BeginFindPath(
                        navMeshQuery.MapLocation(worldPosition, Vector3.one * NavConstants.PATH_SEARCH_MAX, agent.TypeID),
                        navMeshQuery.MapLocation(worldDestination, Vector3.one * NavConstants.PATH_SEARCH_MAX, agent.TypeID),
                        NavMesh.AllAreas
                    );

                    while (status == PathQueryStatus.InProgress) status = navMeshQuery.UpdateFindPath(
                        NavConstants.ITERATION_MAX,
                        out int iterationsPerformed
                    );

                    if (status != PathQueryStatus.Success && status != PathQueryStatus.InProgress)
                    {
                        //Debug.Log("Bad destination, removing plan. Status: " + status);
                        commandBuffer.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);
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

                    var jumpBuffer = !jumpBufferFromEntity.Exists(entity) ? commandBuffer.AddBuffer<NavJumpBufferElement>(entityInQueryIndex, entity) : jumpBufferFromEntity[entity];
                    var pathBuffer = !pathBufferFromEntity.Exists(entity) ? commandBuffer.AddBuffer<NavPathBufferElement>(entityInQueryIndex, entity) : pathBufferFromEntity[entity];

                    if (jumping)
                    {
                        var lastValidPoint = float3.zero;
                        for (int i = 0; i < straightPath.Length; ++i)
                            if (navMeshQuery.IsValid(straightPath[i].polygon)) lastValidPoint = straightPath[i].position;
                            else break;

                        jumpBuffer.Add(
                            NavUtil.MultiplyPoint3x4(
                                math.inverse(localToWorldFromEntity[agent.DestinationSurface].Value),
                                (float3)lastValidPoint + agent.Offset
                            )
                        );

                        if (jumpBuffer.Length > 0)
                        {
                            commandBuffer.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);
                            commandBuffer.AddComponent<NavLerping>(entityInQueryIndex, entity);
                        }
                    }
                    else if (status == PathQueryStatus.Success)
                    {
                        pathBuffer.Clear();

                        for (int i = 0; i < straightPathCount; ++i) pathBuffer.Add(
                            NavUtil.MultiplyPoint3x4(
                                math.inverse(localToWorldFromEntity[surface.Value].Value),
                                (float3)straightPath[i].position + agent.Offset
                            )
                        );

                        if (pathBuffer.Length > 0)
                        {
                            commandBuffer.RemoveComponent<NavPlanning>(entityInQueryIndex, entity);
                            commandBuffer.AddComponent<NavLerping>(entityInQueryIndex, entity);
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
