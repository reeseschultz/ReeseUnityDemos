using System.Collections.Generic;
using Unity.Entities;

namespace Reese.Spawning
{
    public class Spawn : List<IComponentData>
    {
        public Entity PrefabEntity;

        public Spawn(Entity prefabEntity, params IComponentData[] componentList) : base(componentList) {
            PrefabEntity = prefabEntity;
        }
    }
}
