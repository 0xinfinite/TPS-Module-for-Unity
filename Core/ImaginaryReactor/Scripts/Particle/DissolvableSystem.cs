using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Burst;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;

namespace ImaginaryReactor { 
    public partial struct DissolvableSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            //state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Dissolvable>().Build());

        }

        public void OnDestroy(ref SystemState state)
        {
        }

        //[ReadOnly] ComponentLookup<AimingSights> sightsLookup;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            //foreach(var (warhead, dissolvable) in SystemAPI.Query<Warhead, Dissolvable>())
            //{

            //}

            DissolveJob job = new DissolveJob()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
                physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                WarheadLookup = SystemAPI.GetComponentLookup<Warhead>()
            };

            state.Dependency = job.Schedule(state.Dependency);
        }



        [BurstCompile]
        //[WithAll(typeof(Simulate))]
        [WithNone(typeof(Warhead))]
     public partial struct DissolveJob : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer ecb;
            public PhysicsWorld physicsWorld;
            public ComponentLookup<Warhead> WarheadLookup;

            void Execute(
                ref Dissolvable dissolvable,
                in LocalToWorld ltw,
                Entity entity
                )
            {
                dissolvable.RemainTime -= DeltaTime;

                if (dissolvable.RemainTime < 0)
                {
                    //if(WarheadLookup.HasComponent(entity))
                    //{
                    //    var warhead = WarheadLookup[entity];

                    //    var explosion = ecb.Instantiate(warhead.Fragment.ImpactParticleEntity);
                    //    ecb.SetComponent(explosion, new LocalTransform()
                    //    {
                    //        Position = ltw.Position,//rayStart + rayDir * bullet.HitscanRange,
                    //        Rotation = ltw.Rotation,//quaternion.LookRotation(sights.MuzzleForward, new float3(0, 1, 0)),//(rayDir, rayDir.y > 0.99f ? transformForward * -1 : new float3(0, 1, 0)),
                    //        Scale = 1
                    //    }
                    //    );

                    //    var ignoreHitboxList = new DynamicBuffer<IgnoreHitboxData>();
                    //    Ignition(ref warhead, ref physicsWorld, //ref ignoreHitboxList , 
                    //        ltw.Position, ref ecb);
                    //    //ecb.RemoveComponent<Warhead>(entity);
                    //}

                    ecb.DestroyEntity(entity);
                }
            }
        }
        //[BurstCompile]
        //public static void Ignition(ref Warhead warhead, ref PhysicsWorld physicsWorld, //ref DynamicBuffer<IgnoreHitboxData> excludeHitboxes,
        //  in float3 origin, ref EntityCommandBuffer ecb)
        //{

        //    switch (warhead.Fragment.BulletType)
        //    {
        //        case BulletType.Hitscan:
        //            break;
        //        case BulletType.Projectile:
        //            break;
        //        case BulletType.SphericalExplosive:

        //            var collector = new SphereCastObstructionHitsCollector(//excludeHitboxes,
        //                origin, physicsWorld, warhead.Fragment.HitscanFilter, 128);
        //            if (physicsWorld.SphereCastCustom<SphereCastObstructionHitsCollector>(origin, warhead.Fragment.SphereRadius, new float3(0, -1, 0), warhead.Fragment.SphereRadius,
        //                ref collector, warhead.Fragment.ShapeFilter))
        //            //(origin, warhead.Fragment.SphereRadius, float3.zero, warhead.Fragment.SphereRadius,
        //            //ref collector , CollisionFilter.Default))
        //            {
        //                //UnityEngine.Debug.Log("Detect Hitbox");
        //                for (int i = 0; i < collector.NumHits; ++i)
        //                {
        //                    var hit = collector.Hits[i];
        //                  //  UnityEngine.Debug.Log("Hitbox : " + hit.Entity.Index);

        //                    ecb.SetComponent(hit.Entity, new Energy()
        //                    {
        //                        SourcePosition = hit.Position,
        //                        ForcePosition = hit.Position,
        //                        ForceNormal = hit.SurfaceNormal,
        //                        ForceVector = hit.SurfaceNormal,
        //                        BaseDamage = warhead.Fragment.EnergyAmount
        //                    });
        //                }
        //            }
        //            //else
        //            //{
        //            //    UnityEngine.Debug.Log("no hitbox");
        //            //}
        //            collector.Hits.Dispose();
        //            break;
        //        case BulletType.ShapeExplosive:
        //            break;
        //    }
        //}
    }
}