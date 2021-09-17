using Unity.Entities;

namespace Reese.EntityPrefabGroups
{
    public static class EntityPrefabExtensions
    {
        /// <summary>Gets a prefab with the specified singleton component type.</summary>
        public static Entity GetPrefab<T>(this EntityManager entityManager) where T : struct, IComponentData
            => entityManager.CreateEntityQuery(
                typeof(T),
                typeof(Prefab)
            ).GetSingletonEntity();

        /// <summary>Gets a buffer of prefabs with a group that has the specified singleton component type.</summary>
        public static DynamicBuffer<PrefabGroup> GetPrefabs<T>(this EntityManager entityManager) where T : struct, IComponentData
        {
            var group = entityManager.CreateEntityQuery(
                typeof(T),
                typeof(PrefabGroup)
            ).GetSingletonEntity();

            return entityManager.GetBuffer<PrefabGroup>(group);
        }
    }
}
