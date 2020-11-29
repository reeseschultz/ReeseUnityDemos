using UnityEngine;

namespace Reese.Demo
{
    public class StrandedCameraController : MonoBehaviour
    {
        [SerializeField]
        GameObject agentTransformGameObject = default;

        Vector3? offset = null;

        void LateUpdate()
        {
            if (!offset.HasValue)
            {
                offset = transform.position - agentTransformGameObject.transform.position;
                transform.LookAt(agentTransformGameObject.transform.position);
            }

            transform.position = Vector3.Lerp(
                transform.position,
                agentTransformGameObject.transform.position + offset.Value,
                Time.deltaTime
            );
        }
    }
}
