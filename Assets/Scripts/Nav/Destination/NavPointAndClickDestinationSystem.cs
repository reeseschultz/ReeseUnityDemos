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
    partial class NavPointAndClickDestinationSystem : SystemBase
    {
        bool teleport = false;

        Entity entity = default;
        GameObject agentTransformGameObject = default;
        Text teleportationText = default;

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
                entity = GetSingletonEntity<NavAgent>();
            }
            catch
            {
                return;
            }

            if (entity.Equals(Entity.Null)) return;

            var agentPosition = EntityManager.GetComponentData<LocalToWorld>(entity).Position;
            agentTransformGameObject.transform.SetPositionAndRotation(agentPosition, Quaternion.identity);
            Camera.main.transform.LookAt(agentTransformGameObject.transform, Vector3.up);

            var mouse = Mouse.current;

            if (
                mouse == null ||
                !mouse.leftButton.wasPressedThisFrame ||
                !Physics.Raycast(Camera.main.ScreenPointToRay(new Vector2(mouse.position.x.ReadValue(), mouse.position.y.ReadValue())), out RaycastHit hit)
            ) return;

            EntityManager.AddComponentData(entity, new NavDestination
            {
                WorldPoint = hit.point,
                Teleport = teleport
            });
        }
    }
}
