using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Nav
{
    public class NavAgentHybrid : MonoBehaviour
    {
        [SerializeField]
        public float JumpDegrees = 45;

        [SerializeField]
        public float JumpGravity = 100;

        [SerializeField]
        public float JumpSpeedMultiplierX = 1.5f;

        [SerializeField]
        public float JumpSpeedMultiplierY = 2;

        [SerializeField]
        public float TranslationSpeed = 20;

        [SerializeField]
        public float RotationSpeed = 0.3f;

        [SerializeField]
        public string Type = NavConstants.HUMANOID;

        [SerializeField]
        public Vector3 Offset = default;

        [SerializeField]
        public bool Teleport = default;

        [SerializeField]
        public Vector3 WorldDestination = default;

        Vector3 lastWorldDestination = default;

        Entity entity = default;
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        NavSurfaceSystem surfaceSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<NavSurfaceSystem>();

        void InitializeEntityTransform()
        {
            entityManager.AddComponentData<LocalToWorld>(entity, new LocalToWorld
            {
                Value = float4x4.TRS(
                    transform.position,
                    transform.rotation,
                    transform.lossyScale
                )
            });

            entityManager.AddComponentData<Translation>(entity, new Translation
            {
                Value = transform.position 
            });

            entityManager.AddComponent<Rotation>(entity);
            entityManager.AddComponent<Parent>(entity);
            entityManager.AddComponent<LocalToParent>(entity);
            entityManager.AddComponent<NavNeedsSurface>(entity);
        }

        void Start()
        {
            entity = entityManager.CreateEntity();

            entityManager.AddComponentData(entity, new NavAgent
            {
                JumpDegrees = JumpDegrees,
                JumpGravity = JumpGravity,
                JumpSpeedMultiplierX = JumpSpeedMultiplierX,
                JumpSpeedMultiplierY = JumpSpeedMultiplierY,
                TranslationSpeed = TranslationSpeed,
                RotationSpeed = RotationSpeed,
                TypeID = NavUtil.GetAgentType(Type),
                Offset = Offset
            });

            InitializeEntityTransform();
        }

        void FixedUpdate()
        {
            if (
                !entityManager.HasComponent<NavAgent>(entity) ||
                !entityManager.HasComponent<Parent>(entity) ||
                !entityManager.HasComponent<Translation>(entity) ||
                !entityManager.HasComponent<Rotation>(entity)
            ) return;

            var navAgent = entityManager.GetComponentData<NavAgent>(entity);

            navAgent.JumpDegrees = JumpDegrees;
            navAgent.JumpGravity = JumpGravity;
            navAgent.JumpSpeedMultiplierX = JumpSpeedMultiplierX;
            navAgent.JumpSpeedMultiplierY = JumpSpeedMultiplierY;
            navAgent.TranslationSpeed = TranslationSpeed;
            navAgent.RotationSpeed = RotationSpeed;
            navAgent.TypeID = NavUtil.GetAgentType(Type);
            navAgent.Offset = Offset;

            entityManager.SetComponentData(entity, navAgent);

            if (!lastWorldDestination.Equals(WorldDestination))
            {
                entityManager.AddComponentData<NavNeedsDestination>(entity, new NavNeedsDestination
                {
                    Destination = WorldDestination,
                    Teleport = Teleport
                });

                lastWorldDestination = WorldDestination;

                InitializeEntityTransform(); // Reinitialize in case GameObject transform changes in-between pathing.
            }

            var surfaceEntity = entityManager.GetComponentData<Parent>(entity);

            if (surfaceEntity.Value.Equals(Entity.Null) || !entityManager.HasComponent<NavSurface>(surfaceEntity.Value)) return;

            surfaceSystem.GameObjectMapTryGetValue(
                entityManager.GetComponentData<NavSurface>(surfaceEntity.Value).TransformInstanceID,
                out var surfaceGameObject
            );

            if (surfaceGameObject == null) return;

            gameObject.transform.SetParent(surfaceGameObject.transform);
            gameObject.transform.localPosition = entityManager.GetComponentData<Translation>(entity).Value / surfaceGameObject.transform.localScale;
            gameObject.transform.localRotation = entityManager.GetComponentData<Rotation>(entity).Value;
        }
    }
}
