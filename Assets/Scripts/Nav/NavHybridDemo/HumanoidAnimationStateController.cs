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
            animator.SetBool(RUNNING, agent.IsWalking);
            animator.SetBool(JUMPING, agent.IsJumping);
            animator.SetBool(FALLING, agent.IsFalling);
        }
    }
}
