using Reese.Nav;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;

namespace Reese.Demo
{
    class AvoidanceToggler : MonoBehaviour
    {
        public Button Button;

        NavAvoidanceSystem system => World.DefaultGameObjectInjectionWorld.GetExistingSystem<NavAvoidanceSystem>();

        void Start()
        {
            if (Button == null) return;
            Button.onClick.AddListener(Toggle);
        }

        void Toggle() {
            system.Enabled = !system.Enabled;
            var status = system.Enabled ? "on" : "off";
            Debug.Log("Agent-to-agent avoidance is " + status + ".");
        }
    }
}
