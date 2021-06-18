using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Reese.Path
{
    public class PathAgentHybrid : MonoBehaviour
    {
        /// <summary>True if the agent is planning, false if not.</summary>
        public bool IsPlanning { get; private set; } = default;

        /// <summary>Has a value of PathQueryStatus if the agent has a problem, null if not.</summary>
        public PathQueryStatus? HasProblem { get; private set; } = default;

        /// <summary>Set if this agent should follow another GameObject with a NavAgentHybrid component.</summary>
        public GameObject FollowTarget { get; set; } = default;

        /// <summary>Maximum distance before this agent will stop following the target Entity. If less than or equal to zero, this agent will follow the target Entity no matter how far it is away.</summary>
        public float FollowMaxDistance { get; set; } = default;

        /// <summary>Minimum distance this agent maintains between itself and the target Entity it follows.</summary>
        public float FollowMinDistance { get; set; } = default;

        /// <summary>Entity representation of this GameObject, which is used by the navigation package.</summary>
        public Entity Entity { get; private set; } = default;

        /// <summary>The agent's type.</summary>
        [SerializeField]
        public string Type = PathConstants.HUMANOID;

        /// <summary>The agent's offset.</summary>
        [SerializeField]
        public Vector3 Offset = default;

        /// <summary>The agent's world destination.</summary>
        [SerializeField]
        public Vector3 WorldDestination = default;

        Vector3 lastWorldDestination = default;

        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        void InitializeEntityTransform()
        {
            entityManager.AddComponentData<LocalToWorld>(Entity, new LocalToWorld
            {
                Value = float4x4.TRS(
                    transform.position,
                    transform.rotation,
                    transform.lossyScale
                )
            });

            if (!entityManager.HasComponent<Rotation>(Entity)) entityManager.AddComponent<Rotation>(Entity);

            if (!entityManager.HasComponent<Translation>(Entity)) entityManager.AddComponent<Translation>(Entity);
        }

        void Start()
        {
            Entity = entityManager.CreateEntity();

            entityManager.AddComponentData(Entity, new PathAgent
            {
                TypeID = PathUtil.GetAgentType(Type),
                Offset = Offset
            });

            InitializeEntityTransform();
        }

        void Update()
        {
            if (Entity.Equals(Entity.Null)) return;

            IsPlanning = entityManager.HasComponent<PathPlanning>(Entity);

            if (entityManager.HasComponent<PathProblem>(Entity)) HasProblem = entityManager.GetComponentData<PathProblem>(Entity).Value;
            else HasProblem = null;
        }

        void FixedUpdate()
        {
            if (
                Entity.Equals(Entity.Null) ||
                !entityManager.HasComponent<PathAgent>(Entity) ||
                !entityManager.HasComponent<Translation>(Entity) ||
                !entityManager.HasComponent<Rotation>(Entity)
            ) return;

            var agent = entityManager.GetComponentData<PathAgent>(Entity);

            agent.TypeID = PathUtil.GetAgentType(Type);
            agent.Offset = Offset;

            entityManager.SetComponentData(Entity, agent);

            if (!lastWorldDestination.Equals(WorldDestination))
            {
                entityManager.AddComponentData(Entity, new PathDestination
                {
                    WorldPoint = WorldDestination
                });

                lastWorldDestination = WorldDestination;

                InitializeEntityTransform(); // Reinitialize in case GameObject transform changes in-between pathing.
            }

            gameObject.transform.position = entityManager.GetComponentData<Translation>(Entity).Value;
            gameObject.transform.rotation = entityManager.GetComponentData<Rotation>(Entity).Value;

            if (FollowTarget != null && FollowTarget.GetComponent<PathAgentHybrid>() != null)
            {
                entityManager.AddComponentData(Entity, new PathFollow
                {
                    Target = FollowTarget.GetComponent<PathAgentHybrid>().Entity,
                    MaxDistance = FollowMaxDistance,
                    MinDistance = FollowMinDistance,
                });
            }
            else if (entityManager.HasComponent<PathFollow>(Entity)) entityManager.RemoveComponent<PathFollow>(Entity);
        }
    }
}
