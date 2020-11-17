using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
            {
                Enabled = false;
                return;
            }

            agentTransformGameObject = new GameObject("Agent Transform GameObject");
        }

        protected override void OnUpdate()
        {
            if (Camera.main.transform.parent == null)
                Camera.main.transform.SetParent(agentTransformGameObject.transform);

            if (teleportationText == null)
                teleportationText = GameObject.Find("Text").GetComponent<Text>();

            var keyboard = Keyboard.current;

            if (keyboard == null) return;

            if (keyboard.tKey.wasPressedThisFrame)
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

            var mouse = Mouse.current;

            if (
                mouse == null ||
                !mouse.leftButton.wasPressedThisFrame ||
                !Physics.Raycast(Camera.main.ScreenPointToRay(new Vector2(mouse.position.x.ReadValue(), mouse.position.y.ReadValue())), out RaycastHit hit)
            ) return;

            EntityManager.AddComponentData(agentEntity, new NavNeedsDestination
            {
                Destination = hit.point,
                Teleport = teleport
            });
        }
    }
}
