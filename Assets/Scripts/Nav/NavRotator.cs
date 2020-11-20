using UnityEngine;
using Unity.Mathematics;

namespace Reese.Demo
{
    class NavRotator : MonoBehaviour
    {
        [SerializeField]
        float xAngleMax = 10;

        [SerializeField]
        float yAngleMax = 30;

        [SerializeField]
        float xAngleRate = 0.5f;

        [SerializeField]
        float yAngleRate = 2;

        void Update()
            => transform.rotation = Quaternion.Euler(xAngleMax * math.sin(Time.time * xAngleRate), 0, yAngleMax * math.sin(Time.time * yAngleRate));
    }
}
