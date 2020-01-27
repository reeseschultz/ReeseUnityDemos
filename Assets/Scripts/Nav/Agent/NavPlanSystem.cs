using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

namespace Reese.Nav
{
    /// <summary>A hacky system for planning paths and "jumpable" positions
    /// using UnityEngine.Experimental.AI. Works with a dictionary of
    /// NavMeshQueries. Each entity gets its own NavMeshQuery in said
    /// dictionary by index. IJobs with this data are manually batched
    /// to achieve Burst compilation. All that said, the NavMeshQuery
    /// orchestration here appears to be exemplary usage. Note that it depends
    /// on the third-party PathUtils.</summary>
    class NavPlanSystem : JobComponentSystem
    {
        /// <summary>A dictionary of NavMeshQueries. They cannot be placed in
        /// another NativeContainer, hence the dictionary.</summary>
        Dictionary<int, NavMeshQuery> queryDictionary = new Dictionary<int, NavMeshQuery>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = inputDeps;
            for (int i = 0; i < NavConstants.BATCH_MAX; ++i)
            {
                var entity = NavQueueSystem.DequeueJumpPlanning();

                if (entity.Equals(Entity.Null)) entity = NavQueueSystem.DequeuePathPlanning();
                if (entity.Equals(Entity.Null)) break;
                if (!World.EntityManager.Exists(entity)) continue;

                if (!queryDictionary.ContainsKey(entity.Index)) queryDictionary.Add(entity.Index, new NavMeshQuery(
                    NavMeshWorld.GetDefaultWorld(),
                    Allocator.Persistent,
                    NavConstants.PATH_NODE_MAX
                ));

                if (!World.EntityManager.HasComponent<NavAgent>(entity)) continue;

                var agent = GetComponentDataFromEntity<NavAgent>(true)[entity];

                if (agent.Surface.Equals(Entity.Null)) continue;

                var parent = GetComponentDataFromEntity<Parent>(true)[agent.Surface].Value;
                if (!agent.DestinationSurface.Equals(Entity.Null))
                {
                    var destinationTransform = (Matrix4x4)GetComponentDataFromEntity<LocalToWorld>(true)[agent.DestinationSurface].Value;
                    agent.WorldDestination = destinationTransform.MultiplyPoint3x4(agent.LocalDestination);

                    if (agent.IsJumping) parent = GetComponentDataFromEntity<Parent>(true)[agent.DestinationSurface].Value;
                }

                queryDictionary.TryGetValue(entity.Index, out NavMeshQuery navMeshQuery);

                var pathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>();
                var jumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>();

                var childTransform = (Matrix4x4)GetComponentDataFromEntity<LocalToWorld>(true)[entity].Value;
                var parentTransform = (Matrix4x4)GetComponentDataFromEntity<LocalToWorld>(true)[parent].Value;

                job = Job
                    .WithNativeDisableParallelForRestriction(pathBufferFromEntity)
                    .WithNativeDisableParallelForRestriction(jumpBufferFromEntity)
                    .WithCode(() => {
                        Vector3 worldPosition = childTransform.GetColumn(3);
                        Vector3 worldDestination = agent.WorldDestination;

                        if (agent.IsJumping)
                        {
                            worldPosition = agent.WorldDestination;
                            worldDestination = childTransform.GetColumn(3);
                        }

                        var status = navMeshQuery.BeginFindPath(
                            navMeshQuery.MapLocation(worldPosition, Vector3.one * NavConstants.PATH_SEARCH_MAX, agent.TypeID),
                            navMeshQuery.MapLocation(worldDestination, Vector3.one * NavConstants.PATH_SEARCH_MAX, agent.TypeID),
                            NavMesh.AllAreas
                        );

                        while (status == PathQueryStatus.InProgress) status = navMeshQuery.UpdateFindPath(
                            NavConstants.ITERATION_MAX,
                            out int iterationsPerformed
                        );

                        if (status != PathQueryStatus.Success) return;

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

                        if (agent.IsJumping)
                        {
                            var lastValidPoint = float3.zero;
                            for (int j = 0; j < straightPath.Length; ++j)
                                if (navMeshQuery.IsValid(straightPath[j].polygon)) lastValidPoint = straightPath[j].position;
                                else break;

                            jumpBufferFromEntity[entity].Add((float3)parentTransform.inverse.MultiplyPoint3x4(lastValidPoint + agent.Offset));
                        }
                        else if (status == PathQueryStatus.Success)
                        {
                            var pathBuffer = pathBufferFromEntity[entity];
                            pathBuffer.Clear();

                            for (int j = 0; j < straightPathCount; ++j)
                                pathBuffer.Add((float3)parentTransform.inverse.MultiplyPoint3x4((float3)straightPath[j].position) + agent.Offset);
                        }

                        polygonIdArray.Dispose();
                        straightPath.Dispose();
                        straightPathFlags.Dispose();
                        vertexSide.Dispose();
                    })
                    .WithName("NavPlanJob")
                    .Schedule(job);

                NavMeshWorld.GetDefaultWorld().AddDependency(job);
            }

            return job;
        }

        protected override void OnDestroy()
        {
            foreach (var query in queryDictionary.Values) query.Dispose();
        }
    }
}
