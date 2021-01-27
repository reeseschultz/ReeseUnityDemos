using Reese.Nav;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reese.Demo.Stranded
{
    class PlayerSystem : SystemBase
    {
        GameObject go = default;
        PlayerSoundController controller = default;

        protected override void OnUpdate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("Stranded")) return;

            if (go == null)
            {
                go = GameObject.Find("Player GO");
                if (go == null) return;
            }

            if (controller == null)
            {
                controller = go.GetComponent<PlayerSoundController>();
                if (controller == null) return;
            }

            if (controller.IsWalking())
            {
                Entities
                    .WithNone<NavWalking>()
                    .ForEach((Entity entity, in Player player) =>
                    {
                        controller.StopWalking();
                    })
                    .WithoutBurst()
                    .WithName("PlayerStopWalkSoundJob")
                    .Run();
            }

            Entities
                .WithChangeFilter<NavWalking>()
                .ForEach((Entity entity, in Player player) =>
                {
                    controller.Walk();
                })
                .WithoutBurst()
                .WithName("PlayerStartWalkSoundJob")
                .Run();

            Entities
                .WithChangeFilter<NavJumping>()
                .ForEach((Entity entity, in Player player) =>
                {
                    controller.Jump();
                })
                .WithoutBurst()
                .WithName("PlayerJumpSoundJob")
                .Run();
        }
    }
}
