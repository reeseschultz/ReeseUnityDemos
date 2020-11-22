using Unity.Entities;
using UnityEngine;

namespace Reese.Demo
{
    public class StrandedCameraController : MonoBehaviour
    {
        StrandedDestinationSystem strandedDestinationSystem => World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<StrandedDestinationSystem>();

        Vector3? offset = null;

        void LateUpdate()
        {
            if (!offset.HasValue) offset = transform.position - strandedDestinationSystem.AgentTransformGameObject.transform.position;

            transform.position = Vector3.Lerp(
                transform.position,
                strandedDestinationSystem.AgentTransformGameObject.transform.position + offset.Value,
                Time.deltaTime
            );
        }
    }
}
