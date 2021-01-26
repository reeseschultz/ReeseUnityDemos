using Reese.Nav;
using Unity.Entities;
using static Reese.Nav.NavSystem;

namespace Reese.Demo
{
    /// <summary>This is a convenience class to help users easily override the default runtime nav settings. Modify this to prevent losing settings when you update the nav package via UPM. For compile-time constants, see the NavConstants class in the nav package.</summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class NavSettingsOverrides : SystemBase
    {
        NavSystem navSystem = default;

        protected override void OnCreate()
        {
            navSystem = World.GetOrCreateSystem<NavSystem>();

            navSystem.Settings = new NavSettings
            {
                DestinationRateLimitSeconds = 0.8f,
                DestinationSurfaceColliderRadius = 1,
                JumpSecondsMax = 5,
                ObstacleRaycastDistanceMax = 1000,
                SurfaceRaycastDistanceMax = 1000,
                StoppingDistance = 1,
                SurfaceLayer = 28,
                ObstacleLayer = 29,
                ColliderLayer = 30,
                IterationMax = 1000,
                NavMeshQueryNodeMax = 0,
                NeedsSurfaceMapSize = 1000,
                PathSearchMax = 1000,
                SurfaceRaycastMax = 100
            };
        }

        protected override void OnUpdate() { }
    }
}
