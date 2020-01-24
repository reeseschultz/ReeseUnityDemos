using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>A buffer of "jumpable" surfaces from a given surface. These
    /// can be queried from the current NavAgent.Surface to determine which
    /// other surfaces are jumpable for said agent.</summary>
    [InternalBufferCapacity(NavConstants.JUMPABLE_SURFACE_MAX)]
    public struct NavJumpableBufferElement : IBufferElementData
    {
        public static implicit operator Entity(NavJumpableBufferElement e) { return e.Value; }
        public static implicit operator NavJumpableBufferElement(Entity e) { return new NavJumpableBufferElement { Value = e }; }

        /// <summary>A "jumpable" surface from the one this buffer is attached
        /// to.</summary>
        public Entity Value;
    }
}
