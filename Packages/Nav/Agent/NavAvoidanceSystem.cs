// Adapted from the avoidance implementation by Zulfa Juniadi (zulfajuniadi)
// that can be found here: https://github.com/zulfajuniadi/unity-ecs-navmesh.
//
// Here's Zulfa's GitHub Profile: https://github.com/zulfajuniadi.
//
// Zulfa's software is (MIT) licensed with the following terms:
//
// Copyright 2018 Zulfa Juniadi
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Nav
{
    /// <summary>Exploiting IJobNativeMultiHashMapMergedSharedKeyIndices is
    /// this system's prerogative, thus accomplishing agent-to-agent
    /// avoidance as such. See Unity's "Boids" example code for more info
    /// (https://github.com/Unity-Technologies/EntityComponentSystemSamples)
    /// since this system uses the same hashing strategy.
    /// </summary>
    public class NavAvoidanceSystem : JobComponentSystem
    {
        /// <summary>For removing the NavLerping component and adding the
        /// NavPlanning and NavAvoidant components for agents lacking
        /// personal space.</summary>
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        /// <summary>For querying agents that might be avoidant of one another.
        /// </summary>
        EntityQuery agentQuery;

        protected override void OnCreate()
        {
            agentQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new[] {
                    ComponentType.ReadOnly<NavAvoidant>(),
                    ComponentType.ReadOnly<NavPlanning>()
                },
                All = new[] {
                    ComponentType.ReadOnly<NavLerping>(),
                    ComponentType.ReadOnly<Parent>(),
                    ComponentType.ReadOnly<LocalToParent>()
                }
            });

            RequireForUpdate(agentQuery);

            Enabled = NavConstants.AVOIDANCE_ENABLED_ON_CREATE;
        }

        [BurstCompile]
        struct AvoidJob : IJobNativeMultiHashMapMergedSharedKeyIndices // There's no syntactic sugar for this beast yet.
        {
            [ReadOnly]
            public float DeltaSeconds;

            [ReadOnly]
            public ComponentDataFromEntity<Rotation> RotationFromEntity;

            [ReadOnly]
            public BufferFromEntity<NavPathBufferElement> PathBufferFromEntity;

            [ReadOnly]
            public ComponentDataFromEntity<Parent> ParentFromEntity;

            [ReadOnly]
            public ComponentDataFromEntity<LocalToWorld> LocalToWorldFromEntity;

            [ReadOnly, DeallocateOnJobCompletion]
            public NativeArray<Entity> AgentEntityArray;

            [WriteOnly]
            public EntityCommandBuffer.Concurrent CommandBuffer;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<NavAgent> AgentFromEntity;

            public void ExecuteNext(int firstIndex, int index)
            {
                if (index > NavConstants.AGENTS_PER_CELL_MAX) return;

                var agent = AgentFromEntity[AgentEntityArray[index]];

                if (agent.Surface.Equals(Entity.Null) || !ParentFromEntity.Exists(agent.Surface)) return;

                var basis = ParentFromEntity[agent.Surface].Value;

                if (basis.Equals(Entity.Null) || !LocalToWorldFromEntity.Exists(basis)) return;

                var basisTransform = (Matrix4x4)LocalToWorldFromEntity[basis].Value;

                var pathBuffer = PathBufferFromEntity[AgentEntityArray[index]];

                if (pathBuffer.Length == 0 || agent.PathBufferIndex >= pathBuffer.Length) return;

                var rotation = RotationFromEntity[AgentEntityArray[index]];

                float3 avoidanceDestination = index % 2 == 1 ? Vector3.right : Vector3.left;
                avoidanceDestination = (Quaternion)rotation.Value * ((float3)Vector3.forward + avoidanceDestination) * agent.TranslationSpeed * DeltaSeconds;
                avoidanceDestination += pathBuffer[agent.PathBufferIndex];

                agent.AvoidanceDestination = basisTransform.MultiplyPoint3x4(avoidanceDestination - agent.Offset);
                AgentFromEntity[AgentEntityArray[index]] = agent;

                CommandBuffer.RemoveComponent<NavLerping>(index, AgentEntityArray[index]);
                CommandBuffer.AddComponent<NavPlanning>(index, AgentEntityArray[index]);
                CommandBuffer.AddComponent<NavAvoidant>(index, AgentEntityArray[index]);
            }

            public void ExecuteFirst(int index) { }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var agentCount = agentQuery.CalculateEntityCount();
            if (agentCount == 0)
            {
                agentQuery.ResetFilter();
                return inputDeps;
            }

            var hashMap = new NativeMultiHashMap<int, int>(agentCount, Allocator.TempJob);
            var parallelHashMap = hashMap.AsParallelWriter();

            var hashJob = Entities
                .WithNone<NavAvoidant, NavPlanning>()
                .WithAll<NavLerping, Parent, LocalToParent>()
                .WithNativeDisableContainerSafetyRestriction(parallelHashMap)
                .ForEach((int entityInQueryIndex, in LocalToWorld localToWorld) =>
                {
                    parallelHashMap.Add(
                        (int)math.hash(new int3(math.floor(localToWorld.Position / NavConstants.AVOIDANCE_CELL_RADIUS))),
                        entityInQueryIndex
                    );
                })
                .WithName("NavHashJob")
                .Schedule(inputDeps);

            agentQuery.AddDependency(hashJob);
            agentQuery.ResetFilter();

            var avoidJob = new AvoidJob
            {
                DeltaSeconds = Time.DeltaTime,
                AgentEntityArray = agentQuery.ToEntityArray(Allocator.TempJob),
                AgentFromEntity = GetComponentDataFromEntity<NavAgent>(),
                RotationFromEntity = GetComponentDataFromEntity<Rotation>(true),
                LocalToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true),
                ParentFromEntity = GetComponentDataFromEntity<Parent>(true),
                PathBufferFromEntity = GetBufferFromEntity<NavPathBufferElement>(true),
                CommandBuffer = barrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(hashMap, 64, hashJob);

            avoidJob = hashMap.Dispose(avoidJob);
            barrier.AddJobHandleForProducer(avoidJob);

            return avoidJob;
        }
    }
}
