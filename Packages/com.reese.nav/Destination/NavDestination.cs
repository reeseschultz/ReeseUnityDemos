using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>A NavDestination is a physical destination parented to a
    /// destination surface.</summary>
    public struct NavDestination : IComponentData {
        public Entity Agent;
    }
}
