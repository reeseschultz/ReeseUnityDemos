using Unity.Entities;

namespace Reese.Nav
{
    /// <summary>A NavBasis is a glorified parent transform for NavSurfaces.
    /// </summary>
    public struct NavBasis : IComponentData
    {
        /// <summary>This is intended only to be set during authoring. If you
        /// need to make a runtime change, then modify the Parent component
        /// on the basis instead. To be clear, this variable refers to the
        /// parent basis of a given basis, and the only reason it exists is
        /// because Unity.Physics, when it kicks in, eliminates Parent
        /// components set during authoring. (At least, in 2019.3.) Thus,
        /// this reference is used to set the Parent component *after*
        /// Unity.Physics does its thing.</summary>
        public Entity ParentBasis;
    }
}
