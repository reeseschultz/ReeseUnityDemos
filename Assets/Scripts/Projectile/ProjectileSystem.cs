using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReeseUnityDemos
{
    class ProjectileSystem : JobComponentSystem
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        static Scene scene => SceneManager.GetActiveScene();
        EntityQuery randomBufferQuery;

        protected override void OnCreate()
        {
            randomBufferQuery = GetEntityQuery(typeof(RandomBufferElement));
        }

        [BurstCompile]
        struct AddProjectileJob : IJobForEachWithEntity<Person>
        {
            [ReadOnly]
            public ComponentDataFromEntity<Projectile> ProjectileFromEntity;

            [WriteOnly]
            public EntityCommandBuffer.Concurrent CommandBuffer;

            [NativeDisableParallelForRestriction]
            public DynamicBuffer<RandomBufferElement> RandomBuffer;

            [NativeSetThreadIndex]
            int threadIndex;

            public void Execute(Entity entity, int index, [ReadOnly] ref Person person)
            {
                if (ProjectileFromEntity.Exists(entity) && ProjectileFromEntity[entity].HasTarget) return;

                var randomBuffer = RandomBuffer[threadIndex];

                CommandBuffer.AddComponent(index, entity, new Projectile
                {
                    AngleInDegrees = randomBuffer.Value.NextInt(45, 60),
                    Gravity = randomBuffer.Value.NextInt(50, 150),
                    Target = new float3(
                        randomBuffer.Value.NextInt(-10, 10),
                        2.5f,
                        randomBuffer.Value.NextInt(-10, 10)
                    )
                });

                RandomBuffer[threadIndex] = randomBuffer;
            }
        }

        [BurstCompile]
        struct LaunchJob : IJobForEach<Projectile, Translation>
        {
            [ReadOnly]
            public float DeltaTime;

            public void Execute(ref Projectile projectile, ref Translation translation)
            {
                if (!projectile.HasTarget) return;

                var velocity = Vector3.Distance(translation.Value, projectile.Target) / (math.sin(2 * math.radians(projectile.AngleInDegrees)) / projectile.Gravity);
                var xVelocity = math.sqrt(velocity) * math.cos(math.radians(projectile.AngleInDegrees));
                var yVelocity = math.sqrt(velocity) * math.sin(math.radians(projectile.AngleInDegrees));

                translation.Value.y += (yVelocity - projectile.FlightDurationInSeconds * projectile.Gravity) * DeltaTime;
                translation.Value = Vector3.MoveTowards(translation.Value, projectile.Target, xVelocity * DeltaTime);

                projectile.FlightDurationInSeconds += DeltaTime;

                if (translation.Value.y >= projectile.Target.y) return;

                projectile.FlightDurationInSeconds = 0;
                projectile.HasTarget = false;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!scene.name.Equals("ProjectileDemo")) return inputDeps;

            var randomBufferEntities = randomBufferQuery.ToEntityArray(Allocator.TempJob);
            if (randomBufferEntities.Length == 0) return inputDeps;
            var randomBuffer = GetBufferFromEntity<RandomBufferElement>()[randomBufferEntities[0]];
            randomBufferEntities.Dispose();

            var addProjectileJob = new AddProjectileJob
            {
                RandomBuffer = randomBuffer,
                ProjectileFromEntity = GetComponentDataFromEntity<Projectile>(true),
                CommandBuffer = barrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inputDeps);

            barrier.AddJobHandleForProducer(addProjectileJob);
            addProjectileJob.Complete();

            var launchJob = new LaunchJob
            {
                DeltaTime = Time.DeltaTime
            }.Schedule(this, inputDeps);

            return JobHandle.CombineDependencies(addProjectileJob, launchJob);
        }
    }
}
