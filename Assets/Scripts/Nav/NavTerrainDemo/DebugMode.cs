using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Reese.Nav;
using Unity.Collections;
using Unity.Mathematics;

namespace Reese.Demo {

    public class DebugMode : MonoBehaviour
    {
        public static bool IsDebugging = false;

        [SerializeField]
        bool isDebugging = false;

        [SerializeField]
        bool drawnPath = false;

        [SerializeField]
        bool drawUnitVectors = false;

        [SerializeField]
        float3 offset = new float3(-500, 0f, -500);

        EntityManager entityManager;
        EntityQuery entityQuery;

        NavGroundedAgentSystem groundedAgentSystem;

        private void OnEnable()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityQuery = entityManager.CreateEntityQuery(
                new EntityQueryDesc
                {
                    All = new ComponentType[] { typeof(NavLerping), typeof(LocalToParent) },
                    None = new ComponentType[] { typeof(NavPlanning), typeof(NavJumping) }
                });
            groundedAgentSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<NavGroundedAgentSystem>();
        }

        private void OnGUI()
        {
            groundedAgentSystem.IsDebugging = IsDebugging = isDebugging;
            groundedAgentSystem.DrawUnitVectors = drawUnitVectors;
            if (!isDebugging || !drawnPath) return;

            var entityArray = entityQuery.ToEntityArray(Allocator.TempJob);
            foreach (Entity entity in entityArray)
            {
                var pathBuffer = entityManager.GetBuffer<NavPathBufferElement>(entity);
                for (int i = 0; i < pathBuffer.Length - 1; ++i)
                {
                    var node = pathBuffer[i];

                    Debug.DrawLine(node.Value + offset, pathBuffer[i + 1].Value + offset, Color.red);
                }
            }
            entityArray.Dispose();
        }
    }
}