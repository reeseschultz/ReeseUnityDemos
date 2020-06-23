using System;
using System.Text.RegularExpressions;
using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

namespace Reese.Demo
{
    class NavPointAndClick : MonoBehaviour
    {
        public Camera Cam = null;
        public Text TeleportationText = null;

        bool teleport;
        EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery agentQuery => entityManager.CreateEntityQuery(typeof(NavAgent));
        GameObject agentTransformGameObject;

        void Start()
        {
            agentTransformGameObject = new GameObject("Agent Transform GameObject");

            if (Cam == null) return;

            Cam.transform.SetParent(agentTransformGameObject.transform);
        }

        void LateUpdate()
        {
            if (Cam == null || TeleportationText == null) return;

            if (Input.GetKeyDown(KeyCode.T))
            {
                teleport = !teleport;

                if (teleport) TeleportationText.text = "Press <b>T</b> to toggle teleportation. It's <b>on</b>.";
                else TeleportationText.text = "Press <b>T</b> to toggle teleportation. It's <b>off</b>.";
            }

            Entity agentEntity;
            try
            {
                agentEntity = agentQuery.GetSingletonEntity();
            }
            catch
            {
                return;
            }

            if (agentEntity.Equals(Entity.Null)) return;

            var agentPosition = entityManager.GetComponentData<LocalToWorld>(agentEntity).Position;
            agentTransformGameObject.transform.SetPositionAndRotation(agentPosition, Quaternion.identity);
            Cam.gameObject.transform.LookAt(agentTransformGameObject.transform, Vector3.up);

            if (
                !Input.GetMouseButtonDown(0) ||
                !Physics.Raycast(Cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)
            ) return;

            entityManager.AddComponentData(agentEntity, new NavNeedsDestination
            {
                Destination = hit.point,
                Teleport = teleport
            });
        }
    }
}
