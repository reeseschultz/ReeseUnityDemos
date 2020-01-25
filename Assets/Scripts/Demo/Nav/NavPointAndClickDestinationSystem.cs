using Reese.Nav;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Reese.Demo
{
    class NavPointAndClickDestinationSystem : JobComponentSystem
    {
        public static float3 Destination;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!SceneManager.GetActiveScene().name.Equals("NavPointAndClickDemo")) return inputDeps;

            var destination = Destination;

            var job = Entities
                .ForEach((ref NavAgent agent) =>
                {
                    var offsetDestination = destination + agent.Offset;
                    if (agent.IsLerping || agent.LastDestination.Equals(offsetDestination)) return;
                    agent.WorldDestination = offsetDestination;
                })
                .WithName("NavPointAndClickDestinationJob")
                .Schedule(inputDeps);

            job.Complete();

            return job;
        }
    }
}
