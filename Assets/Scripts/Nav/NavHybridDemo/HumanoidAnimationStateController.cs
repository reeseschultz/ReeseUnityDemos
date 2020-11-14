using UnityEngine;
using Reese.Nav;

namespace Reese.Demo
{
    public class HumanoidAnimationStateController : MonoBehaviour
    {
        const string RUNNING = "running";
        const string JUMPING = "jumping";
        const string FALLING = "falling";

        Animator animator = default;
        NavAgentHybrid agent = default;

        void Start()
        {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavAgentHybrid>();
        }

        void Update()
        {
            animator.SetBool(RUNNING, agent.IsLerping);
            animator.SetBool(JUMPING, agent.IsJumping);
            animator.SetBool(FALLING, agent.IsFalling);

            // if (agent.IsLerping)
            // {
                // animator.CrossFadeInFixedTime("Running", 0.05f, 0);
                // animator.Play("Running", 0);
            // }
        }
    }
}
