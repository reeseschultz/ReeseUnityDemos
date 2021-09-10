using UnityEngine;

namespace Reese.Demo {
    public class DemoCameraControl : MonoBehaviour
    {
        [SerializeField]
        public float cameraTraverseSpeed = 100f;

        [SerializeField]
        public float cameraRotateSpeed = 100f;

        void Update()
        {
            var cameraMovement = Vector3.zero;

            if (Input.GetKey(KeyCode.Mouse1))
            {
                transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * cameraRotateSpeed * Time.deltaTime, Space.World);
                transform.Rotate(-Input.GetAxis("Mouse Y") * cameraRotateSpeed * Time.deltaTime, 0, 0);
            }

            if (Input.GetKey(KeyCode.W)) cameraMovement += transform.forward * cameraTraverseSpeed * Time.deltaTime;
            else if (Input.GetKey(KeyCode.S)) cameraMovement -= transform.forward * cameraTraverseSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.D)) cameraMovement += transform.right * cameraTraverseSpeed * Time.deltaTime;
            else if (Input.GetKey(KeyCode.A)) cameraMovement -= transform.right * cameraTraverseSpeed * Time.deltaTime;

            transform.position += cameraMovement;
        }
    }
}
