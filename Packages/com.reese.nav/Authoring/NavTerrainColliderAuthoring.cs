using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Rendering;
using UnityEngine;

namespace Reese.Nav
{
    /// <summary>Authors a terrain collider.</summary>
    [RequiresEntityConversion]
    public class NavTerrainColliderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        PhysicsCategoryTags belongsTo;

        [SerializeField]
        PhysicsCategoryTags collidesWith;

        [SerializeField]
        int groupIndex;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var terrain = GetComponent<Terrain>();

            if (terrain == null)
            {
                Debug.LogError("No terrain found!");
                return;
            }

            CollisionFilter collisionFilter = new CollisionFilter
            {
                BelongsTo = belongsTo.Value,
                CollidesWith = collidesWith.Value,
                GroupIndex = groupIndex
            };

            dstManager.AddComponentData(entity, CreateTerrainCollider(terrain.terrainData, collisionFilter));

            var renderer = GetComponent<Renderer>();

            if (renderer == null)
            {
                Debug.LogError("No renderer found! Please attach a mesh renderer to the terrain.");
                return;
            }

            var bounds = new AABB
            {
                Center = renderer.bounds.center,
                Extents = renderer.bounds.extents
            };

            dstManager.AddComponentData(entity, new RenderBounds { Value = bounds });
        }

        public static PhysicsCollider CreateTerrainCollider(TerrainData terrainData, CollisionFilter filter)
        {
            var physicsCollider = new PhysicsCollider();
            var scale = terrainData.heightmapScale;
            var size = new int2(terrainData.heightmapResolution, terrainData.heightmapResolution);
            var colliderHeights = new NativeArray<float>(terrainData.heightmapResolution * terrainData.heightmapResolution, Allocator.TempJob);
            var terrainHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

            for (var j = 0; j < size.y; ++j)
                for (var i = 0; i < size.x; ++i)
                    colliderHeights[j + i * size.x] = terrainHeights[i, j];

            physicsCollider.Value = Unity.Physics.TerrainCollider.Create(colliderHeights, size, scale, Unity.Physics.TerrainCollider.CollisionMethod.Triangles, filter);

            colliderHeights.Dispose();

            return physicsCollider;
        }
    }
}
