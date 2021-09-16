using Reese.EntityPrefabGroups;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Reese.Demo
{
    class PointAndClick : MonoBehaviour
    {
        [SerializeField]
        Camera Cam = default;

        const float RAYCAST_DISTANCE = 1000;

        PhysicsWorld physicsWorld => World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        EntityPrefabSystem prefabSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EntityPrefabSystem>();

        void Start()
        {
            var prefab = prefabSystem.GetPrefab(typeof(Person));

            var entities = new NativeArray<Entity>(3, Allocator.Temp);
            prefabSystem.EntityManager.Instantiate(prefab, entities);

            prefabSystem.EntityManager.AddComponentData(entities[0], new Translation
            {
                Value = new float3(0, 1, 0)
            });

            prefabSystem.EntityManager.AddComponentData(entities[1], new Translation
            {
                Value = new float3(5, 1, 0)
            });

            prefabSystem.EntityManager.AddComponentData(entities[2], new Translation
            {
                Value = new float3(-5, 1, 0)
            });

            entities.Dispose();
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
            var renderMesh = prefabSystem.EntityManager.GetSharedComponentData<RenderMesh>(selectedEntity);
            var mat = new UnityEngine.Material(renderMesh.material);
            mat.SetColor("_BaseColor", UnityEngine.Random.ColorHSV());
            renderMesh.material = mat;

            prefabSystem.EntityManager.SetSharedComponentData(selectedEntity, renderMesh);
        }
    }
}
