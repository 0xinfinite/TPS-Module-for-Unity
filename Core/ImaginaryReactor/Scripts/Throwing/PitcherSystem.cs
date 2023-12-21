using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

namespace ImaginaryReactor
{
    //[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
    public partial struct PitcherSystem : ISystem
    {



        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        //[ReadOnly] ComponentLookup<AimingSights> sightsLookup;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var _ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            //foreach (var(pitcher, cc, sights) in SystemAPI.Query<Pitcher, ThirdPersonCharacterControl, AimingSights>()
            //    .WithAll<Pitcher, ThirdPersonCharacterControl, AimingSights>())
            //{
            //    if (cc.ThrowPress)
            //    {
            //        Pitcher p = pitcher;

            //        pitcher = p;
            //    }
            //    //if (cc.ThrowRelease)
            //    //{
            //    //    UnityEngine.Debug.Log("Release Granade");
            //    //    _ecb.RemoveComponent<Parent>(pitcher.ValueRW.CurrentGrabEntity);
            //    //    _ecb.SetComponent(pitcher.ValueRW.CurrentGrabEntity, new PhysicsVelocity()
            //    //    {
            //    //        Linear = sights.MuzzleForward * pitcher.ValueRW.ThrowingPower
            //    //    });
            //    //}
            //}
            //foreach (var (throwable, entity) in SystemAPI.Query<Throwable>().WithEntityAccess())
            //{
            //    var pitcherLookup = SystemAPI.GetComponentLookup<Pitcher>();
            //    if (pitcherLookup.HasComponent(throwable.Owner))
            //    {
            //        Pitcher pitcher = pitcherLookup[throwable.Owner];
            //        pitcher.CurrentGrabEntity = entity;
            //        pitcherLookup[throwable.Owner] = pitcher;

            //        _ecb.RemoveComponent<Throwable>(entity);
            //    }
            //}

            //var entityManager = state.EntityManager;

            //foreach (var ( pitcher, cc,sights,entity) in SystemAPI.Query< RefRW<Pitcher> ,  ThirdPersonCharacterControl ,  AimingSights > ().WithEntityAccess())
            //{
            //    if (cc.ThrowPress)
            //    {


            //        var throwable = _ecb.Instantiate(pitcher.ValueRW.GrabEntity);
            //        //UnityEngine.Debug.Log("Cook Start : " + throwable.Index);
            //        //pitcher.CurrentGrabEntity = throwable;
            //        _ecb.AddComponent(throwable, new Parent() { Value = pitcher.ValueRW.Hand });
            //        _ecb.AddComponent(throwable, new Throwable() { Owner = entity });
            //        _ecb.AddComponent(throwable, new PreviousParent() { Value = pitcher.ValueRW.Hand });
            //        var handLTW = SystemAPI.GetComponentLookup<LocalToWorld>(); //ltwLookup[pitcher.Hand];
            //        _ecb.SetComponent(throwable, new LocalTransform()
            //        {
            //            Position = float3.zero //sights.MuzzlePosition
            //                                                                      ,
            //            Rotation = quaternion.LookRotationSafe(sights.MuzzleForward, new float3(0, 1, 0))
            //        });
            //        _ecb.SetComponent(throwable, new LocalToWorld() { Value = new float4x4(quaternion.LookRotationSafe(sights.MuzzleForward, new float3(0, 1, 0)),
            //            /*sights.MuzzlePosition*/ handLTW[pitcher.ValueRW.Hand].Position) });

            //        //entityManager.AddComponentData(throwable, new Parent() { Value = pitcher.ValueRW.Hand });
            //        //entityManager.AddComponentData(throwable, new Throwable() { Owner = entity });
            //        //entityManager.AddComponentData(throwable, new LocalTransform()
            //        //{
            //        //    Position = float3.zero //sights.MuzzlePosition
            //        //                                                              ,
            //        //    Rotation = quaternion.LookRotationSafe(sights.MuzzleForward, new float3(0, 1, 0))
            //        //});
            //        //entityManager.AddComponentData(throwable, new LocalToWorld() { Value = new float4x4(quaternion.LookRotationSafe(sights.MuzzleForward, new float3(0, 1, 0)), sights.MuzzlePosition) });

            //        //ecb.AddComponent(throwable)
            //        //ecb.SetComponent(pitcher.CurrentGrabEntity, new PhysicsVelocity()
            //        //{
            //        //    Linear = sights.MuzzleForward * pitcher.ThrowingPower
            //        //});
            //    }
            //    if (cc.ThrowRelease)
            //    {
            //        if (pitcher.ValueRW.CurrentGrabEntity != Entity.Null)
            //        {
            //            _ecb.RemoveComponent<Parent>(pitcher.ValueRW.CurrentGrabEntity);
            //            _ecb.AddComponent(pitcher.ValueRW.CurrentGrabEntity, new PhysicsVelocity()
            //            {
            //                Linear = sights.MuzzleForward * pitcher.ValueRW.ThrowingPower
            //            });
            //            _ecb.AddComponent(pitcher.ValueRW.CurrentGrabEntity, new PhysicsGravityFactor()
            //            {
            //            });
            //        }
            //    }
            //}

            PitcherJob job = new PitcherJob()
            {
                ecb = _ecb,
                //entityManager = state.EntityManager,
                ltwLookup = SystemAPI.GetComponentLookup<LocalToWorld>(false),
            };
            state.Dependency = job.Schedule(state.Dependency);

            //state.Dependency.Complete();


        }
    }

    [BurstCompile]
    public partial struct PitcherJob:IJobEntity
    {
        public EntityCommandBuffer ecb;
        public ComponentLookup<LocalToWorld> ltwLookup;
        //public EntityManager entityManager;

        [BurstCompile]
        void Execute(ref Pitcher pitcher, ref ThirdPersonCharacterControl cc, in AimingSights sights, Entity entity)
        {
            if (cc.ThrowPress)
            {
                //var throwable = ecb.Instantiate(pitcher.GrabEntity);
                //UnityEngine.Debug.Log("Cook Start : " + throwable.Index);
                //pitcher.CurrentGrabEntity = throwable;
                //ecb.AddComponent(throwable, new Parent() { Value = pitcher.Hand });
                //ecb.AddComponent(throwable, new Throwable() { Owner = entity });
                //ecb.AddComponent(throwable, new PreviousParent() { Value = pitcher.Hand});
                ////var ltw = ltwLookup[pitcher.Hand];
                //ecb.AddComponent(throwable, new LocalTransform() { Position = float3.zero //sights.MuzzlePosition
                //                                                              , Rotation = quaternion.LookRotationSafe(sights.MuzzleForward, new float3(0, 1, 0)) });
                //ecb.AddComponent(throwable, new LocalToWorld() { Value = new float4x4(quaternion.LookRotationSafe(sights.MuzzleForward, new float3(0, 1, 0)), /*sights.MuzzlePosition*/ ltw.Position) });

                //entityManager.AddComponentData(throwable, new Parent() { Value = pitcher.Hand });
                //entityManager.AddComponentData(throwable, new Throwable() { Owner = entity });
                //entityManager.AddComponentData(throwable, new LocalTransform()
                //{
                //    Position = float3.zero //sights.MuzzlePosition
                //                                                              ,
                //    Rotation = quaternion.LookRotationSafe(sights.MuzzleForward, new float3(0, 1, 0))
                //});
                //entityManager.AddComponentData(throwable, new LocalToWorld() { Value = new float4x4(quaternion.LookRotationSafe(sights.MuzzleForward, new float3(0, 1, 0)), sights.MuzzlePosition) });

                //ecb.AddComponent(throwable)
                //ecb.SetComponent(pitcher.CurrentGrabEntity, new PhysicsVelocity()
                //{
                //    Linear = sights.MuzzleForward * pitcher.ThrowingPower
                //});
            }
            if (cc.ThrowRelease)
            {
                cc.ThrowRelease = false;
                Entity granade = ecb.Instantiate(pitcher.GrabEntity);

                ecb.SetComponent(granade , new LocalTransform()
                {
                    Position = sights.MuzzlePosition,//rayStart + rayDir * bullet.HitscanRange,
                    Rotation = quaternion.LookRotation(sights.MuzzleForward,new float3(0,1,0)),//(rayDir, rayDir.y > 0.99f ? transformForward * -1 : new float3(0, 1, 0)),
                    Scale = 1
                }
                );
                ecb.SetComponent(granade , new PhysicsVelocity() { Linear = sights.MuzzleForward * pitcher.ThrowingPower});
                //if (bullet.MagicIFF_Key != ColliderKey.Empty)
                //{
                //    ecb.AddComponent(granade , new MagicIFF() { Key = bullet.MagicIFF_Key });
                //}
                //ecb.AddComponent(granade , new TriggedWarheadData()
                //{
                //    FiredPosition = rayStart,
                //    WarheadForward = rayDir,
                //    WarheadBaseDamage = bullet.EnergyAmount
                //});
                //if (pitcher.CurrentGrabEntity != Entity.Null)
                //{
                //    ecb.RemoveComponent<Parent>(pitcher.CurrentGrabEntity);
                //    ecb.AddComponent(pitcher.CurrentGrabEntity, new PhysicsVelocity()
                //    {
                //        Linear = sights.MuzzleForward * pitcher.ThrowingPower
                //    });
                //    ecb.AddComponent(pitcher.CurrentGrabEntity, new PhysicsGravityFactor()
                //    {
                //    });
                //}
            }
        }
    }
}
