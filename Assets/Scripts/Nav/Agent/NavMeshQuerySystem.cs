using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Experimental.AI;

namespace Reese.Nav
{
    unsafe struct NavMeshQueryPointer
    {
        [NativeDisableUnsafePtrRestriction]
        public void* Value;
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    unsafe class NavMeshQuerySystem : ComponentSystem
    {
        public NativeArray<NavMeshQueryPointer> PointerArray { get; private set; }

        // Must dispose queries from the below collection, *not* via pointer
        // from the PointerArray. Otherwise, the DisposeSentinel will
        // mistakenly report that the queries aren't being disposed, when
        // they actually *are*.
        List<NavMeshQuery> queryList = new List<NavMeshQuery>();

        protected override void OnCreate()
        {
            var pointerArray = new NavMeshQueryPointer[JobsUtility.MaxJobThreadCount];

            for (int i = 0; i < JobsUtility.MaxJobThreadCount; ++i)
            {
                pointerArray[i] = new NavMeshQueryPointer
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
                    NavConstants.PATH_NODE_MAX
                );

                queryList.Add(query);

                UnsafeUtility.CopyStructureToPtr(ref query, pointerArray[i].Value);
            }

            PointerArray = new NativeArray<NavMeshQueryPointer>(pointerArray, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            queryList.ForEach(query =>
            {
                query.Dispose();
            });

            PointerArray.Dispose();
        }

        protected override void OnUpdate() { }
    }
}
