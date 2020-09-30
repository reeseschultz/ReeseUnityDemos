using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Reese.Nav;
using Unity.Collections;
using Unity.Mathematics;

namespace Reese.Demo {

    public class DebugMode : MonoBehaviour
    {
        [SerializeField]
        bool isDebugging = false;

        [SerializeField]
        bool drawUnitVectors = false;

        [SerializeField]
        bool logNavError = false;

        [SerializeField]
        bool drawnPath = false;

        [SerializeField]
        float3 offsetForDrawPath = new float3(-500, 0f, -500);

        EntityManager entityManager;
        EntityQuery entityQuery;

        NavGroundingSystem navGroundingSystem;
        DebugSystems debugSystems;

        private void OnEnable()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityQuery = entityManager.CreateEntityQuery(
                new EntityQueryDesc
                {
                    All = new ComponentType[] { typeof(NavLerping), typeof(LocalToParent) },
                    None = new ComponentType[] { typeof(NavPlanning), typeof(NavJumping) }
                });
            navGroundingSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<NavGroundingSystem>();
            debugSystems = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<DebugSystems>();
            debugSystems.DrawPathOffset = offsetForDrawPath;
        }

        private void Update()
        {
            navGroundingSystem.IsDebugging = isDebugging && drawUnitVectors;
            debugSystems.LogNavError = isDebugging && logNavError;
            debugSystems.DrawPath = isDebugging && drawnPath;
        }
    }

    public class DebugSystems : SystemBase
    {
        public bool LogNavError = false;
        public bool DrawPath = false;
        public float3 DrawPathOffset = default;

        protected override void OnUpdate()
        {
            if (LogNavError)
            {
                Entities.ForEach((Entity entity, in NavHasProblem navHasProblem) =>
                {
                    Debug.Log(string.Format("{0} has a nav problem! {1}", entity, navHasProblem.Value));
                }).Run();
            }

            if (DrawPath)
            {
                float3 drawPathOffset = DrawPathOffset;
                Entities.ForEach((Entity entity, in DynamicBuffer<NavPathBufferElement> pathBuffer) =>
                {

                    for (int i = 0; i < pathBuffer.Length - 1; ++i)
                    {
                        var node = pathBuffer[i];

                        Debug.DrawLine(node.Value + drawPathOffset, pathBuffer[i + 1].Value + drawPathOffset, Color.red);
                    }
                }).Run();
            }

        }
    }
}