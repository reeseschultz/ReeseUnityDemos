using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Reese.Demo
{
    [UpdateAfter(typeof(TransformSystemGroup))]
    class StrandedDestinationSystem : SystemBase
    {
        PhysicsWorld physicsWorld => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>().PhysicsWorld;

        GameObject agentTransformGO = default;

        GameObject cursor = default;
        Renderer cursorRenderer = default;

        Entity agentEntity = default;

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
                agentEntity = GetSingletonEntity<NavAgent>();
            }
            catch
            {
                return;
            }

            if (agentEntity.Equals(Entity.Null)) return;

            var agentPosition = EntityManager.GetComponentData<LocalToWorld>(agentEntity).Position;

            agentTransformGO.transform.SetPositionAndRotation(agentPosition, Quaternion.identity);

            var mouse = Mouse.current;

            var point = new Vector3(
                mouse.position.x.ReadValue(),
                mouse.position.y.ReadValue()
            );

            var pointOnNavigableSurface = NavUtil.GetPointOnNavigableSurface(
                point,
                agentEntity,
                Camera.main,
                physicsWorld,
                500,
                EntityManager,
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
                    EntityManager.AddComponentData(agentEntity, new NavNeedsDestination
                    {
                        Destination = hit.Position,
                        Tolerance = 1
                    });
                }
            }
            else cursorRenderer.enabled = false;
        }
    }
}
