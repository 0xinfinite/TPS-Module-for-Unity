using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Physics;
using Unity.Transforms;

public partial struct DissolvableSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Dissolvable>().Build());

    }

    public void OnDestroy(ref SystemState state)
    {
    }

    //[ReadOnly] ComponentLookup<AimingSights> sightsLookup;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        DissolveJob job = new DissolveJob()
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged)
        };

        state.Dependency = job.Schedule(state.Dependency);
    }



    [BurstCompile]
    //[WithAll(typeof(Simulate))]
    public partial struct DissolveJob : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer ecb;

        void Execute(
            Entity entity,
            ref Dissolvable dissolvable
            )
        {
            dissolvable.RemainTime -= DeltaTime;

            if (dissolvable.RemainTime < 0)
            {
                ecb.DestroyEntity(entity); 
            }
        }
    }
}
