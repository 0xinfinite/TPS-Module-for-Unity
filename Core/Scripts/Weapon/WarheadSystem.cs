using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics.Systems;
using Unity.Physics.Extensions;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;
using Unity.Transforms;

namespace ImaginaryReactor {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct WarheadSystem : ISystem
    {

        //private EndSimulationEntityCommandBufferSystem endECBSystem;
        //public ComponentLookup<Warhead> _WarheadLookUp;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();

        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var _ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);//state.World.GetOrCreateSystemManaged<BeginSimulationEntityCommandBufferSystem>().CreateCommandBuffer();


            //state.Dependency = new WarheadJob()
            var warheadJob = new WarheadJob
            {
                WarheadLookUp = state.GetComponentLookup<Warhead>(false),// _WarheadLookUp,
                FiredWarheadDataLookUp = state.GetComponentLookup<TriggedWarheadData>(true),
                LTWLookUp = state.GetComponentLookup<LocalToWorld>(false),
                MagicIFF_LookUp = state.GetComponentLookup<MagicIFF>(false),
                HitboxLookUp = state.GetComponentLookup<Hitbox>(false),
                PhysicsWorldSingleton = physicsWorldSingleton,// PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                ecb = _ecb,
                entityManager = state.EntityManager
            };//.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(),state.Dependency);//.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

            state.Dependency = warheadJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
            state.Dependency.Complete();

        }
        [BurstCompile]
    public partial struct WarheadJob : ICollisionEventsJob//ITriggerEventsJob
        {
            public ComponentLookup<Warhead> WarheadLookUp;
            [ReadOnly]
            public ComponentLookup<TriggedWarheadData> FiredWarheadDataLookUp;
            public ComponentLookup<LocalToWorld> LTWLookUp;
            public ComponentLookup<MagicIFF> MagicIFF_LookUp;
            public ComponentLookup<Hitbox> HitboxLookUp;
            /*[ReadOnly] */
            //public BuildPhysicsWorld BuildPhysilcsWorld;
            //public PhysicsWorld PhysicsWorld;
            /*[ReadOnly]*/
            public PhysicsWorldSingleton PhysicsWorldSingleton;
            public EntityCommandBuffer ecb;
            public EntityManager entityManager;

            [BurstCompile]
            public void Execute(/*TriggerEvent*/CollisionEvent collisionEvent)
            {
                //UnityEngine.Debug.Log("Collision Event Invoked");
                var entityA = collisionEvent.EntityA;
                var entityB = collisionEvent.EntityB;

                var isEntityAWarhead = WarheadLookUp.HasComponent(entityA);
                var isEntityBWarhead = WarheadLookUp.HasComponent(entityB);

                if (isEntityAWarhead && isEntityBWarhead) {
                    ecb.DestroyEntity(entityA);
                    ecb.DestroyEntity(entityB);
                    return;
                }

                var detail = collisionEvent.CalculateDetails(ref PhysicsWorldSingleton.PhysicsWorld);
                DynamicBuffer<IgnoreHitboxData> buffer;
                //bool ignoreThis = false;
                if (isEntityAWarhead)
                {
                    if (WarheadLookUp.TryGetComponent(entityA, out Warhead warhead))
                    {
                        if (entityManager.HasBuffer<IgnoreHitboxData>(entityA))
                        {
                            buffer = entityManager.GetBuffer<IgnoreHitboxData>(entityA);

                            for (int i = 0; i < buffer.Length; ++i)
                            {
                                if (buffer[i].hitboxEntity == entityB)
                                {
                                    return;
                                    //ignoreThis = true;
                                    //break;
                                }
                            }
                        }
                        //if (ignoreThis)
                        //{
                        //    return;
                        //}

                        if (warhead.ImpactParticle != Entity.Null)// && LTWLookUp.TryGetComponent(entityA, out LocalToWorld ltw))
                        {
                            Entity impactParticle = ecb.Instantiate(warhead.ImpactParticle);
                            ecb.SetComponent(impactParticle, new LocalTransform()
                            {
                                Position = detail.AverageContactPointPosition,//ltw.Position,//hit.Position,
                                Rotation = quaternion.LookRotation(collisionEvent.Normal, new float3(0, 1, 0)),//ltw.Rotation,
                                Scale = 1
                            }
                            );
                        }

                        bool hostile = true;

                        if (HitboxLookUp.TryGetComponent(entityB, out Hitbox hitbox) && MagicIFF_LookUp.TryGetComponent(entityA, out MagicIFF iff) && hitbox.IFF_Key == iff.Key)
                        {
                            hostile = false;
                        }

                        //UnityEngine.Debug.Log(hitbox.IFF_Key + " VS " + iff.Key);

                        if (hostile)
                        {
                            var hasFiredData = FiredWarheadDataLookUp.HasComponent(entityA);
                            ecb.AddComponent(entityB, new Energy()
                            {
                                SourcePosition = hasFiredData ? FiredWarheadDataLookUp[entityA].FiredPosition : LTWLookUp[entityA].Position - LTWLookUp[entityA].Forward * 100f,
                                ForcePosition = detail.AverageContactPointPosition,
                                ForceNormalPhysically = -collisionEvent.Normal * warhead.FragmentForce,
                                ForceNormal = hasFiredData ? FiredWarheadDataLookUp[entityA].WarheadForward : LTWLookUp[entityA].Forward,
                                ForceAmount = warhead.FragmentDamage
                            });
                        }
                        //PhysicsWorldSingleton.PhysicsWorld.ApplyImpulse(PhysicsWorldSingleton.PhysicsWorld.GetRigidBodyIndex(entityB),
                        //-collisionEvent.Normal * detail.EstimatedImpulse,  detail.AverageContactPointPosition);
                    }
                    //UnityEngine.Debug.Log("A has warhead");
                    ecb.DestroyEntity(entityA);
                }
                else if (isEntityBWarhead)
                {
                    //UnityEngine.Debug.Log("B has warhead");
                    if (WarheadLookUp.TryGetComponent(entityB, out Warhead warhead))
                    {
                        if (entityManager.HasBuffer<IgnoreHitboxData>(entityB))
                        {
                            buffer = entityManager.GetBuffer<IgnoreHitboxData>(entityB);

                            for (int i = 0; i < buffer.Length; ++i)
                            {
                                if (buffer[i].hitboxEntity == entityA)
                                {
                                    return;
                                    //ignoreThis = true;
                                    //break;
                                }
                            }
                        }
                        //if (ignoreThis)
                        //{
                        //    return;
                        //}

                        if (warhead.ImpactParticle != Entity.Null)// && LTWLookUp.TryGetComponent(entityB, out LocalToWorld ltw))
                        {
                            Entity impactParticle = ecb.Instantiate(warhead.ImpactParticle);
                            ecb.SetComponent(impactParticle, new LocalTransform()
                            {
                                Position = detail.AverageContactPointPosition,//ltw.Position,//hit.Position,
                                Rotation = quaternion.LookRotation(collisionEvent.Normal, new float3(0, 1, 0)),//ltw.Rotation,
                                Scale = 1
                            }
                            );
                        }

                        bool hostile = true;

                        if (HitboxLookUp.TryGetComponent(entityA, out Hitbox hitbox) && MagicIFF_LookUp.TryGetComponent(entityB, out MagicIFF iff) && hitbox.IFF_Key == iff.Key)
                        {
                            hostile = false;
                        }
                        //UnityEngine.Debug.Log(hitbox.IFF_Key + " VS " + iff.Key);

                        if (hostile)
                        {
                            var hasFiredData = FiredWarheadDataLookUp.HasComponent(entityB);
                            ecb.AddComponent(entityA, new Energy()
                            {
                                SourcePosition = hasFiredData ? FiredWarheadDataLookUp[entityB].FiredPosition : LTWLookUp[entityB].Position - LTWLookUp[entityB].Forward * 100f,
                                ForcePosition = detail.AverageContactPointPosition,
                                ForceNormalPhysically = -collisionEvent.Normal * warhead.FragmentForce,
                                ForceNormal = hasFiredData ? FiredWarheadDataLookUp[entityB].WarheadForward : LTWLookUp[entityB].Forward,
                                ForceAmount = warhead.FragmentDamage
                            });
                        }
                        //PhysicsWorldSingleton.PhysicsWorld.ApplyImpulse(PhysicsWorldSingleton.PhysicsWorld.GetRigidBodyIndex(entityA),
                        //    -collisionEvent.Normal * detail.EstimatedImpulse,  detail.AverageContactPointPosition);
                    }
                    //PhysicsWorldSingleton.PhysicsWorld.ApplyImpulse(PhysicsWorldSingleton.PhysicsWorld.GetRigidBodyIndex(entityA),
                    //    -collisionEvent.Normal * detail.EstimatedImpulse, detail.AverageContactPointPosition);
                    ecb.DestroyEntity(entityB);
                }

            }
        }
    }
}