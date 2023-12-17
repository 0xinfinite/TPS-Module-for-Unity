using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;

[UpdateInGroup(typeof(PresentationSystemGroup))]
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class AimingQueueSystem : SystemBase
{
    //NativeArray<float3> position;

    protected override void OnUpdate()
    {
        //if (AimingQueueManager.instance)
        //{
        //    AimingQueueManager _queueManager = AimingQueueManager.instance;

        //    NativeArray<int> _id = new NativeArray<int>(1, Allocator.TempJob);
        //    NativeArray<float3> _position = new NativeArray<float3>(1, Allocator.TempJob);

        //    TrackingProcessJob processjob = new TrackingProcessJob()
        //    {
        //        ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged),
        //        id = _id,
        //        position = _position//queueManager = _queueManager
        //    };
        //    Dependency = processjob.Schedule(Dependency);

        //    Dependency.Complete();

        //    _queueManager.Enqueue(_id[0], _position[0],15);

        //    //Debug.Log(_queueManager.Dequeue(_id[0], 15));
        //    //_position[0] = _queueManager.Dequeue(_id[0], 15);

        //    foreach(var(brain, entity) in SystemAPI.Query<RefRW<Brain>>().WithEntityAccess())
        //    {
        //        if(brain.ValueRO.TargetFound && entity.Index == _id[0])
        //        {
        //            float3 dequeuedPos = _queueManager.Dequeue(_id[0], 15);
        //            Debug.Log(math.lengthsq(dequeuedPos - new float3(-100, -100, -100)));
        //            brain.ValueRW.FinalDesirePositionToLook = math.lengthsq(dequeuedPos - new float3(-100, -100, -100)) > 0.01f ?
        //            dequeuedPos : brain.ValueRW.FinalDesirePositionToLook;
        //        }
        //    }

        //    //TrackingJob job = new TrackingJob()
        //    //{
        //    //    id = _id,
        //    //    position = _position
        //    //};
        //    //Dependency = job.Schedule(Dependency);

        //    //Dependency.Complete();

        //    _id.Dispose();
        //    _position.Dispose();
        //}
    }

    //[BurstCompile]
    //public partial struct TrackingProcessJob : IJobEntity
    //{
    //    public EntityCommandBuffer ecb;
    //    public NativeArray<int> id;
    //    public NativeArray<float3> position;

    //    void Execute(
    //        Entity entity,
    //        in TrackInfo info)
    //    {
    //        //queueManager.Enqueue(entity.Index, info.Position);
    //        id[0] = entity.Index;
    //        position[0] = info.LastKnownVector;

    //        ecb.RemoveComponent<TrackInfo>(entity);
    //    }

    //}

    //public partial struct TrackingJob : IJobEntity
    //{
    //    public NativeArray<int> id;
    //    public NativeArray<float3> position;

    //    void Execute(
    //        Entity entity,
    //        ref Brain brain
    //        )
    //    {
    //        //queueManager.Enqueue(entity.Index, info.Position);
    //        if(entity.Index == id[0])
    //        {
    //            brain.FinalDesirePositionToLook = math.lengthsq(position[0] - new float3(-100f, -100f, -100f)) > 0.01f ?
    //                position[0] : brain.FinalDesirePositionToLook;
    //        }

    //    }

    //}

}