using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Reese.Nav
{
    public class NavAgentHybrid : MonoBehaviour
    {
        /// <summary>True if the agent is lerping, false if not.</summary>
        public bool IsLerping { get; private set; }

        /// <summary>True if the agent is jumping, false if not.</summary>
        public bool IsJumping { get; private set; }

        /// <summary>True if the agent is falling, false if not.</summary>
        public bool IsFalling { get; private set; }

        /// <summary>True if the agent is planning, false if not.</summary>
        public bool IsPlanning { get; private set; }

        /// <summary>True if the agent is terrain-capable, false if not.</summary>
        public bool IsTerrainCapable { get; set; }

        /// <summary>Has a value of PathQueryStatus if the agent has a problem, null if not.</summary>
        public PathQueryStatus? HasProblem { get; private set; }

        /// <summary>True if the agent needs a surface, false if not.</summary>
        public bool NeedsSurface { get; private set; }

        /// <summary>The agent's jump angle in degrees.</summary>
        [SerializeField]
        public float JumpDegrees = 45;

        /// <summary>Artificial gravity applied to the agent.</summary>
        [SerializeField]
        public float JumpGravity = 100;

        /// <summary>The agent's horizontal jump speed multiplier.</summary>
        [SerializeField]
        public float JumpSpeedMultiplierX = 1.5f;

        /// <summary>The agent's vertical jump speed mulitiplier.</summary>
        [SerializeField]
        public float JumpSpeedMultiplierY = 2;

        /// <summary>The agent's translation speed.</summary>
        [SerializeField]
        public float TranslationSpeed = 20;

        /// <summary>The agent's rotation speed.</summary>
        [SerializeField]
        public float RotationSpeed = 0.3f;

        /// <summary>The agent's type.</summary>
        [SerializeField]
        public string Type = NavConstants.HUMANOID;

        /// <summary>The agent's offset.</summary>
        [SerializeField]
        public Vector3 Offset = default;

        /// <summary>True if the agent should teleport to destinations, false if not.</summary>
        [SerializeField]
        public bool Teleport = default;

        /// <summary>The agent's world destination.</summary>
        [SerializeField]
        public Vector3 WorldDestination = default;

        Vector3 lastWorldDestination = default;

        Entity entity = default;
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        NavSurfaceSystem surfaceSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<NavSurfaceSystem>();

        /// <summary>Stops the agent from navigating (waits for jumping or falling to complete).</summary>
        public void Stop()
            => entityManager.AddComponent<NavStop>(entity);

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

            if (!entityManager.HasComponent<Rotation>(entity)) entityManager.AddComponent<Rotation>(entity);

            if (!entityManager.HasComponent<Parent>(entity))
            {
                entityManager.AddComponent<Parent>(entity);
                entityManager.AddComponent<LocalToParent>(entity);
            }

            if (!entityManager.HasComponent<Translation>(entity)) entityManager.AddComponent<Translation>(entity);

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

        void Update()
        {
            if (entity.Equals(Entity.Null)) return;

            IsLerping = entityManager.HasComponent<NavLerping>(entity);
            IsJumping = entityManager.HasComponent<NavJumping>(entity);
            IsFalling = entityManager.HasComponent<NavFalling>(entity);
            IsPlanning = entityManager.HasComponent<NavPlanning>(entity);

            NeedsSurface = entityManager.HasComponent<NavNeedsSurface>(entity);

            if (IsTerrainCapable && !entityManager.HasComponent<NavTerrainCapable>(entity)) entityManager.AddComponent<NavTerrainCapable>(entity);
            else entityManager.RemoveComponent<NavTerrainCapable>(entity);

            if (entityManager.HasComponent<NavHasProblem>(entity)) HasProblem = entityManager.GetComponentData<NavHasProblem>(entity).Value;
            else HasProblem = null;
        }

        void FixedUpdate()
        {
            if (
                entity.Equals(Entity.Null) ||
                !entityManager.HasComponent<NavAgent>(entity) ||
                !entityManager.HasComponent<Parent>(entity) ||
                !entityManager.HasComponent<Translation>(entity) ||
                !entityManager.HasComponent<Rotation>(entity)
            ) return;

            var agent = entityManager.GetComponentData<NavAgent>(entity);

            agent.JumpDegrees = JumpDegrees;
            agent.JumpGravity = JumpGravity;
            agent.JumpSpeedMultiplierX = JumpSpeedMultiplierX;
            agent.JumpSpeedMultiplierY = JumpSpeedMultiplierY;
            agent.TranslationSpeed = TranslationSpeed;
            agent.RotationSpeed = RotationSpeed;
            agent.TypeID = NavUtil.GetAgentType(Type);
            agent.Offset = Offset;

            entityManager.SetComponentData(entity, agent);

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
