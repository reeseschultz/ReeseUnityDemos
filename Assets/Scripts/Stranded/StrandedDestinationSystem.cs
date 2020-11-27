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

        public GameObject AgentTransformGameObject { get; private set; }

        GameObject cursor = default;
        Renderer cursorRenderer = default;

        Entity agentEntity;

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("Stranded"))
            {
                Enabled = false;
                return;
            }

            AgentTransformGameObject = new GameObject("Agent Transform GameObject");

            cursor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            cursor.name = "3D Cursor";
            cursorRenderer = cursor.GetComponent<Renderer>();
            cursorRenderer.enabled = false;
            cursorRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        protected override void OnUpdate()
        {
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

            AgentTransformGameObject.transform.SetPositionAndRotation(agentPosition, Quaternion.identity);

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
