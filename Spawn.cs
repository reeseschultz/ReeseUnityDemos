using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace Reese.Spawning
{
    /// <summary>Contains a prefab, a buffer set and a component set to be
    /// enqueued with the SpawnSystem.</summary>
    public class Spawn
    {
        /// <summary>The prefab as an entity.</summary>
        public Entity Prefab { get; private set; } = Entity.Null;

        /// <summary>Returns the stored buffers as a list.</summary>
        public List<IBufferElementData> BufferList { get => bufferSet.ToList(); }

        /// <summary>Returns the stored components as a list.</summary>
        public List<IComponentData> ComponentList { get => componentSet.ToList(); }

        /// <summary>The set of buffers.</summary>
        HashSet<IBufferElementData> bufferSet = new HashSet<IBufferElementData>();

        /// <summary>The set of components.</summary>
        HashSet<IComponentData> componentSet = new HashSet<IComponentData>();

        /// <summary>Sets the prefab with an entity. Defaults to Entity.Null
        /// (which the SpawnSystem would take to mean that there is no prefab,
        /// but an entity would be spawned regardless).</summary>
        public Spawn WithPrefab(Entity entity)
        {
            Prefab = entity;
            return this;
        }

        /// <summary>Sets buffers. Defaults to storing an empty set.
        /// </summary>
        public Spawn WithBufferList(params IBufferElementData[] bufferList)
        {
            bufferSet = new HashSet<IBufferElementData>(bufferList);
            return this;
        }

        /// <summary>Sets components. Defaults to storing an empty set.
        /// </summary>
        public Spawn WithComponentList(params IComponentData[] componentList)
        {
            componentSet = new HashSet<IComponentData>(componentList);
            return this;
        }
    }
}
