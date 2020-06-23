using UnityEngine;
using Unity.Mathematics;

namespace Reese.Demo
{
    class NavRotator : MonoBehaviour
    {
        float xAngleMax = 10;
        float yAngleMax = 30;

        void Update()
            => transform.rotation = Quaternion.Euler(xAngleMax * math.sin(Time.time * 0.5f), 0, yAngleMax * math.sin(Time.time * 2));
    }
}
