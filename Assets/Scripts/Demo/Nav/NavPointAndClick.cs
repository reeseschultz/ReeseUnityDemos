using UnityEngine;

namespace Reese.Demo
{
    class NavPointAndClick : MonoBehaviour
    {
        public Camera cam;

        void Update()
        {
            if (
                cam == null ||
                !Input.GetMouseButtonDown(0) ||
                !Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)
            ) return;

            NavPointAndClickDestinationSystem.Destination = hit.point;
        }
    }
}
