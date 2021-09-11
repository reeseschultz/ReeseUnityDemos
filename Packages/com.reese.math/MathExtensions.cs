using Unity.Mathematics;
using Unity.Transforms;

namespace Reese.Math
{
    public static class MathExtensions
    {
        #region float4x4 Extensions

        /// <summary>Changes the basis of a point with the given transform.</summary>
        public static float3 MultiplyPoint3x4(this float4x4 transform, float3 point)
            => math.mul(transform, new float4(point, 1)).xyz;

        #endregion

        #region float3 Extensions

        /// <summary>Moves from a point to another point by the max delta (moves less than the max delta if target is closer than the max delta).</summary>
        public static float3 MoveTowards(this ref float3 from, float3 to, float maxDelta)
        {
            var moveTo = to - from;
            var length = math.length(moveTo);

            if (length == 0f || length <= maxDelta)
            {
                from = to;
                return from;
            }

            from = moveTo / length * maxDelta + from;
            return from;
        }

        /// <summary>Changes the basis of a point with the given transform.</summary>
        public static float3 MultiplyPoint3x4(this float3 point, float4x4 transform)
            => math.mul(transform, new float4(point, 1)).xyz;

        /// <summary>Transforms a point from world to local space.</summary>
        public static float3 ToLocal(this float3 point, LocalToWorld localToWorld)
            => point.MultiplyPoint3x4(math.inverse(localToWorld.Value));

        /// <summary>Transforms a point from local to world space.</summary>
        public static float3 ToWorld(this float3 point, LocalToWorld localToWorld)
            => point.MultiplyPoint3x4(localToWorld.Value);

        /// <summary>Projects a float3 vector onto a planar normal.</summary>
        public static float3 ProjectOnPlane(this float3 vector, float3 normal)
            => vector - math.dot(vector, normal);

        /// <summary>Component-wise replacement along the provided axes.</summary>
        public static float3 AxialReplacement(this float3 vector, float3 replacingVector, float3 axis)
            => axis.InvertToUnsignedAxis() * vector + axis * replacingVector;

        /// <summary>Inverts to an unsigned axis. For example, (-1, 0, 1) becomes (0, 1, 0).<summary>
        public static float3 InvertToUnsignedAxis(this float3 axis)
            => new int3(math.sign(math.abs(axis))) ^ 1;

        /// <summary>Returns the angle between two float3s in radians.</summary>
        public static float AngleRadians(this float3 from, float3 to)
        {
            // (See page 47 of https://people.eecs.berkeley.edu/~wkahan/Mindless.pdf by William Kahan).

            var a = from * math.length(to);
            var b = to * math.length(from);

            return math.atan2(math.length(a - b), math.length(a + b)) * 2f;
        }

        /// <summary>Returns the angle between two float3s in degrees.</summary>
        public static float AngleDegrees(this float3 from, float3 to)
            => math.degrees(AngleRadians(from, to));

        /// <summary>Returns the signed angle between two float3s in radians about the given axis.</summary>
        public static float SignedAngleRadians(this float3 from, float3 to, float3 axis)
            => AngleRadians(from, to) * math.sign(math.dot(axis, math.cross(from, to)));

        /// <summary>Returns the signed angle between two float3s in degrees about the given axis.</summary>
        public static float SignedAngleDegrees(this float3 from, float3 to, float3 axis)
            => math.degrees(SignedAngleRadians(from, to, axis));

        /// <summary>Returns the angle between two float3s ranging between 0 and 360 degrees.</summary>
        public static float Angle360(this float3 from, float3 to, float3 right)
        {
            var angle = from.AngleDegrees(to);

            return right.AngleDegrees(to) > 90 ? 360 - angle : angle;
        }

        /// <summary>Rotates from one rotation to another.</summary>
        public static quaternion FromToRotation(this float3 from, float3 to)
            => quaternion.AxisAngle(
                math.normalizesafe(math.cross(from, to)),
                math.acos(math.clamp(math.dot(math.normalizesafe(from), math.normalizesafe(to)), -1f, 1f))
            );

        /// <summary>Gets the rotation to the target in world space.</summary>
        public static quaternion GetWorldRotationToTarget(this float3 worldTargetPosition, LocalToWorld localToWorld, float3 planarAxis = default)
        {
            var lookAt = math.normalizesafe(worldTargetPosition - localToWorld.Position);

            return quaternion.LookRotationSafe(lookAt.ProjectOnPlane(planarAxis), math.up());
        }

        #endregion

        #region quaternion Extensions

        /// <summary>Transforms a quaternion from world to local space.</summary>
        public static quaternion ToLocal(this quaternion worldQuaternion, LocalToWorld localToWorld)
            => math.mul(math.inverse(localToWorld.Rotation), worldQuaternion);

        /// <summary>Transforms a quaternion from local to world space.</summary>
        public static quaternion ToWorld(this quaternion worldQuaternion, LocalToWorld localToWorld)
            => math.mul(localToWorld.Rotation, worldQuaternion);

        /// <summary>Returns a quaternion interpolated by delta radians.</summary>
        public static quaternion RotateTowardsRadians(this quaternion from, quaternion to, float deltaRadians)
            => math.slerp(from, to, math.min(1f, deltaRadians / AngleRadians(from, to)));

        /// <summary>Returns a quaternion interpolated by delta degrees.</summary>
        public static quaternion RotateTowardsDegrees(this quaternion from, quaternion to, float deltaDegrees)
            => RotateTowardsRadians(from, to, math.radians(deltaDegrees));

        /// <summary>Returns the angle between two quaternions in radians.</summary>
        public static float AngleRadians(this quaternion from, quaternion to)
            => math.acos(math.min(math.abs(math.dot(from, to)), 1f)) * 2f;

        /// <summary>Returns the angle between two quaternions in degrees.</summary>
        public static float AngleDegrees(this quaternion from, quaternion to)
            => math.degrees(AngleRadians(from, to));

        #endregion
    }
}
