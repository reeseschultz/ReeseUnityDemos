using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Reese.Nav
{
    /// <summary>Authors a NavSurface.</summary>
    [RequiresEntityConversion]
    public class NavSurfaceAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        /// <summary>If true the GameObject's transform will be used and
        /// applied to possible children via CopyTransformFromGameObject.
        /// If false the entity's transform will be used and applied conversely
        /// via CopyTransformToGameObject.
        /// </summary>
        public bool HasGameObjectTransform;

        /// <summary>This is a list of surfaces that are "jumpable" from *this*
        /// one. Immense thought went into this design, and it was determined
        /// that automating what's "jumpable" is probably out of scope for this
        /// project, but not automating jumping itself. Ultimately it largely
        /// depends on the design of one's game. This means it's *entirely on
        /// you* to figure out which surfaces are "jumpable" from one another.
        /// The agent will *not* automatically know that there is a surface
        /// in-between them.</summary>
        public List<NavSurfaceAuthoring> JumpableSurfaces;

        /// <summary>This is the glorified parent transform of the surface.
        /// </summary>
        public NavBasisAuthoring Basis;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData<NavSurface>(entity, new NavSurface
            {
                Basis = conversionSystem.GetPrimaryEntity(Basis)
            });

            if (HasGameObjectTransform) dstManager.AddComponent(entity, typeof(CopyTransformFromGameObject));
            else dstManager.AddComponent(entity, typeof(CopyTransformToGameObject));

            dstManager.AddComponent(entity, typeof(NavJumpableBufferElement));
            var jumpableBuffer = dstManager.GetBuffer<NavJumpableBufferElement>(entity);
            JumpableSurfaces.ForEach(surface => jumpableBuffer.Add(conversionSystem.GetPrimaryEntity(surface)));

            dstManager.AddComponent(entity, typeof(NavJumpableBufferElement));

            dstManager.RemoveComponent(entity, typeof(NonUniformScale));
            dstManager.RemoveComponent(entity, typeof(MeshRenderer));
            dstManager.RemoveComponent(entity, typeof(RenderMesh));
        }
    }
}
