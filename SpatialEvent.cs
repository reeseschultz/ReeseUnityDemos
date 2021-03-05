using System;
using Unity.Collections;
using Unity.Entities;

namespace Reese.Spatial
{
    public struct SpatialEvent : IEquatable<SpatialEvent>, IComparable<SpatialEvent>
    {
        /// <summary>The activator responsible for causing the event.</summary>
        public Entity Activator;

        /// <summary>The tag associated with the event.</summary>
        public FixedString128 Tag;

        public int CompareTo(SpatialEvent other)
            => Activator.CompareTo(other.Activator) + Tag.CompareTo(other.Tag);

        public bool Equals(SpatialEvent other)
            => Activator == other.Activator && Tag.Equals(other.Tag);
    }
}