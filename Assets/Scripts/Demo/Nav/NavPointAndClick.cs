using System;
using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Demo
{
    class NavPointAndClick : MonoBehaviour
    {
        public Camera cam;
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery agentQuery => entityManager.CreateEntityQuery(typeof(NavAgent));
        GameObject agentTransformGameObject;

        void Start() {
            agentTransformGameObject = new GameObject("Agent Transform GameObject");

            if (cam == null) return;

            cam.transform.SetParent(agentTransformGameObject.transform);
        }

        void LateUpdate()
        {
            if (cam == null) return;

            Entity agentEntity;
            try {
                agentEntity = agentQuery.GetSingletonEntity();
            } catch {
                return;
            }

            if (agentEntity.Equals(Entity.Null)) return;

            var agentPosition = entityManager.GetComponentData<LocalToWorld>(agentEntity).Position;
            agentTransformGameObject.transform.SetPositionAndRotation(agentPosition, Quaternion.identity);
            cam.gameObject.transform.LookAt(agentTransformGameObject.transform, Vector3.up);

            if (
                !Input.GetMouseButtonDown(0) ||
                !Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)
            ) return;

            entityManager.AddComponentData(agentEntity, new NavNeedsDestination{
                Value = hit.point
            });
        }
    }
}
