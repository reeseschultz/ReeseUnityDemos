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

        Entity agentEntity;

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("Stranded"))
            {
                Enabled = false;
                return;
            }

            AgentTransformGameObject = new GameObject("Agent Transform GameObject");
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

            var screenPointToRay = Camera.main.ScreenPointToRay(
                new Vector3(
                    mouse.position.x.ReadValue(),
                    mouse.position.y.ReadValue()
                )
            );

            var rayInput = new RaycastInput
            {
                Start = screenPointToRay.origin,
                End = screenPointToRay.GetPoint(500),
                Filter = CollisionFilter.Default
            };

            if (
                mouse == null ||
                !mouse.leftButton.isPressed ||
                !physicsWorld.CastRay(rayInput, out var hit)
            ) return;

            if (hit.RigidBodyIndex == -1) return;

            var hitSurfaceEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;

            if (hitSurfaceEntity == Entity.Null) return;

            if (!EntityManager.HasComponent<Parent>(agentEntity)) return;

            var surfaceEntity = EntityManager.GetComponentData<Parent>(agentEntity).Value;

            if (surfaceEntity == Entity.Null) return;

            if (surfaceEntity == hitSurfaceEntity)
            {
                EntityManager.AddComponentData(agentEntity, new NavNeedsDestination
                {
                    Destination = hit.Position,
                    Tolerance = 1
                });

                return;
            }

            if (!EntityManager.HasComponent<NavJumpableBufferElement>(surfaceEntity)) return;

            var jumpableSurfaces = EntityManager.GetBuffer<NavJumpableBufferElement>(surfaceEntity);

            for (var i = 0; i < jumpableSurfaces.Length; ++i)
            {
                if (hitSurfaceEntity == jumpableSurfaces[i])
                {
                    EntityManager.AddComponentData(agentEntity, new NavNeedsDestination
                    {
                        Destination = hit.Position,
                        Tolerance = 1
                    });

                    break;
                }
            }
        }
    }
}
