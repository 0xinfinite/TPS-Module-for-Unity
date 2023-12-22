using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics.Systems;
using Unity.Physics.Extensions;
using Unity.Mathematics;
using Unity.Transforms;

namespace ImaginaryReactor {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct WarheadSystem : ISystem
    {
      

        public static void Penetrate(Warhead warhead, Entity warheadEntity, Entity targetEntity, ref EntityCommandBuffer ecb,
            ComponentLookup<TriggedWarheadData> FiredWarheadDataLookup, ComponentLookup<LocalToWorld> LTWLookup,
            float3 forcePosition, float3 forceNormal)
        {
            var hasFiredData = FiredWarheadDataLookup.HasComponent(warheadEntity);
            ecb.AddComponent(targetEntity, new Energy()
            {
                SourcePosition = hasFiredData ? FiredWarheadDataLookup[warheadEntity].FiredPosition : 
                LTWLookup[warheadEntity].Position - LTWLookup[warheadEntity].Forward * 100f,
                ForcePosition = forcePosition,
                IsForcePoint = false,
                ForceVector = forceNormal* warhead.Fragment.RigidbodyPushForce,
                ForceNormal = hasFiredData ? FiredWarheadDataLookup[warheadEntity].WarheadForward : LTWLookup[warheadEntity].Forward,
                BaseDamage = warhead.Fragment.EnergyAmount,
                //ApplyCritical = true
                CriticalDamage = warhead.Fragment.EnergyAmount * warhead.Fragment.CriticalMultiply
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
            //var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
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
                PhysicsWorld//Singleton
                = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,   //physicsWorldSingleton,// PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
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
            public PhysicsWorld PhysicsWorld;
            public EntityCommandBuffer ecb;
            public EntityManager entityManager;

            [BurstCompile]
            public void Execute(/*TriggerEvent*/CollisionEvent collisionEvent)
            {
                //UnityEngine.Debug.Log("Collision Event Invoked");
                Entity entityA;
                Entity entityB;

                var isEntityAWarhead = WarheadLookup.HasComponent(collisionEvent.EntityA);
                var isEntityBWarhead = WarheadLookup.HasComponent(collisionEvent.EntityB);

                //if (isEntityAWarhead && isEntityBWarhead) {
                //    ecb.DestroyEntity(collisionEvent.EntityA);
                //    ecb.DestroyEntity(collisionEvent.EntityB);
                //    return;
                //}
                if (isEntityAWarhead)
                {
                    entityA = collisionEvent.EntityA;
                    entityB = collisionEvent.EntityB;
                }
                else
                {
                    entityA = collisionEvent.EntityB;
                    entityB = collisionEvent.EntityA;
                }

                    var detail = collisionEvent.CalculateDetails(ref PhysicsWorld);
                DynamicBuffer<IgnoreHitboxData> buffer;
                //bool ignoreThis = false;
                //if (isEntityAWarhead)
                {
                    if (WarheadLookup.TryGetComponent(entityA, out Warhead warhead))
                    {
                        warhead.BouncedNormal = collisionEvent.Normal * (isEntityAWarhead?1:-1);

                        if (!warhead.DetonateWhenContact)
                            return;

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

                        switch(warhead.Fragment.BulletType) 
                        {
                            case BulletType.Hitscan:
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
                                    Penetrate(warhead, entityA, entityB, ref ecb, FiredWarheadDataLookup, LTWLookup,
                                        detail.AverageContactPointPosition, -warhead.BouncedNormal);//collisionEvent.Normal);


                                //var hasFiredData = FiredWarheadDataLookup.HasComponent(entityA);
                                //ecb.AddComponent(entityB, new Energy()
                                //{
                                //    SourcePosition = hasFiredData ? FiredWarheadDataLookup[entityA].FiredPosition : LTWLookup[entityA].Position - LTWLookup[entityA].Forward * 100f,
                                //    ForcePosition = detail.AverageContactPointPosition,
                                //    ForceVector = -collisionEvent.Normal * warhead.FragmentForce,
                                //    ForceNormal = hasFiredData ? FiredWarheadDataLookup[entityA].WarheadForward : LTWLookup[entityA].Forward,
                                //    BaseDamage = warhead.Damage
                                //});
                            }
                                break;
                            case BulletType.SphericalExplosive:
                                //var ignoreHitboxList = new DynamicBuffer<IgnoreHitboxData>();
                                //Ignition(warhead, 
                                //    ref PhysicsWorld, //ref ignoreHitboxList , 
                                //    LTWLookup[entityA].Position + warhead.BouncedNormal * 0.1f, ref ecb);
                                var ignoreHitboxList = new DynamicBuffer<IgnoreHitboxData>();
                                //Ignition(ref warhead, ref physicsWorld, //ref ignoreHitboxList , 
                                //    ltw.Position, ref ecb);
                                //ecb.RemoveComponent<Warhead>(entity);

                                var entityALTW = LTWLookup[entityA];
                                var warheadPosition = entityALTW.Position;
                                //var warheadRotation = entityALTW.Rotation;
                                var explosionEffect = ecb.Instantiate(warhead.Fragment.ImpactParticleEntity);
                                var collisionNormal = collisionEvent.Normal * (isEntityAWarhead ? 1 : -1);
                                ecb.SetComponent(explosionEffect, new LocalTransform()
                                {
                                    Position = detail.AverageContactPointPosition, //warheadPosition,//rayStart + rayDir * bullet.HitscanRange,
                                    Rotation = collisionNormal.y>0.99f? quaternion.LookRotation(entityALTW.Up*-1, collisionNormal)
                                    : quaternion.LookRotation(math.cross(math.cross(entityALTW.Forward, collisionNormal), collisionNormal), collisionNormal),
                                    //quaternion.LookRotation(sights.MuzzleForward, new float3(0, 1, 0)),//(rayDir, rayDir.y > 0.99f ? transformForward * -1 : new float3(0, 1, 0)),
                                    Scale = 1
                                });
                                        var collector = new SphereCastObstructionHitsCollector(//excludeHitboxes,
                                            warheadPosition, PhysicsWorld, warhead.Fragment.HitscanFilter, warhead.Fragment.SphereRadius,128);
                                        if (PhysicsWorld.SphereCastCustom<SphereCastObstructionHitsCollector>(warheadPosition, warhead.Fragment.SphereRadius, new float3(0, -1, 0), warhead.Fragment.SphereRadius,
                                            ref collector, warhead.Fragment.ShapeFilter, QueryInteraction.IgnoreTriggers))
                                        //(origin, warhead.Fragment.SphereRadius, float3.zero, warhead.Fragment.SphereRadius,
                                        //ref collector , CollisionFilter.Default))
                                        {
                                            //UnityEngine.Debug.Log("Detect Hitbox : "+ collector.NumHits);
                                            for (int i = 0; i < collector.NumHits; ++i)
                                            {
                                                var hit = collector.Hits[i];
                                        //  UnityEngine.Debug.Log("Hitbox : " + hit.Entity.Index);
                                        UnityEngine.Debug.DrawLine(warheadPosition, hit.Position, UnityEngine.Color.red, 3);
                                        ecb.AddComponent(hit.Entity, new Energy()
                                                {
                                                    SourcePosition = warheadPosition,
                                                    ForcePosition = hit.Position,
                                                    ForceNormal = hit.SurfaceNormal,
                                                    //IsForcePoint = true,
                                                    ForceVector = math.normalizesafe( hit.Position-warheadPosition )* warhead.Fragment.RigidbodyPushForce,
                                                    BaseDamage = warhead.Fragment.EnergyAmount,
                                                    //ApplyCritical = false
                                                    CriticalDamage = warhead.Fragment.EnergyAmount
                                                });
                                            }
                                        }
                                        //else
                                        //{
                                        //    UnityEngine.Debug.Log("no hitbox");
                                        //}
                                        collector.Hits.Dispose();
                                      
                                break;
                        }
                        ecb.DestroyEntity(entityA);
                    }
                }
              
            }

         //   [BurstCompile]
         //   public void Ignition(in Warhead warhead, 
         //  ref PhysicsWorld physicsWorld, //ref DynamicBuffer<IgnoreHitboxData> excludeHitboxes,
         //in float3 origin, ref EntityCommandBuffer ecb)
         //   {

         //       switch (warhead.Fragment.BulletType)
         //       {
         //           case BulletType.Hitscan:
         //               break;
         //           case BulletType.Projectile:
         //               break;
         //           case BulletType.SphericalExplosive:

         //               var collector = new SphereCastObstructionHitsCollector(//excludeHitboxes,
         //                   origin, physicsWorld, warhead.Fragment.HitscanFilter, 128);
         //               if (physicsWorld.SphereCastCustom<SphereCastObstructionHitsCollector>(origin, warhead.Fragment.SphereRadius, new float3(0, -1, 0), warhead.Fragment.SphereRadius,
         //                   ref collector, warhead.Fragment.ShapeFilter,QueryInteraction.IgnoreTriggers))
         //               //(origin, warhead.Fragment.SphereRadius, float3.zero, warhead.Fragment.SphereRadius,
         //               //ref collector , CollisionFilter.Default))
         //               {
         //                   //UnityEngine.Debug.Log("Detect Hitbox");
         //                   for (int i = 0; i < collector.NumHits; ++i)
         //                   {
         //                       var hit = collector.Hits[i];
         //                       //  UnityEngine.Debug.Log("Hitbox : " + hit.Entity.Index);

         //                       ecb.SetComponent(hit.Entity, new Energy()
         //                       {
         //                           SourcePosition = hit.Position,
         //                           ForcePosition = hit.Position,
         //                           ForceNormal = hit.SurfaceNormal,
         //                           ForceVector = hit.SurfaceNormal,
         //                           BaseDamage = warhead.Fragment.EnergyAmount
         //                       });
         //                   }
         //               }
         //               //else
         //               //{
         //               //    UnityEngine.Debug.Log("no hitbox");
         //               //}
         //               collector.Hits.Dispose();
         //               break;
         //           case BulletType.ShapeExplosive:
         //               break;
         //       }
         //   }
        }
       
    }
}