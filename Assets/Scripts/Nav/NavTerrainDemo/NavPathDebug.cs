using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Reese.Nav;
using Unity.Collections;
using Unity.Mathematics;

namespace Reese.Demo {

    public class NavPathDebug : MonoBehaviour
    {
        float3 offset = new float3(-500, 0f, -500);

        private void OnGUI()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityQuery entityQuery = entityManager.CreateEntityQuery(
                new EntityQueryDesc
                {
                    All = new ComponentType[] { typeof(NavLerping), typeof(LocalToParent) },
                    None = new ComponentType[] { typeof(NavPlanning), typeof(NavJumping) }
                });

            var entityArray = entityQuery.ToEntityArray(Allocator.TempJob);

            //Debug.Log("entityArray.Length " + entityArray.Length);

            foreach (Entity entity in entityArray)
            {
                var pathBuffer = entityManager.GetBuffer<NavPathBufferElement>(entity);
                //Debug.Log("pathBuffer.Length " + pathBuffer.Length);

                for (int i = 0; i < pathBuffer.Length - 1; ++i)
                {
                    var node = pathBuffer[i];

                    Debug.DrawLine(node.Value + offset, pathBuffer[i + 1].Value + offset, Color.red);
                    //Debug.Log("node.Value " + node.Value);
                }
            }
            entityArray.Dispose();
        }
    }
}