using Reese.Nav;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reese.Demo.Stranded
{
    class PlayerDrowningSystem : SystemBase
    {
        PlayerDeathSequenceController deathSequenceController = default;

        bool ranDeathSequence = default;

        protected override void OnUpdate()
        {
            if (ranDeathSequence || !SceneManager.GetActiveScene().name.Equals("Stranded")) return;

            if (deathSequenceController == null)
            {
                var go = GameObject.Find("Player Death Sequence Controller");

                if (go == null) return;

                deathSequenceController = go.GetComponent<PlayerDeathSequenceController>();

                if (deathSequenceController == null) return;
            }

            var elapsedSeconds = (float)Time.ElapsedTime;
            var controller = deathSequenceController;
            var ran = false;

            Entities
                .WithAny<NavHasProblem, NavFalling>()
                .ForEach((in NavAgent agent, in Player player) =>
                {
                    controller.Run();

                    ran = true;
                })
                .WithoutBurst()
                .WithName("DrownJob")
                .Run();

            ranDeathSequence = ran;
        }
    }
}
