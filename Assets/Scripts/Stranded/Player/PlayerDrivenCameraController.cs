using UnityEngine;

namespace Reese.Demo.Stranded
{
    public class PlayerDrivenCameraController : MonoBehaviour
    {
        [SerializeField]
        GameObject agentTransformGO = default;

        Vector3? offset = default;

        void LateUpdate()
        {
            if (agentTransformGO.transform.position.Equals(Vector3.zero)) return; // Must wait for agent transform GO to initialize.

            if (!offset.HasValue)
            {
                offset = transform.position - agentTransformGO.transform.position;
                transform.LookAt(agentTransformGO.transform.position);
            }

            transform.position = Vector3.Lerp(
                transform.position,
                agentTransformGO.transform.position + offset.Value,
                Time.deltaTime
            );
        }
    }
}
