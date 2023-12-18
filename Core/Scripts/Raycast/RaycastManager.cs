using Unity.Collections;
using Unity.Jobs;
using Unity.Physics;
//using UnityEngine.Rendering;
//using UnityEngine;

//using UnityEngine;

namespace ImaginaryReactor { public class RaycastManager
    {
        public struct RaycastJob : IJobParallelForBatch
        {
            [ReadOnly] public CollisionWorld collisionWorld;
            [ReadOnly] public NativeArray<RaycastInput> inputs;

            public NativeArray<RaycastHit> results;

            public void Execute(int index, int count)
            {
                collisionWorld.CastRay(inputs[index], out var hit);
                results[index] = hit;
            }
        }

        public static JobHandle ScheduleRaycast(CollisionWorld _collisionWorld, NativeArray<RaycastInput> _inputs, NativeArray<RaycastHit> _hits)
        {
            var raycastJob = new RaycastJob()
            {
                collisionWorld = _collisionWorld,
                inputs = _inputs,
                results = _hits
            };

            JobHandle jobHandle = raycastJob.ScheduleBatch(_inputs.Length, 1);
            return jobHandle;
        }

        public static void SingleRaycast(CollisionWorld _collisionWorld, RaycastInput _input, ref RaycastHit _hit)
        {
            var raycastInput = new NativeArray<RaycastInput>(1, Allocator.TempJob);
            var raycastResult = new NativeArray<RaycastHit>(1, Allocator.TempJob);

            raycastInput[0] = _input;

            var jobHandle = ScheduleRaycast(_collisionWorld, raycastInput, raycastResult);
            jobHandle.Complete();

            _hit = raycastResult[0];

            raycastInput.Dispose();
            raycastResult.Dispose();
        }

        public static void SingleRaycast(PhysicsWorld _physicsWorld, RaycastInput _input, ref RaycastHit _hit)
        {
            var raycastInput = new NativeArray<RaycastInput>(1, Allocator.TempJob);
            var raycastResult = new NativeArray<RaycastHit>(1, Allocator.TempJob);

            raycastInput[0] = _input;

            var jobHandle = ScheduleRaycast(_physicsWorld.CollisionWorld, raycastInput, raycastResult);
            jobHandle.Complete();

            _hit = raycastResult[0];

            raycastInput.Dispose();
            raycastResult.Dispose();
        }
    }
     public class FilterConversion
        {
    public static CollisionFilter LayerMaskToFilter(UnityEngine.LayerMask mask)
        {
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = (uint)mask.value,
                CollidesWith = (uint)mask.value
            };
            return filter;
        }

        //public static CollisionFilter LayerToFilter(int layer)
        //{
        //    if (layer == -1)
        //    {
        //        return CollisionFilter.Zero;
        //    }

        //    BitArray32 mask = new BitArray32();
        //    mask[layer] = true;

        //    CollisionFilter filter = new CollisionFilter()
        //    {
        //        BelongsTo = mask.Bits,
        //        CollidesWith = mask.Bits
        //    };
        //    return filter;
        //}

    } }