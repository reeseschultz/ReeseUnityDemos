using Unity.Mathematics;
using UnityEngine.AI;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine.Experimental.AI;
using UnityEngine;
using Unity.Entities;

namespace Reese.Nav
{
    public static class NavUtil
    {
        /// <summary>Gets a point on a navigable surface (either the current surface or a jumpable one) for the provided agent entity via the out hit parameter. Returns true if there is a navigable surface, false if not.</summary>
        public static bool GetPointOnNavigableSurface(Vector3 point, Entity agentEntity, Camera cam, PhysicsWorld physicsWorld, float raycastDistance, EntityManager entityManager, CollisionFilter filter, out Unity.Physics.RaycastHit hit)
        {
            var screenPointToRay = cam.ScreenPointToRay(point);

            var rayInput = new RaycastInput
            {
                Start = screenPointToRay.origin,
                End = screenPointToRay.GetPoint(raycastDistance),
                Filter = filter
            };

            if (!physicsWorld.CastRay(rayInput, out hit) || hit.RigidBodyIndex == -1) return false;

            if (!NavMesh.SamplePosition(hit.Position, out var navMeshHit, 1, NavMesh.AllAreas)) return false;

            var hitSurfaceEntity = physicsWorld.Bodies[hit.RigidBodyIndex].Entity;

            if (hitSurfaceEntity == Entity.Null) return false;

            if (!entityManager.HasComponent<Parent>(agentEntity)) return false;

            var surfaceEntity = entityManager.GetComponentData<Parent>(agentEntity).Value;

            if (surfaceEntity == Entity.Null) return false;

            if (surfaceEntity == hitSurfaceEntity) return true;

            if (!entityManager.HasComponent<NavJumpableBufferElement>(surfaceEntity)) return false;

            var jumpableSurfaces = entityManager.GetBuffer<NavJumpableBufferElement>(surfaceEntity);

            for (var i = 0; i < jumpableSurfaces.Length; ++i)
                if (hitSurfaceEntity == jumpableSurfaces[i])
                    return true;

            return false;
        }

        /// <summary>Checks approximate equality between two float3s.</summary>
        public static bool ApproxEquals(float3 a, float3 b, float tolerance)
            => ApproxEquals(a.x, b.x, tolerance) && ApproxEquals(a.y, b.y, tolerance) && ApproxEquals(a.z, b.z, tolerance);

        /// <summary>Checks approximate equality between two floats.</summary>
        public static bool ApproxEquals(float a, float b, float tolerance)
            => math.abs(a - b) <= tolerance;

        /// <summary>Gets a random point within the provided bounds (AABB).
        /// A scale is additionally provided to massage the output position. It
        /// multiplies the AABB extents which are computed from the center.
        /// Note also that a Unity.Mathematics.Random *ref* must be supplied
        /// to ensure the state of the random number generator is updated--that
        /// way the job this is called from can ensure that state change is
        /// preserved for the next job using said generator.</summary>
        public static float3 GetRandomPointInBounds(ref Unity.Mathematics.Random random, AABB aabb, float scale, float3 offset)
        {
            var extents = offset + aabb.Extents * scale;

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
        public static bool GetPointOnSurfaceLayer(this PhysicsWorld physicsWorld, LocalToWorld localToWorld, float3 position, out float3 pointOnSurface, float obstacleRaycastDistanceMax, int colliderLayer, int surfaceLayer)
        {
            var rayInput = new RaycastInput()
            {
                Start = position + localToWorld.Up * obstacleRaycastDistanceMax,
                End = position - localToWorld.Up * obstacleRaycastDistanceMax,
                Filter = new CollisionFilter()
                {
                    BelongsTo = ToBitMask(colliderLayer),
                    CollidesWith = ToBitMask(surfaceLayer)
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
