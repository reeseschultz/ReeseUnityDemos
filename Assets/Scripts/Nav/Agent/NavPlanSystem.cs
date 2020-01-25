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
    /// NavMeshQueries. They're challenging to deal with since they maintain
    /// state but are also NativeContainers, meaning that they cannot be used
    /// in another NativeContainer. Thus, each entity gets its own NavMeshQuery
    /// in said dictionary by index. IJobs with this data are manually batched
    /// to achieve Burst compilation. All that said, the NavMeshQuery
    /// orchestration here appears to be exemplary usage. Note that it depends
    /// on the third-party PathUtils.</summary>
    class NavPlanSystem : JobComponentSystem
    {
        /// <summary>A dictionary of NavMeshQueries. They cannot be placed in
        /// another NativeContainer, hence the dictionary. But that would make
        /// life much easier. Then a better solution *would* be a separate
        /// NavMeshQuerySystem that assigns them to a publicly accessible
        /// NativeArray the size of JobsUtility.MaxJobThreadCount. Then the
        /// native thread index could be injected into the planning job to
        /// reference the queries in a thread-safe manner, assuming that
        /// previous queries for a given NavAgent can be reduced to no
        /// consequence.</summary>
        Dictionary<int, NavMeshQuery> queryDictionary = new Dictionary<int, NavMeshQuery>();

        [BurstCompile]
        struct PlanJob : IJob
        {
            [ReadOnly]
            public Entity Entity;

            [ReadOnly]
            public NavAgent Agent;

            [ReadOnly]
            public Matrix4x4 ChildTransform;

            [ReadOnly]
            public Matrix4x4 ParentTransform;

            public NavMeshQuery NavMeshQuery;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<NavPathBufferElement> PathBufferFromEntity;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<NavJumpBufferElement> JumpBufferFromEntity;

            public void Execute()
            {
                Vector3 worldPosition = ChildTransform.GetColumn(3);
                Vector3 worldDestination = Agent.WorldDestination;

                if (Agent.IsJumping)
                {
                    worldPosition = Agent.WorldDestination;
                    worldDestination = ChildTransform.GetColumn(3);
                }

                var status = NavMeshQuery.BeginFindPath( // Note that BeginFindPath(...) may return 'Success' (this is not documented in UnityEngine.Experimental.AI).
                    NavMeshQuery.MapLocation(worldPosition, Vector3.one * NavConstants.PATH_SEARCH_MAX, Agent.TypeID),
                    NavMeshQuery.MapLocation(worldDestination, Vector3.one * NavConstants.PATH_SEARCH_MAX, Agent.TypeID),
                    NavMesh.AllAreas
                );

                while (status == PathQueryStatus.InProgress) status = NavMeshQuery.UpdateFindPath(
                    NavConstants.ITERATION_MAX,
                    out int iterationsPerformed
                );

                if (status != PathQueryStatus.Success) return;

                NavMeshQuery.EndFindPath(out int pathLength);

                var polygonIdArray = new NativeArray<PolygonId>(
                    NavConstants.PATH_NODE_MAX,
                    Allocator.Temp
                );

                NavMeshQuery.GetPathResult(polygonIdArray);

                var len = pathLength + 1; // Must add one to pathLength in case BeginFindPath returned 'Success'.
                var straightPath = new NativeArray<NavMeshLocation>(len, Allocator.Temp);
                var straightPathFlags = new NativeArray<StraightPathFlags>(len, Allocator.Temp);
                var vertexSide = new NativeArray<float>(len, Allocator.Temp);
                var straightPathCount = 0;

                status = PathUtils.FindStraightPath(
                    NavMeshQuery,
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

                if (Agent.IsJumping)
                {
                    var lastValidPoint = float3.zero;
                    for (int i = 0; i < straightPath.Length; ++i)
                        if (NavMeshQuery.IsValid(straightPath[i].polygon)) lastValidPoint = straightPath[i].position;
                        else break;

                    JumpBufferFromEntity[Entity].Add((float3)ParentTransform.inverse.MultiplyPoint3x4(lastValidPoint + Agent.Offset));
                }
                else if (status == PathQueryStatus.Success)
                {
                    var pathBuffer = PathBufferFromEntity[Entity];
                    pathBuffer.Clear();

                    for (int i = 0; i < straightPathCount; ++i)
                        pathBuffer.Add((float3)ParentTransform.inverse.MultiplyPoint3x4((float3)straightPath[i].position) + Agent.Offset);
                }

                polygonIdArray.Dispose();
                straightPath.Dispose();
                straightPathFlags.Dispose();
                vertexSide.Dispose();
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle job = inputDeps;
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
                job = new PlanJob
                {
                    Entity = entity,
                    NavMeshQuery = navMeshQuery,
                    PathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>(),
                    JumpBufferFromEntity = GetBufferFromEntity<NavJumpBufferElement>(),
                    Agent = agent,
                    ChildTransform = GetComponentDataFromEntity<LocalToWorld>(true)[entity].Value,
                    ParentTransform = GetComponentDataFromEntity<LocalToWorld>(true)[parent].Value
                }.Schedule(job);
            }

            return job;
        }

        protected override void OnDestroy()
        {
            foreach (var query in queryDictionary.Values) query.Dispose();
        }
    }
}
