using UnityEngine;

namespace Reese.Demo
{
    class RotationBehaviour : MonoBehaviour
    {
        [SerializeField]
        Vector3 fromRelativeAngles = new Vector3(0, 0, 0);

        [SerializeField]
        Vector3 toRelativeAngles = new Vector3(0, 0, 0);

        [SerializeField]
        float frequency = 1;

        void Start()
        {
            fromRelativeAngles += transform.localRotation.eulerAngles;
            toRelativeAngles += transform.localRotation.eulerAngles;
        }

        void Update()
            => transform.localRotation = Quaternion.Lerp(
                Quaternion.Euler(fromRelativeAngles),
                Quaternion.Euler(toRelativeAngles),
                (Mathf.Sin(Mathf.PI * frequency * Time.time) + 1) * 0.5f
            );
    }
}
