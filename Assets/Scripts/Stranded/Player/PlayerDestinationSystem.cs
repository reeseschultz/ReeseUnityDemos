using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.Physics;
using Unity.Physics.Systems;
using static Reese.Nav.NavSystem;

namespace Reese.Demo.Stranded
{
    [UpdateAfter(typeof(TransformSystemGroup))]
    class PlayerDestinationSystem : SystemBase
    {
        PhysicsWorld physicsWorld => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;
        NavSettings settings => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<NavSystem>().Settings;

        GameObject agentTransformGO = default;

        GameObject cursor = default;
        Renderer cursorRenderer = default;

        Entity playerEntity = default;

        CollisionFilter filter = CollisionFilter.Default;

        protected override void OnCreate()
            => filter = new CollisionFilter
            {
                BelongsTo = Util.ToBitMask(settings.SurfaceLayer),
                CollidesWith = Util.ToBitMask(settings.SurfaceLayer)
            };

        protected override void OnUpdate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("Stranded")) return;

            if (agentTransformGO == null)
            {
                agentTransformGO = GameObject.Find("Player GO");
                if (agentTransformGO == null) return;
            }

            if (cursor == null)
            {
                cursor = GameObject.Find("3D Cursor");
                if (cursor == null) return;
            }

            if (cursorRenderer == null)
            {
                var cursorMesh = cursor.transform.GetChild(0);

                if (cursorMesh == null) return;

                cursorRenderer = cursorMesh.GetComponent<Renderer>();

                if (cursorRenderer == null) return;

                cursorRenderer.enabled = false;
            }

            var keyboard = Keyboard.current;

            if (keyboard == null) return;

            try
            {
                playerEntity = GetSingletonEntity<Player>();
            }
            catch
            {
                return;
            }

            if (playerEntity.Equals(Entity.Null)) return;

            var agentPosition = EntityManager.GetComponentData<LocalToWorld>(playerEntity).Position;

            agentTransformGO.transform.SetPositionAndRotation(agentPosition, Quaternion.identity);

            var mouse = Mouse.current;

            var point = new Vector3(
                mouse.position.x.ReadValue(),
                mouse.position.y.ReadValue()
            );

            var pointOnNavigableSurface = NavUtil.GetPointOnNavigableSurface(
                point,
                playerEntity,
                Camera.main,
                physicsWorld,
                500,
                EntityManager,
                filter,
                out var hit
            );

            if (pointOnNavigableSurface)
            {
                cursorRenderer.enabled = true;

                var cursorPosition = hit.Position;
                cursorPosition.y += 1;

                cursor.transform.position = cursorPosition;

                cursor.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.SurfaceNormal);

                if (mouse != null && mouse.leftButton.isPressed)
                {
                    EntityManager.AddComponentData(playerEntity, new NavDestination
                    {
                        WorldPoint = hit.Position,
                        Tolerance = 1
                    });
                }
            }
            else cursorRenderer.enabled = false;
        }
    }
}
