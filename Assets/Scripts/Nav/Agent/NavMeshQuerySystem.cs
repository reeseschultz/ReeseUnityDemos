using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
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

                UnsafeUtility.CopyStructureToPtr(ref query, pointerArray[i].Value);
            }

            PointerArray = new NativeArray<NavMeshQueryPointer>(pointerArray, Allocator.Persistent);

            Debug.Log("Expect an error to be thrown about native collections not being disposed, specifically regarding NavMeshQueries allocated in the NavMeshQuerySystem. The error *appears* to be incorrect because the built-in DisposeSentinel cannot ascertain that the collections are actually disposed later via pointers to them, which is clear in NavMeshQuerySystem.OnDestroy.");
        }

        protected override void OnDestroy()
        {
            for (int i = 0; i < PointerArray.Length; ++i)
            {
                UnsafeUtility.CopyPtrToStructure(PointerArray[i].Value, out NavMeshQuery query);
                query.Dispose();
                UnsafeUtility.Free(PointerArray[i].Value, Allocator.Persistent);
            }

            PointerArray.Dispose();
        }

        protected override void OnUpdate() { }
    }
}
