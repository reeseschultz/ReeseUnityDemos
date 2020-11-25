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
                    var source = catGO.GetComponent<AudioSource>();

                    if (source == null) return;

                    source.Play();
                })
                .WithoutBurst()
                .WithName("MeowJob")
                .Run();
        }
    }
}
