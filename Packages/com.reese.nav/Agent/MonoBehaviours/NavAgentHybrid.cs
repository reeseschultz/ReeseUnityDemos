using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Reese.Nav
{
    public class NavAgentHybrid : MonoBehaviour
    {
        /// <summary>True if the agent is walking, false if not.</summary>
        public bool IsWalking { get; private set; } = default;

        /// <summary>True if the agent is jumping, false if not.</summary>
        public bool IsJumping { get; private set; } = default;

        /// <summary>True if the agent is falling, false if not.</summary>
        public bool IsFalling { get; private set; } = default;

        /// <summary>True if the agent is planning, false if not.</summary>
        public bool IsPlanning { get; private set; } = default;

        /// <summary>True if the agent is terrain-capable, false if not.</summary>
        public bool IsTerrainCapable { get; set; } = default;

        /// <summary>True if the agent should flock, false if not.</summary>
        public bool ShouldFlock { get; set; } = default;

        /// <summary>Has a value of PathQueryStatus if the agent has a problem, null if not.</summary>
        public PathQueryStatus? HasProblem { get; private set; } = default;

        /// <summary>True if the agent needs a surface, false if not.</summary>
        public bool NeedsSurface { get; private set; } = default;

        /// <summary>Set if this agent should follow another GameObject with a NavAgentHybrid component.</summary>
        public GameObject FollowTarget { get; set; } = default;

        /// <summary>Maximum distance before this agent will stop following the target Entity. If less than or equal to zero, this agent will follow the target Entity no matter how far it is away.</summary>
        public float FollowMaxDistance { get; set; } = default;

        /// <summary>Minimum distance this agent maintains between itself and the target Entity it follows.</summary>
        public float FollowMinDistance { get; set; } = default;

        /// <summary>Entity representation of this GameObject, which is used by the navigation package.</summary>
        public Entity Entity { get; private set; } = default;

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

        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        NavSurfaceSystem surfaceSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<NavSurfaceSystem>();

        /// <summary>Stops the agent from navigating (waits for jumping or falling to complete).</summary>
        public void Stop()
            => entityManager.AddComponent<NavStop>(Entity);

        void InitializeEntityTransform()
        {
            entityManager.AddComponentData(Entity, new LocalToWorld
            {
                Value = float4x4.TRS(
                    transform.position,
                    transform.rotation,
                    transform.lossyScale
                )
            });

            if (!entityManager.HasComponent<Rotation>(Entity)) entityManager.AddComponent<Rotation>(Entity);

            if (!entityManager.HasComponent<Parent>(Entity))
            {
                entityManager.AddComponent<Parent>(Entity);
                entityManager.AddComponent<LocalToParent>(Entity);
            }

            if (!entityManager.HasComponent<Translation>(Entity)) entityManager.AddComponent<Translation>(Entity);

            entityManager.AddComponent<NavNeedsSurface>(Entity);
        }

        void Start()
        {
            Entity = entityManager.CreateEntity();

            entityManager.AddComponentData(Entity, new NavAgent
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

            if (ShouldFlock) entityManager.AddComponent<NavFlocking>(Entity);

            InitializeEntityTransform();
        }

        void Update()
        {
            if (Entity.Equals(Entity.Null)) return;

            IsWalking = entityManager.HasComponent<NavWalking>(Entity);
            IsJumping = entityManager.HasComponent<NavJumping>(Entity);
            IsFalling = entityManager.HasComponent<NavFalling>(Entity);
            IsPlanning = entityManager.HasComponent<NavPlanning>(Entity);

            NeedsSurface = entityManager.HasComponent<NavNeedsSurface>(Entity);

            if (IsTerrainCapable && !entityManager.HasComponent<NavTerrainCapable>(Entity)) entityManager.AddComponent<NavTerrainCapable>(Entity);
            else entityManager.RemoveComponent<NavTerrainCapable>(Entity);

            if (entityManager.HasComponent<NavProblem>(Entity)) HasProblem = entityManager.GetComponentData<NavProblem>(Entity).Value;
            else HasProblem = null;
        }

        void FixedUpdate()
        {
            if (
                Entity.Equals(Entity.Null) ||
                !entityManager.HasComponent<NavAgent>(Entity) ||
                !entityManager.HasComponent<Parent>(Entity) ||
                !entityManager.HasComponent<Translation>(Entity) ||
                !entityManager.HasComponent<Rotation>(Entity)
            ) return;

            var agent = entityManager.GetComponentData<NavAgent>(Entity);

            agent.JumpDegrees = JumpDegrees;
            agent.JumpGravity = JumpGravity;
            agent.JumpSpeedMultiplierX = JumpSpeedMultiplierX;
            agent.JumpSpeedMultiplierY = JumpSpeedMultiplierY;
            agent.TranslationSpeed = TranslationSpeed;
            agent.RotationSpeed = RotationSpeed;
            agent.TypeID = NavUtil.GetAgentType(Type);
            agent.Offset = Offset;

            entityManager.SetComponentData(Entity, agent);

            if (!lastWorldDestination.Equals(WorldDestination))
            {
                entityManager.AddComponentData<NavDestination>(Entity, new NavDestination
                {
                    WorldPoint = WorldDestination,
                    Teleport = Teleport
                });

                lastWorldDestination = WorldDestination;

                InitializeEntityTransform(); // Reinitialize in case GameObject transform changes in-between pathing.
            }

            var surfaceEntity = entityManager.GetComponentData<Parent>(Entity);

            if (surfaceEntity.Value.Equals(Entity.Null) || !entityManager.HasComponent<NavSurface>(surfaceEntity.Value)) return;

            surfaceSystem.GameObjectMapTryGetValue(
                entityManager.GetComponentData<NavSurface>(surfaceEntity.Value).TransformInstanceID,
                out var surfaceGameObject
            );

            if (surfaceGameObject == null) return;

            gameObject.transform.SetParent(surfaceGameObject.transform);
            gameObject.transform.localPosition = entityManager.GetComponentData<Translation>(Entity).Value / surfaceGameObject.transform.localScale;
            gameObject.transform.localRotation = entityManager.GetComponentData<Rotation>(Entity).Value;

            if (FollowTarget != null && FollowTarget.GetComponent<NavAgentHybrid>() != null)
            {
                entityManager.AddComponentData(Entity, new NavFollow
                {
                    Target = FollowTarget.GetComponent<NavAgentHybrid>().Entity,
                    MaxDistance = FollowMaxDistance,
                    MinDistance = FollowMinDistance,
                });
            }
            else if (entityManager.HasComponent<NavFollow>(Entity))
            {
                entityManager.RemoveComponent<NavFollow>(Entity);
            }
        }
    }
}
