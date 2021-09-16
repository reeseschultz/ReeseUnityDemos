using Unity.Collections;
using Unity.Entities;

namespace Reese.EntityPrefabGroups
{
    /// <summary>References entities initialized via entity prefab groups.</summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class EntityPrefabSystem : SystemBase
    {
        NativeHashMap<FixedString128, Entity> prefabNameToPrefabMap = default;
        NativeMultiHashMap<FixedString128, Entity> groupNameToPrefabsMap = default;

        internal bool Initialized { get; private set; } = false;

        /// <summary>Gets a single entity prefab using the provided prefab name. Returns false if the prefab cannot be found.</summary>
        public bool TryGet(FixedString128 prefabName, out Entity prefab)
            => prefabNameToPrefabMap.TryGetValue(prefabName, out prefab);

        /// <summary>Gets a native list of entity prefabs using the provided group name. Returns false if the group cannot be found. Remember to dispose the list whether the group is found or not!</summary>
        public bool TryGet(FixedString128 groupName, out NativeList<Entity> prefabs, Allocator allocator)
        {
            prefabs = new NativeList<Entity>(allocator);

            if (!groupNameToPrefabsMap.TryGetFirstValue(groupName, out var prefab, out var iterator)) return false;

            do
            {
                prefabs.Add(prefab);
            } while (groupNameToPrefabsMap.TryGetNextValue(out prefab, ref iterator));

            return true;
        }

        internal void AddGroup(string groupName, Entity prefab)
            => groupNameToPrefabsMap.Add(EntityPrefabUtility.Clean(groupName), prefab);

        internal void AddPrefab(string prefabName, Entity prefab)
            => prefabNameToPrefabMap.TryAdd(EntityPrefabUtility.Clean(prefabName), prefab);

        internal void Initialize(int size)
        {
            prefabNameToPrefabMap = new NativeHashMap<FixedString128, Entity>(size, Allocator.Persistent);
            groupNameToPrefabsMap = new NativeMultiHashMap<FixedString128, Entity>(1, Allocator.Persistent);
            Initialized = true;
        }

        protected override void OnDestroy()
        {
            if (!Initialized) return;

            prefabNameToPrefabMap.Dispose();
            groupNameToPrefabsMap.Dispose();
        }

        protected override void OnUpdate() { }
    }
}
