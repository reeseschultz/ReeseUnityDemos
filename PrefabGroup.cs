using Unity.Entities;

namespace Reese.EntityPrefabGroups
{
    [InternalBufferCapacity(1)]
    public struct PrefabGroup : IBufferElementData
    {
        public static implicit operator Entity(PrefabGroup e) { return e.Value; }
        public static implicit operator PrefabGroup(Entity e) { return new PrefabGroup { Value = e }; }

        /// <summary>The position of the intended jump destination.</summary>
        public Entity Value;
    }
}
