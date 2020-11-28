using UnityEngine;
using UnityEngine.InputSystem;

namespace Reese.Demo
{
    class CursorHoldController : MonoBehaviour
    {
        [SerializeField]
        float speed = 25;

        Vector3 originalScale = default;

        void Start()
            => originalScale = transform.localScale;

        void Update()
        {
            var mouse = Mouse.current;

            var scale = transform.localScale;

            if (transform.localScale.magnitude <= originalScale.magnitude) scale = originalScale;
            else scale *= 0.8f;

            if (mouse.leftButton.isPressed) scale += Vector3.one * 0.44f;

            transform.localScale = Vector3.Lerp(transform.localScale, scale, speed * Time.deltaTime);
        }
    }
}
