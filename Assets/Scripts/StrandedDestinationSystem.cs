using Reese.Nav;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Reese.Demo
{
    [UpdateAfter(typeof(TransformSystemGroup))]
    class StrandedDestinationSystem : SystemBase
    {
        Entity agentEntity;
        GameObject agentTransformGameObject;

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("Stranded"))
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

            var keyboard = Keyboard.current;

            if (keyboard == null) return;

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
            });
        }
    }
}
