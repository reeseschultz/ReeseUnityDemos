using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    [UpdateAfter(typeof(TransformSystemGroup))]
    class NavPointAndClickDestinationSystem : SystemBase
    {
        bool teleport = false;

        Entity agentEntity;
        GameObject agentTransformGameObject;
        Text teleportationText;

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("NavPointAndClickDemo"))
                Enabled = false;

            agentTransformGameObject = new GameObject("Agent Transform GameObject");
        }

        protected override void OnUpdate()
        {
            if (Camera.main.transform.parent == null)
                Camera.main.transform.SetParent(agentTransformGameObject.transform);

            if (teleportationText == null)
                teleportationText = GameObject.Find("Text").GetComponent<Text>();

            if (Input.GetKeyDown(KeyCode.T))
            {
                teleport = !teleport;

                if (teleport) teleportationText.text = "Press <b>T</b> to toggle teleportation. It's <b>on</b>.";
                else teleportationText.text = "Press <b>T</b> to toggle teleportation. It's <b>off</b>.";
            }

            try
            {
                agentEntity = GetSingletonEntity<NavAgent>();
            }
            catch
            {
                return;
            }

            if (agentEntity.Equals(Entity.Null)) return;

            var agentPosition = EntityManager.GetComponentData<LocalToWorld>(agentEntity).Position;
            agentTransformGameObject.transform.SetPositionAndRotation(agentPosition, Quaternion.identity);
            Camera.main.transform.LookAt(agentTransformGameObject.transform, Vector3.up);

            if (
                !Input.GetMouseButtonDown(0) ||
                !Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)
            ) return;

            EntityManager.AddComponentData(agentEntity, new NavNeedsDestination
            {
                Destination = hit.point,
                Teleport = teleport
            });
        }
    }
}
