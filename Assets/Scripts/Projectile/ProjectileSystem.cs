using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    class ProjectileSystem : JobComponentSystem
    {
        static Scene scene => SceneManager.GetActiveScene();

        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!scene.name.Equals("ProjectileDemo")) return inputDeps;

            var projectileFromEntity = GetComponentDataFromEntity<Projectile>(true);
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;
            var commandBuffer = barrier.CreateCommandBuffer().ToConcurrent();

            var addProjectileJob = Entities
                .WithReadOnly(projectileFromEntity)
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, ref Person person) =>
                {
                    if (
                        projectileFromEntity.Exists(entity) &&
                        projectileFromEntity[entity].HasTarget
                    ) return;

                    var random = randomArray[nativeThreadIndex];

                    commandBuffer.AddComponent(entityInQueryIndex, entity, new Projectile
                    {
                        AngleInDegrees = random.NextInt(45, 60),
                        Gravity = random.NextInt(50, 150),
                        Target = new float3(
                            random.NextInt(-10, 10),
                            2.5f,
                            random.NextInt(-10, 10)
                        )
                    });

                    randomArray[nativeThreadIndex] = random;
                })
                .WithName("AddProjectileJob")
                .Schedule(inputDeps);

            barrier.AddJobHandleForProducer(addProjectileJob);
            addProjectileJob.Complete();

            var deltaSeconds = Time.DeltaTime;

            var launchJob = Entities
                .ForEach((ref Projectile projectile, ref Translation translation) =>
                {
                    if (!projectile.HasTarget) return;

                    var velocity = Vector3.Distance(translation.Value, projectile.Target) / (math.sin(2 * math.radians(projectile.AngleInDegrees)) / projectile.Gravity);
                    var xVelocity = math.sqrt(velocity) * math.cos(math.radians(projectile.AngleInDegrees));
                    var yVelocity = math.sqrt(velocity) * math.sin(math.radians(projectile.AngleInDegrees));

                    translation.Value.y += (yVelocity - projectile.FlightDurationInSeconds * projectile.Gravity) * deltaSeconds;
                    translation.Value = Vector3.MoveTowards(translation.Value, projectile.Target, xVelocity * deltaSeconds);

                    projectile.FlightDurationInSeconds += deltaSeconds;

                    if (translation.Value.y >= projectile.Target.y) return;

                    projectile.FlightDurationInSeconds = 0;
                    projectile.HasTarget = false;
                })
                .WithName("LaunchJob")
                .Schedule(inputDeps);

            return JobHandle.CombineDependencies(addProjectileJob, launchJob);
        }
    }
}
