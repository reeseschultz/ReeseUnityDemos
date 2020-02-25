using Unity.Mathematics;
using UnityEngine.AI;

namespace Reese.Nav
{
    public static class NavUtil
    {

        /// <summary>Checks approximate equality between two float3s.</summary>
        public static bool ApproxEquals(float3 a, float3 b, float tolerance)
            => !ApproxEquals(a.x, b.x, tolerance) || !ApproxEquals(a.y, b.y, tolerance) || !ApproxEquals(a.z, b.z, tolerance) ? false : true;

        /// <summary>Checks approximate equality between two floats.</summary>
        public static bool ApproxEquals(float a, float b, float tolerance)
            => math.abs(a - b) > tolerance ? false : true;

        /// <summary>Gets a random point within the provided bounds (AABB).
        /// An offset and scale are additionally provided to massage the output
        /// position. The offset is self-explanatory, but the scale
        /// multiplies the AABB extents which are computed from the center.
        /// Note also that a Unity.Mathematics.Random *ref* must be supplied
        /// to ensure the state of the random number generator is updated--that
        /// way the job this is called from can ensure that state change is
        /// preserved for the next job using said generator.</summary>
        public static float3 GetRandomPointInBounds(ref Unity.Mathematics.Random random, AABB aabb, float3 offset, float scale)
        {
            var extents = aabb.Center + aabb.Extents * scale;

            var position = new float3(
                random.NextFloat(-extents.x, extents.x),
                random.NextFloat(-extents.y, extents.y),
                random.NextFloat(-extents.z, extents.z)
            );

            return position + offset;
        }

        /// <summary>Transforms a point, reimplementing the old
        /// Matrix4x4.MultiplyPoint3x4 using Unity.Mathematics.</summary>
        public static float3 MultiplyPoint3x4(float4x4 transform, float3 point)
            => math.mul(transform, new float4(point, 1)).xyz;

        /// <summary>Gets the agent type from the string name.</summary>
        public static int GetAgentType(string agentName)
        {
            for (int i = 0; i < NavMesh.GetSettingsCount(); ++i)
                if (agentName == NavMesh.GetSettingsNameFromID(NavMesh.GetSettingsByIndex(i).agentTypeID))
                    return i;

            return -1;
        }
    }
}
