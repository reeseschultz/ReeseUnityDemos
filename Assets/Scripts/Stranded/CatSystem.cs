using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    class CatSystem : SystemBase
    {
        GameObject catGO = default;

        protected override void OnCreate()
        {
            if (!SceneManager.GetActiveScene().name.Equals("Stranded"))
            {
                Enabled = false;
                return;
            }

            catGO = GameObject.Find("Cat");
        }

        protected override void OnUpdate()
        {
            if (catGO == null) return;

            Entities
                .WithChangeFilter<SpatialEvent>()
                .ForEach((in Cat cat) =>
                {
                    var meowController = catGO.GetComponent<CatMeowController>();

                    if (meowController == null) return;

                    meowController.Meow();
                })
                .WithoutBurst()
                .WithName("MeowJob")
                .Run();
        }
    }
}
