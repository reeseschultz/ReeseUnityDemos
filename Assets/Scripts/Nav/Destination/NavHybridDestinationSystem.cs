using Reese.Nav;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Reese.Demo
{
    public class NavHybridDestinationSystem : MonoBehaviour
    {
        [SerializeField]
        GameObject humanoid = default;

        NavAgentHybrid agent = default;

        void Start()
            => agent = humanoid.GetComponent<NavAgentHybrid>();

        void FixedUpdate()
        {
            var mouse = Mouse.current;

            if (mouse == null) return;

            if (
                mouse.leftButton.wasPressedThisFrame &&
                Physics.Raycast(Camera.main.ScreenPointToRay(new Vector2(mouse.position.x.ReadValue(), mouse.position.y.ReadValue())), out var hit)
            ) agent.WorldDestination = hit.point;
        }
    }
}
