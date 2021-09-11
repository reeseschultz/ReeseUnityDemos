//
// Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
//
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
//

// The original source code has been modified by Unity Technologies.

// The source code as modified by Unity Technologies has been modified by Reese Schultz.

using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.AI;

[Flags]
public enum StraightPathFlags
{
    Start = 0x01,              // The vertex is the start position.
    End = 0x02,                // The vertex is the end position.
    OffMeshConnection = 0x04   // The vertex is start of an off-mesh link.
}

public static class PathUtils
{
    public static float Perp2D(float3 u, float3 v)
        => u.z * v.x - u.x * v.z;

    public static void Swap(ref Vector3 a, ref Vector3 b)
    {
        var temp = a;
        a = b;
        b = temp;
    }

    // Retrace portals between corners and register if type of polygon changes:
    public static int RetracePortals(NavMeshQuery query, int startIndex, int endIndex, NativeSlice<PolygonId> path, int n, float3 termPos, ref NativeArray<NavMeshLocation> straightPath, ref NativeArray<StraightPathFlags> straightPathFlags, int maxStraightPath)
    {
        for (var i = startIndex; i < endIndex - 1; ++i)
        {
            var type1 = query.GetPolygonType(path[i]);
            var type2 = query.GetPolygonType(path[i + 1]);

            if (type1 == type2) continue;

            var status = query.GetPortalPoints(path[i], path[i + 1], out var l, out var r);

            GeometryUtils.SegmentSegmentCPA(out var cpa1, out var cpa2, l, r, straightPath[n - 1].position, termPos);
            straightPath[n] = query.CreateLocation(cpa1, path[i + 1]);

            straightPathFlags[n] = (type2 == NavMeshPolyTypes.OffMeshConnection) ? StraightPathFlags.OffMeshConnection : 0;

            if (++n == maxStraightPath) return maxStraightPath;
        }

        straightPath[n] = query.CreateLocation(termPos, path[endIndex]);
        straightPathFlags[n] = query.GetPolygonType(path[endIndex]) == NavMeshPolyTypes.OffMeshConnection ? StraightPathFlags.OffMeshConnection : 0;

        return ++n;
    }

    public static PathQueryStatus FindStraightPath(NavMeshQuery query, float3 startPos, float3 endPos, NativeSlice<PolygonId> path, int pathSize, ref NativeArray<NavMeshLocation> straightPath, ref NativeArray<StraightPathFlags> straightPathFlags, ref NativeArray<float> vertexSide, ref int straightPathCount, int maxStraightPath)
    {
        if (!query.IsValid(path[0]))
        {
            straightPath[0] = new NavMeshLocation();
            return PathQueryStatus.Failure;
        }

        straightPath[0] = query.CreateLocation(startPos, path[0]);

        straightPathFlags[0] = StraightPathFlags.Start;

        var apexIndex = 0;
        var n = 1;

        if (pathSize > 1)
        {
            var startPolyWorldToLocal = query.PolygonWorldToLocalMatrix(path[0]);

            var apex = (float3)startPolyWorldToLocal.MultiplyPoint(startPos);
            var left = float3.zero;
            var right = float3.zero;
            var leftIndex = -1;
            var rightIndex = -1;

            for (var i = 1; i <= pathSize; ++i)
            {
                var polyWorldToLocal = query.PolygonWorldToLocalMatrix(path[apexIndex]);

                Vector3 vl, vr;
                if (i == pathSize) vl = vr = polyWorldToLocal.MultiplyPoint(endPos);
                else
                {
                    var success = query.GetPortalPoints(path[i - 1], path[i], out vl, out vr);
                    if (!success) return PathQueryStatus.Failure;

                    vl = polyWorldToLocal.MultiplyPoint(vl);
                    vr = polyWorldToLocal.MultiplyPoint(vr);
                }

                vl = vl - (Vector3)apex;
                vr = vr - (Vector3)apex;

                // Ensure left/right ordering:
                if (Perp2D(vl, vr) < 0) Swap(ref vl, ref vr);

                // Terminate funnel by turning:
                if (Perp2D(left, vr) < 0)
                {
                    var polyLocalToWorld = query.PolygonLocalToWorldMatrix(path[apexIndex]);
                    var termPos = polyLocalToWorld.MultiplyPoint(apex + left);

                    n = RetracePortals(query, apexIndex, leftIndex, path, n, termPos, ref straightPath, ref straightPathFlags, maxStraightPath);
                    if (vertexSide.Length > 0) vertexSide[n - 1] = -1;

                    if (n == maxStraightPath)
                    {
                        straightPathCount = n;
                        return PathQueryStatus.Success;
                    }

                    apex = polyWorldToLocal.MultiplyPoint(termPos);
                    left = float3.zero;
                    right = float3.zero;
                    i = apexIndex = leftIndex;

                    continue;
                }

                if (Perp2D(right, vl) > 0)
                {
                    var polyLocalToWorld = query.PolygonLocalToWorldMatrix(path[apexIndex]);
                    var termPos = polyLocalToWorld.MultiplyPoint(apex + right);

                    n = RetracePortals(query, apexIndex, rightIndex, path, n, termPos, ref straightPath, ref straightPathFlags, maxStraightPath);
                    if (vertexSide.Length > 0) vertexSide[n - 1] = 1;

                    if (n == maxStraightPath)
                    {
                        straightPathCount = n;
                        return PathQueryStatus.Success;
                    }

                    apex = polyWorldToLocal.MultiplyPoint(termPos);
                    left = float3.zero;
                    right = float3.zero;
                    i = apexIndex = rightIndex;

                    continue;
                }

                // Narrow funnel:
                if (Perp2D(left, vl) >= 0)
                {
                    left = vl;
                    leftIndex = i;
                }

                if (Perp2D(right, vr) <= 0)
                {
                    right = vr;
                    rightIndex = i;
                }
            }
        }

        // Remove the the next to last if duplicate point - e.g. start and end positions are the same (in which case we have get a single point):
        if (n > 0 && straightPath[n - 1].position == (Vector3)endPos) --n;

        n = RetracePortals(query, apexIndex, pathSize - 1, path, n, endPos, ref straightPath, ref straightPathFlags, maxStraightPath);
        if (vertexSide.Length > 0) vertexSide[n - 1] = 0;

        if (n == maxStraightPath)
        {
            straightPathCount = n;
            return PathQueryStatus.Success;
        }

        // Fix flag for final path point:
        straightPathFlags[n - 1] = StraightPathFlags.End;

        straightPathCount = n;

        return PathQueryStatus.Success;
    }
}
