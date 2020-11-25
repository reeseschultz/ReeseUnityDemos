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
        public GameObject AgentTransformGameObject { get; private set; }

        Entity agentEntity;

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("Stranded"))
            {
                Enabled = false;
                return;
            }

            AgentTransformGameObject = new GameObject("Agent Transform GameObject");
        }

        protected override void OnUpdate()
        {
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

            AgentTransformGameObject.transform.SetPositionAndRotation(agentPosition, Quaternion.identity);

            var mouse = Mouse.current;

            if (
                mouse == null ||
                !mouse.leftButton.isPressed ||
                !Physics.Raycast(Camera.main.ScreenPointToRay(new Vector2(mouse.position.x.ReadValue(), mouse.position.y.ReadValue())), out RaycastHit hit)
            ) return;

            EntityManager.AddComponentData(agentEntity, new NavNeedsDestination
            {
                Destination = hit.point,
                Tolerance = 15
            });
        }
    }
}
