# Reese's DOTS Math Extensions

[![Discord Shield](https://discordapp.com/api/guilds/732665868521177117/widget.png?style=shield)](https://discord.gg/CZ85mguYjK)

Includes math functions missing from DOTS.

## Import

This requires Unity editor `2019.3` or greater. Copy one of the below Git URLs:

* **HTTPS:** `https://github.com/reeseschultz/ReeseUnityDemos.git#math`
* **SSH:** `git@github.com:reeseschultz/ReeseUnityDemos.git#math`

Then go to `Window ⇒ Package Manager` in the editor. Press the `+` symbol in the top-left corner, and then click on `Add package from git URL`. Paste the text you copied and finally click `Add`.

## Usage

Be sure to import `Reese.Math` in any file where you intend to use these extensions!

## Extensions

### `float4x4` Extensions

| Method                                                                                         | Return Type                                            | Description                                                                                                                  |
|------------------------------------------------------------------------------------------------|--------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|
| `MultiplyPoint3x4(float3 point)`                                                               | `float3`                                               | Changes the basis of a point with the given transform.                                                                       |

### `float3` Extensions

| Method                                                                                         | Return Type                                            | Description                                                                                                                  |
|------------------------------------------------------------------------------------------------|--------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|
| `ref MoveTowards(float3 to, float maxDelta)`                                                   | `float3`                                               | Moves from a point to another point by the max delta (moves less than the max delta if target is closer than the max delta). |
| `MultiplyPoint3x4(float4x4 transform)`                                                         | `float3`                                               | Changes the basis of a point with the given transform.                                                                       |
| `ToLocal(LocalToWorld localToWorld)`                                                           | `float3`                                               | Transforms a point from world to local space.                                                                                |
| `ToWorld(LocalToWorld localToWorld)`                                                           | `float3`                                               | Transforms a point from local to world space.                                                                                |
| `ProjectOnPlane(float3 normal)`                                                                | `float3`                                               | Projects a float3 vector onto a planar normal.                                                                               |
| `AxialReplacement(float3 replacingVector, float3 axis)`                                        | `float3`                                               | Component-wise replacement along the provided axes.                                                                          |
| `InvertToUnsignedAxis()`                                                                       | `float3`                                               | Inverts to an unsigned axis. For example, (-1, 0, 1) becomes (0, 1, 0).                                                      |
| `AngleRadians(float3 to)`                                                                      | `float`                                                | Returns the angle between two float3s in radians.                                                                            |
| `AngleDegrees(float3 to)`                                                                      | `float`                                                | Returns the angle between two float3s in degrees.                                                                            |
| `SignedAngleRadians(float3 to, float3 axis)`                                                   | `float`                                                | Returns the signed angle between two float3s in radians about the given axis.                                                |
| `SignedAngleDegrees(float3 to, float3 axis)`                                                   | `float`                                                | Returns the signed angle between two float3s in degrees about the given axis.                                                |
| `Angle360(float3 to, float3 right)`                                                            | `float`                                                | Returns the angle between two float3s ranging between 0 and 360 degrees.                                                     |
| `FromToRotation(float3 to)`                                                                    | `quaternion`                                           | Rotates from one rotation to another.                                                                                        |
| `GetWorldRotationToTarget(LocalToWorld localToWorld, float3 planarAxis = default)`             | `quaternion`                                           | Gets the rotation to the target in world space.                                                                              |

### `quaternion` Extensions

| Method                                                                                         | Return Type                                            | Description                                                                                                                  |
|------------------------------------------------------------------------------------------------|--------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|
| `ToLocal(LocalToWorld localToWorld)`                                                           | `quaternion`                                           | Transforms a quaternion from world to local space.                                                                           |
| `ToWorld(LocalToWorld localToWorld)`                                                           | `quaternion`                                           | Transforms a quaternion from local to world space.                                                                           |
| `RotateTowardsRadians(quaternion to, float deltaRadians)`                                      | `quaternion`                                           | Returns a quaternion interpolated by delta radians.                                                                          |
| `RotateTowardsDegrees(quaternion to, float deltaDegrees)`                                      | `quaternion`                                           | Returns a quaternion interpolated by delta degrees.                                                                          |
| `AngleRadians(quaternion to)`                                                                  | `float`                                                | Returns the angle between two quaternions in radians.                                                                        |
| `AngleDegrees(quaternion to)`                                                                  | `float`                                                | Returns the angle between two quaternions in degrees.                                                                        |

## Contributing

Find a problem, or have an improvement in mind? Great. Go ahead and submit a pull request. Note that the maintainer, Reese, offers no assurance he will respond to you, fix bugs or add features on your behalf in a timely fashion, if ever, [unless you reach an agreement with him about support.](https://reese.codes)

By submitting a pull request, you agree to license your work under [this project's MIT license](https://github.com/reeseschultz/ReeseUnityDemos/blob/master/LICENSE).
