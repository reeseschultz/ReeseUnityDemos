using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Entities;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using Unity.Collections;

namespace Reese.Demo
{
    class PointAndClick : MonoBehaviour
    {
        [SerializeField]
        Camera Cam = default;

        const float RAYCAST_DISTANCE = 1000;
        PhysicsWorld physicsWorld => World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        void Start()
        {
            var outputEntities = new NativeArray<Entity>(3, Allocator.Temp);
            var prefabEntity = entityManager.CreateEntityQuery(typeof(PersonPrefab)).GetSingleton<PersonPrefab>().Value;

            entityManager.Instantiate(prefabEntity, outputEntities);

            entityManager.AddComponentData(outputEntities[0], new Translation
            {
                Value = new float3(0, 1, 0)
            });

            entityManager.AddComponentData(outputEntities[1], new Translation
            {
                Value = new float3(5, 1, 0)
            });

            entityManager.AddComponentData(outputEntities[2], new Translation
            {
                Value = new float3(-5, 1, 0)
            });

            outputEntities.Dispose();
        }

        void LateUpdate()
        {
            var mouse = Mouse.current;

            if (Cam == null || mouse == null || !mouse.leftButton.wasPressedThisFrame) return;

            var position = new Vector3(mouse.position.x.ReadValue(), mouse.position.y.ReadValue());
            var screenPointToRay = Cam.ScreenPointToRay(position);
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
            mat.SetColor("_BaseColor", UnityEngine.Random.ColorHSV());
            renderMesh.material = mat;

            entityManager.SetSharedComponentData(selectedEntity, renderMesh);
        }
    }
}
