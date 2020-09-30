using Unity.Mathematics;
using UnityEngine.AI;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine.Experimental.AI;

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
        /// A scale is additionally provided to massage the output position. It
        /// multiplies the AABB extents which are computed from the center.
        /// Note also that a Unity.Mathematics.Random *ref* must be supplied
        /// to ensure the state of the random number generator is updated--that
        /// way the job this is called from can ensure that state change is
        /// preserved for the next job using said generator.</summary>
        public static float3 GetRandomPointInBounds(ref Unity.Mathematics.Random random, AABB aabb, float scale)
        {
            var extents = aabb.Center + aabb.Extents * scale;

            var position = new float3(
                random.NextFloat(-extents.x, extents.x),
                random.NextFloat(-extents.y, extents.y),
                random.NextFloat(-extents.z, extents.z)
            );

            return position;
        }

        /// <summary>Extension method for PhysicsWorld, checking for a valid
        /// position by raycasting onto the surface layer from the passed
        /// position. Returns true if the raycast is successful and a position
        /// via out.</summary>
        public static bool GetPointOnSurfaceLayer(this PhysicsWorld physicsWorld, LocalToWorld localToWorld, float3 position, out float3 pointOnSurface)
        {
            var rayInput = new RaycastInput()
            {
                Start = position + localToWorld.Up * NavConstants.OBSTACLE_RAYCAST_DISTANCE_MAX,
                End = position - localToWorld.Up * NavConstants.OBSTACLE_RAYCAST_DISTANCE_MAX,
                Filter = new CollisionFilter()
                {
                    BelongsTo = ToBitMask(NavConstants.COLLIDER_LAYER),
                    CollidesWith = ToBitMask(NavConstants.SURFACE_LAYER)
                }
            };

            pointOnSurface = float3.zero;
            if (physicsWorld.CastRay(rayInput, out var hit))
            {
                pointOnSurface = hit.Position;
                return true;
            }
            return false;
        }

        /// <summary>Transforms a point, reimplementing the old
        /// Matrix4x4.MultiplyPoint3x4 using Unity.Mathematics.</summary>
        public static float3 MultiplyPoint3x4(float4x4 transform, float3 point)
            => math.mul(transform, new float4(point, 1)).xyz;

        /// <summary>Gets the agent type from the string name.</summary>
        public static int GetAgentType(string agentName)
        {
            for (var i = 0; i < NavMesh.GetSettingsCount(); ++i)
                if (agentName == NavMesh.GetSettingsNameFromID(NavMesh.GetSettingsByIndex(i).agentTypeID))
                    return i;

            return -1;
        }

        /// <summary>Converts the layer to a bit mask. Valid layers range from
        /// 8 to 30, inclusive. All other layers are invalid, and will always
        /// result in layer 8, since they are used by Unity internally. See
        /// https://docs.unity3d.com/Manual/class-TagManager.html and
        /// https://docs.unity3d.com/Manual/Layers.html for more information.
        /// </summary>
        public static uint ToBitMask(int layer)
            => (layer < 8 || layer > 30) ? 1u << 8 : 1u << layer;

        /// <summary>Inverts a bit mask, meaning that it applies to all layers
        /// *except* for the one expressed in said mask.</summary>
        public static uint InvertBitMask(uint bitMask)
            => ~bitMask;

        /// <summary>Returns true if a status `a` has `b`. Note that a
        /// PathQueryStatus is a set of flags which can be simultaneously
        /// active.</summary>
        public static bool HasStatus(PathQueryStatus a, PathQueryStatus b)
            => (a & b) == a;
    }
}
