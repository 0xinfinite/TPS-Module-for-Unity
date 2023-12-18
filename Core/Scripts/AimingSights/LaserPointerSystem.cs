using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace ImaginaryReactor { 
//[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
//[UpdateAfter(typeof(TransformSystemGroup))]
//[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
//[UpdateAfter(typeof(AimingSightsSystem))]
public partial  struct LaserPointerSystem : ISystem
{
    //public void OnCreate(ref SystemState state)
    //{
    //}

    //public void OnDestroy(ref SystemState state)
    //{
    //}

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (myLaser, transform) in SystemAPI.Query<PlayerLaserPointer, RefRW<LocalTransform>>())
        {
            if (SystemAPI.HasComponent<AimingSights>(myLaser.Owner))
            {
                var sights = SystemAPI.GetComponent<AimingSights>(myLaser.Owner);
                transform.ValueRW.Position = sights.LaserPointerPosition;
            }
        }

        state.Dependency =
                          new LaserPointerJob()
                          {
                              AimingSightsLookup = SystemAPI.GetComponentLookup<AimingSights>(true),
                          }.Schedule(state.Dependency);

        state.Dependency.Complete();
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct LaserPointerJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<AimingSights> AimingSightsLookup;

            void Execute(
                //Entity entity,
                ref LocalTransform transform,
                in LaserPointer laser
                )
            {
                // if there is a followed entity, place the camera relatively to it
                if (AimingSightsLookup.TryGetComponent(laser.Owner, out AimingSights sights))
                {

                    transform.Position = sights.LaserPointerPosition;


                }
            }
        }
    }
}