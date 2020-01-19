using Unity.Entities;
using Unity.Mathematics;

namespace Reese.Demo
{
    struct Projectile : IComponentData
    {
        public bool HasTarget;
        public float AngleInDegrees;
        public float FlightDurationInSeconds;
        public float Gravity;
        public float3 target;
        public float3 Target
        {
            get
            {
                return target;
            }
            set
            {
                this.HasTarget = true;
                target = value;
            }
        }
    }
}
