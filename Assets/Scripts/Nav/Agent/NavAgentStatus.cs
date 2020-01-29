using Unity.Entities;

namespace Reese.Nav
{
    struct NavFalling : IComponentData { }
    struct NavJumping : IComponentData { }
    struct NavJumped : IComponentData { }
    struct NavLerping : IComponentData { }
    struct NavPlanning : IComponentData { }
}
