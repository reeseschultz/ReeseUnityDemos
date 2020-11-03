using Reese.Nav;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject prefab = default;

    GameObject go = default;

    NavAgentHybrid navAgentHybrid = default;

    void Start()
    {
        go = Instantiate(prefab, new Vector3(0, 10, 0), Quaternion.identity);

        navAgentHybrid = go.AddComponent<NavAgentHybrid>();
    }

    void FixedUpdate()
    {
        var mouse = Mouse.current;

        if (mouse == null) return;

        if (
            mouse.leftButton.wasPressedThisFrame &&
            Physics.Raycast(Camera.main.ScreenPointToRay(new Vector2(mouse.position.x.ReadValue(), mouse.position.y.ReadValue())), out var hit)
        ) navAgentHybrid.WorldDestination = hit.point;
    }
}
