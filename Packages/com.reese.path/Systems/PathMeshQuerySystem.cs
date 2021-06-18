﻿using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Experimental.AI;

namespace Reese.Path
{
    /// <summary>This struct is intended to hold a pointer to a NavMeshQuery.
    /// It's annotated with [NativeDisableUnsafePtrRestriction] so any job that
    /// interfaces with said pointer does not complain.</summary>
    unsafe public struct PathMeshQueryPointer
    {
        [NativeDisableUnsafePtrRestriction]
        public void* Value;
    }

    /// <summary>This system exists because the NavMeshQuery type is inherently
    /// evil. It's a NativeContainer, so it can't be placed in another such as
    /// a NativeArray. You can't instantiate one within a job using
    /// Allocator.Temp because the default NavMeshWorld is needed to create it,
    /// which includes unsafe code lacking [NativeDisableUnsafePtrRestriction],
    /// meaning that's a no-go inside a job. So, long story short, this system
    /// hacks the queries into a public NativeArray via pointers to them. The
    /// Path PlanSystem can then access them via the UnsafeUtility. It would be
    /// overkill to include a pointer in each NavAgent, since the number of
    /// threads is limited at any given time anyway, so the solution here is
    /// similar to what you'll find in Reese.Random.RandomSystem, which was
    /// inspired by how the PhysicsWorld exposes a NativeSlice of bodies.
    /// But here we index each query by native thread, taking thread
    /// safety into our own hands.</summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    unsafe public class PathMeshQuerySystem : ComponentSystem
    {
        PathSystem pathSystem => World.GetOrCreateSystem<PathSystem>();

        /// <summary>An array of structs containing pointers, each referencing
        /// its own respective NavMeshQuery.</summary>
        internal NativeArray<PathMeshQueryPointer> PointerArray { get; private set; }

        /// <summary>This list exists because the queries must be disposed from
        /// it, *not* via pointer from the PointerArray, because otherwise the
        /// DisposeSentinel will mistakenly report that the queries aren't being
        /// disposed, when they actually *are*.</summary>
        List<NavMeshQuery> queryList = new List<NavMeshQuery>();

        protected override void OnCreate()
        {
            var settings = pathSystem.Settings;
            var pointerArray = new PathMeshQueryPointer[JobsUtility.MaxJobThreadCount];

            for (var i = 0; i < JobsUtility.MaxJobThreadCount; ++i)
            {
                pointerArray[i] = new PathMeshQueryPointer
                {
                    Value = UnsafeUtility.Malloc(
                        UnsafeUtility.SizeOf<NavMeshQuery>(),
                        UnsafeUtility.AlignOf<NavMeshQuery>(),
                        Allocator.Persistent
                    )
                };

                var query = new NavMeshQuery(
                    NavMeshWorld.GetDefaultWorld(),
                    Allocator.Persistent,
                    settings.PathMeshQueryNodeMax
                );

                queryList.Add(query);

                UnsafeUtility.CopyStructureToPtr(ref query, pointerArray[i].Value);
            }

            PointerArray = new NativeArray<PathMeshQueryPointer>(pointerArray, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            queryList.ForEach(query => query.Dispose());

            foreach (var pointer in PointerArray) UnsafeUtility.Free(pointer.Value, Allocator.Persistent);

            PointerArray.Dispose();
        }

        protected override void OnUpdate() { }
    }
}
