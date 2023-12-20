using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics.Systems;
using Unity.Physics.Extensions;
using Unity.Mathematics;
using Unity.Transforms;
using Codice.CM.Client.Differences;
using static UnityEditor.Experimental.GraphView.Port;

namespace ImaginaryReactor {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct WarheadSystem : ISystem
    {
      

        public static void Ignition(Warhead warhead, Entity warheadEntity, Entity targetEntity, ref EntityCommandBuffer ecb,
            ComponentLookup<TriggedWarheadData> FiredWarheadDataLookup, ComponentLookup<LocalToWorld> LTWLookup,
            float3 forcePosition, float3 forceNormal)
        {
            var hasFiredData = FiredWarheadDataLookup.HasComponent(warheadEntity);
            ecb.AddComponent(targetEntity, new Energy()
            {
                SourcePosition = hasFiredData ? FiredWarheadDataLookup[warheadEntity].FiredPosition : 
                LTWLookup[warheadEntity].Position - LTWLookup[warheadEntity].Forward * 100f,
                ForcePosition = forcePosition,
                ForceNormalPhysically = forceNormal* warhead.Fragment.RigidbodyPushForce,
                ForceNormal = hasFiredData ? FiredWarheadDataLookup[warheadEntity].WarheadForward : LTWLookup[warheadEntity].Forward,
                ForceAmount = warhead.Fragment.EnergyAmount
            });
        }

        //private EndSimulationEntityCommandBufferSystem endECBSystem;
        //public ComponentLookup<Warhead> _WarheadLookup;

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

            //foreach (var (delayedWarhead, ltw) in SystemAPI.Query<Warhead, LocalToWorld>()
            //    .WithAll<Warhead,  LocalToWorld>())
            //{

            //}

            //state.Dependency = new WarheadJob()
            var warheadJob = new WarheadCollisionJob
            {
                WarheadLookup = state.GetComponentLookup<Warhead>(false),// _WarheadLookup,
                FiredWarheadDataLookup = state.GetComponentLookup<TriggedWarheadData>(true),
                LTWLookup = state.GetComponentLookup<LocalToWorld>(false),
                MagicIFF_Lookup = state.GetComponentLookup<MagicIFF>(false),
                HitboxLookup = state.GetComponentLookup<Hitbox>(false),
                PhysicsWorldSingleton = physicsWorldSingleton,// PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                ecb = _ecb,
                entityManager = state.EntityManager
            };//.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(),state.Dependency);//.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

            state.Dependency = warheadJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
            state.Dependency.Complete();

        }
        [BurstCompile]
    public partial struct WarheadCollisionJob : ICollisionEventsJob//ITriggerEventsJob
        {
            public ComponentLookup<Warhead> WarheadLookup;
            [ReadOnly]
            public ComponentLookup<TriggedWarheadData> FiredWarheadDataLookup;
            public ComponentLookup<LocalToWorld> LTWLookup;
            public ComponentLookup<MagicIFF> MagicIFF_Lookup;
            public ComponentLookup<Hitbox> HitboxLookup;
            public PhysicsWorldSingleton PhysicsWorldSingleton;
            public EntityCommandBuffer ecb;
            public EntityManager entityManager;

            [BurstCompile]
            public void Execute(/*TriggerEvent*/CollisionEvent collisionEvent)
            {
                //UnityEngine.Debug.Log("Collision Event Invoked");
                var entityA = collisionEvent.EntityA;
                var entityB = collisionEvent.EntityB;

                var isEntityAWarhead = WarheadLookup.HasComponent(entityA);
                var isEntityBWarhead = WarheadLookup.HasComponent(entityB);

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
                    if (WarheadLookup.TryGetComponent(entityA, out Warhead warhead))
                    {
                        if (entityManager.HasBuffer<IgnoreHitboxData>(entityA))
                        {
                            buffer = entityManager.GetBuffer<IgnoreHitboxData>(entityA);

                            for (int i = 0; i < buffer.Length; ++i)
                            {
                                if (buffer[i].hitboxEntity == entityB)
                                {
                                    return;
                                }
                            }
                        }
                        if (warhead.ImpactParticle != Entity.Null)// && LTWLookup.TryGetComponent(entityA, out LocalToWorld ltw))
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
                        if (HitboxLookup.TryGetComponent(entityB, out Hitbox hitbox) && MagicIFF_Lookup.TryGetComponent(entityA, out MagicIFF iff) && hitbox.IFF_Key == iff.Key)
                        {
                            hostile = false;
                        }
                        if (hostile)
                        {

                            Ignition(warhead, entityA, entityB, ref ecb, FiredWarheadDataLookup, LTWLookup,
                                detail.AverageContactPointPosition, -collisionEvent.Normal);


                            //var hasFiredData = FiredWarheadDataLookup.HasComponent(entityA);
                            //ecb.AddComponent(entityB, new Energy()
                            //{
                            //    SourcePosition = hasFiredData ? FiredWarheadDataLookup[entityA].FiredPosition : LTWLookup[entityA].Position - LTWLookup[entityA].Forward * 100f,
                            //    ForcePosition = detail.AverageContactPointPosition,
                            //    ForceNormalPhysically = -collisionEvent.Normal * warhead.FragmentForce,
                            //    ForceNormal = hasFiredData ? FiredWarheadDataLookup[entityA].WarheadForward : LTWLookup[entityA].Forward,
                            //    ForceAmount = warhead.Damage
                            //});
                        }
                    }
                    ecb.DestroyEntity(entityA);
                }
                else if (isEntityBWarhead)
                {
                    //UnityEngine.Debug.Log("B has warhead");
                    if (WarheadLookup.TryGetComponent(entityB, out Warhead warhead))
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

                        if (warhead.ImpactParticle != Entity.Null)// && LTWLookup.TryGetComponent(entityB, out LocalToWorld ltw))
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

                        if (HitboxLookup.TryGetComponent(entityA, out Hitbox hitbox) && MagicIFF_Lookup.TryGetComponent(entityB, out MagicIFF iff) && hitbox.IFF_Key == iff.Key)
                        {
                            hostile = false;
                        }
                        //UnityEngine.Debug.Log(hitbox.IFF_Key + " VS " + iff.Key);

                        if (hostile)
                        {
                            Ignition(warhead, entityB, entityA, ref ecb, FiredWarheadDataLookup, LTWLookup,
                                detail.AverageContactPointPosition, -collisionEvent.Normal);

                           
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