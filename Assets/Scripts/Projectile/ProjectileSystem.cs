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

        [BurstCompile]
        struct AddProjectileJob : IJobForEachWithEntity<Person>
        {
            [ReadOnly]
            public ComponentDataFromEntity<Projectile> ProjectileFromEntity;

            [WriteOnly]
            public EntityCommandBuffer.Concurrent CommandBuffer;

            [NativeDisableParallelForRestriction]
            public NativeArray<Unity.Mathematics.Random> RandomArray;

            [NativeSetThreadIndex]
            int threadIndex;

            public void Execute(Entity entity, int index, [ReadOnly] ref Person person)
            {
                if (ProjectileFromEntity.Exists(entity) && ProjectileFromEntity[entity].HasTarget) return;

                var random = RandomArray[threadIndex];

                CommandBuffer.AddComponent(index, entity, new Projectile
                {
                    AngleInDegrees = random.NextInt(45, 60),
                    Gravity = random.NextInt(50, 150),
                    Target = new float3(
                        random.NextInt(-10, 10),
                        2.5f,
                        random.NextInt(-10, 10)
                    )
                });

                RandomArray[threadIndex] = random;
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

            var addProjectileJob = new AddProjectileJob
            {
                ProjectileFromEntity = GetComponentDataFromEntity<Projectile>(true),
                CommandBuffer = barrier.CreateCommandBuffer().ToConcurrent(),
                RandomArray = World.GetExistingSystem<RandomSystem>().RandomArray
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
