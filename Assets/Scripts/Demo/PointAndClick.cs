using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Entities;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;
using Unity.Rendering;
using Reese.Spawning;
using Unity.Transforms;
using Unity.Mathematics;

namespace Reese.Demo
{
    class PointAndClick : MonoBehaviour
    {
        public Camera Cam;
        const float RAYCAST_DISTANCE = 1000;
        PhysicsWorld physicsWorld => World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        void Start()
        {
            var prefabEntity = entityManager.CreateEntityQuery(typeof(PersonPrefab)).GetSingleton<PersonPrefab>().Value;

            SpawnSystem.Enqueue(new Spawn()
                .WithPrefab(prefabEntity)
                .WithComponentList(
                    new Translation
                    {
                        Value = new float3(0, 1, 0)
                    }
                )
            );

            SpawnSystem.Enqueue(new Spawn()
                .WithPrefab(prefabEntity)
                .WithComponentList(
                    new Translation
                    {
                        Value = new float3(5, 1, 0)
                    }
                )
            );

            SpawnSystem.Enqueue(new Spawn()
                .WithPrefab(prefabEntity)
                .WithComponentList(
                    new Translation
                    {
                        Value = new float3(-5, 1, 0)
                    }
                )
            );
        }

        void LateUpdate()
        {
            if (!Input.GetMouseButtonDown(0) || Cam == null) return;

            var screenPointToRay = Cam.ScreenPointToRay(Input.mousePosition);
            var rayInput = new RaycastInput
            {
                Start = screenPointToRay.origin,
                End = screenPointToRay.GetPoint(RAYCAST_DISTANCE),
                Filter = CollisionFilter.Default
            };

            if (!physicsWorld.CastRay(rayInput, out RaycastHit hit)) return;

            var selectedEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;
            var renderMesh = entityManager.GetSharedComponentData<RenderMesh>(selectedEntity);
            var mat = new UnityEngine.Material(renderMesh.material);
            mat.SetColor("_Color", UnityEngine.Random.ColorHSV());
            renderMesh.material = mat;

            entityManager.SetSharedComponentData(selectedEntity, renderMesh);
        }
    }
}
