using Unity.Collections;
using Unity.Entities;

namespace Reese.EntityPrefabGroups
{
    /// <summary>References entities initialized via entity prefab groups.</summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class EntityPrefabSystem : SystemBase
    {
        #region Collection Minimum Sizes
        static readonly int MINIMUM_COMPONENTS = 128;
        static readonly int MINIMUM_GROUPS = 128;
        static readonly int MINIMUM_PREFABS = 4096;
        #endregion

        #region Initialization
        NativeHashSet<ComponentType> deniedComponents = default;
        NativeHashSet<Entity> prefabs = default;
        NativeHashSet<Entity> groups = default;

        internal bool Initialized { get; private set; } = false;
        #endregion

        #region Runtime
        NativeHashMap<ComponentType, Entity> componentToPrefabMap = new NativeHashMap<ComponentType, Entity>();
        NativeMultiHashMap<ComponentType, Entity> componentToPrefabsMap = new NativeMultiHashMap<ComponentType, Entity>();
        #endregion

        /// <summary>Gets a list of entity prefabs associated with a group. Returns an empty list if none are found. Remember to dispose the list. Compatible with parallel, Burst-compiled jobs.</summary>
        public NativeList<Entity> GetGroup(ComponentType component, Allocator allocator)
        {
            var prefabList = new NativeList<Entity>(allocator);

            if (!componentToPrefabsMap.TryGetFirstValue(component, out var prefab, out var iterator)) return prefabList;

            do
            {
                prefabList.Add(prefab);
            } while (componentToPrefabsMap.TryGetNextValue(out prefab, ref iterator));

            return prefabList;
        }

        /// <summary>Gets a single entity prefab using the provided component type. Entity is equal to Entity.Null if not found. Compatible with parallel, Burst-compiled jobs.</summary>
        public Entity GetPrefab(ComponentType component)
        {
            componentToPrefabMap.TryGetValue(component, out var prefab);
            return prefab;
        }

        internal void AddGroup(Entity group)
        {
            DenyBuiltInComponents(group);
            groups.Add(group);
        }

        internal void AddPrefabs(NativeArray<Entity> prefabArray)
        {
            foreach (var prefab in prefabArray)
            {
                DenyBuiltInComponents(prefab);
                prefabs.Add(prefab);
            }
        }

        void DenyBuiltInComponents(Entity entity)
        {
            var componentTypes = EntityManager.GetComponentTypes(entity, Allocator.TempJob);
            foreach (var componentType in componentTypes) deniedComponents.Add(componentType); // No custom authoring components will have been added yet, so these can all be denied.
            componentTypes.Dispose();
        }

        internal void Initialize()
        {
            deniedComponents = new NativeHashSet<ComponentType>(MINIMUM_COMPONENTS, Allocator.Persistent);

            groups = new NativeHashSet<Entity>(MINIMUM_GROUPS, Allocator.Persistent);
            prefabs = new NativeHashSet<Entity>(MINIMUM_PREFABS, Allocator.Persistent);

            componentToPrefabsMap = new NativeMultiHashMap<ComponentType, Entity>(MINIMUM_COMPONENTS, Allocator.Persistent);
            componentToPrefabMap = new NativeHashMap<ComponentType, Entity>(MINIMUM_COMPONENTS, Allocator.Persistent);

            Initialized = true;
        }

        bool ran = default;
        protected override void OnStartRunning() // Custom authoring components are not available until this lifecycle method!
        {
            if (ran || !Initialized) return; // Prevents re-running if disabled and re-enabled.

            foreach (var prefab in prefabs)
            {
                var componentTypes = EntityManager.GetComponentTypes(prefab, Allocator.TempJob);

                foreach (var componentType in componentTypes)
                {
                    if (deniedComponents.Contains(componentType)) continue;

                    componentToPrefabMap.TryAdd(componentType, prefab);
                }

                componentTypes.Dispose();
            }

            foreach (var group in groups)
            {
                if (!EntityManager.HasComponent<PrefabGroup>(group)) continue;

                var groupBuffer = EntityManager.GetBuffer<PrefabGroup>(group);
                if (groupBuffer.Length < 1) continue;

                var componentTypes = EntityManager.GetComponentTypes(group, Allocator.TempJob);

                foreach (var componentType in componentTypes)
                {
                    if (deniedComponents.Contains(componentType)) continue;

                    foreach (var prefab in groupBuffer)
                        componentToPrefabsMap.Add(componentType, prefab);
                }

                componentTypes.Dispose();
            }

            prefabs.Dispose();
            groups.Dispose();
            deniedComponents.Dispose();

            ran = true;
        }

        protected override void OnDestroy()
        {
            if (!Initialized) return;

            componentToPrefabMap.Dispose();
            componentToPrefabsMap.Dispose();
        }

        protected override void OnUpdate() { }
    }
}
