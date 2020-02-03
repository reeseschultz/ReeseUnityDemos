using Unity.Entities;

namespace Reese.Demo {
    [GenerateAuthoringComponent]
    struct PersonPrefab : IComponentData
    {
        public Entity Value;
    }
}
