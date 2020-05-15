using Reese.Nav;
using Unity.Entities;
using UnityEngine;

namespace Reese.Demo
{
    class NavPointAndClick : MonoBehaviour
    {
        public Camera cam;
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery agentQuery => entityManager.CreateEntityQuery(typeof(NavAgent));

        void Update()
        {
            if (
                cam == null ||
                !Input.GetMouseButtonDown(0) ||
                !Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)
            ) return;

            entityManager.AddComponentData(agentQuery.GetSingletonEntity(), new NavNeedsDestination{
                Value = hit.point
            });

            entityManager.AddComponent<NavPlanning>(agentQuery.GetSingletonEntity());
        }
    }
}
