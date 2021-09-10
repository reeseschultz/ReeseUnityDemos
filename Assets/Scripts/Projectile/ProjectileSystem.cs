using Reese.Random;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.SceneManagement;
using Reese.Math;

namespace Reese.Demo
{
    class ProjectileSystem : SystemBase
    {
        EntityCommandBufferSystem barrier => World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        protected override void OnUpdate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("ProjectileDemo")) return;

            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;
            var commandBuffer = barrier.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithNativeDisableParallelForRestriction(randomArray)
                .ForEach((Entity entity, int entityInQueryIndex, int nativeThreadIndex, in Projectile projectile) =>
                {
                    if (projectile.HasTarget) return;

                    var random = randomArray[nativeThreadIndex];

                    commandBuffer.SetComponent(entityInQueryIndex, entity, new Projectile
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
                .WithName("SetProjectileJob")
                .ScheduleParallel();

            barrier.AddJobHandleForProducer(Dependency);

            var deltaSeconds = Time.DeltaTime;

            Entities
                .ForEach((ref Projectile projectile, ref Translation translation) =>
                {
                    if (!projectile.HasTarget) return;

                    var velocity = math.distance(translation.Value, projectile.Target) / (math.sin(2 * math.radians(projectile.AngleInDegrees)) / projectile.Gravity);
                    var xVelocity = math.sqrt(velocity) * math.cos(math.radians(projectile.AngleInDegrees));
                    var yVelocity = math.sqrt(velocity) * math.sin(math.radians(projectile.AngleInDegrees));

                    translation.Value.y += (yVelocity - projectile.FlightDurationInSeconds * projectile.Gravity) * deltaSeconds;
                    translation.Value.MoveTowards(projectile.Target, xVelocity * deltaSeconds);

                    projectile.FlightDurationInSeconds += deltaSeconds;

                    if (translation.Value.y >= projectile.Target.y) return;

                    projectile.FlightDurationInSeconds = 0;
                    projectile.HasTarget = false;
                })
                .WithName("LaunchJob")
                .ScheduleParallel();
        }
    }
}
