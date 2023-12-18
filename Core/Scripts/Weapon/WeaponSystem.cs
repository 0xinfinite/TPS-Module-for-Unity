using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.CharacterController;


namespace ImaginaryReactor
{
    //using UnityEngine;

    //[UpdateInGroup(typeof(SimulationSystemGroup))]
    [BurstCompile]
public struct RayCastObstructionHitsCollector : ICollector<RaycastHit>
{
    public bool EarlyOutOnFirstHit => false;
    public float MaxFraction => 1f;
    public int NumHits { get; private set; }

    public RaycastHit ClosestHit;

    private float _closestHitFraction;
    private float3 _bulletDirection;
    //private Entity _followedCharacter;
    private DynamicBuffer<IgnoreHitboxData> _ignoredEntitiesBuffer;

    public RayCastObstructionHitsCollector(//Entity followedCharacter,
        DynamicBuffer<IgnoreHitboxData> ignoredEntitiesBuffer, float3 bulletDirection)
    {
        NumHits = 0;
        ClosestHit = default;

        _closestHitFraction = float.MaxValue;
        _bulletDirection = bulletDirection;
        //_followedCharacter = followedCharacter;
        _ignoredEntitiesBuffer = ignoredEntitiesBuffer;
    }

    [BurstCompile]
    public bool AddHit(RaycastHit hit)
    {
        //if (_followedCharacter == hit.Entity)
        //{
        //    return false;
        //}

        if (/*math.dot(hit.SurfaceNormal, _bulletDirection) < 0f ||*/ !PhysicsUtilities.IsCollidable(hit.Material))
        {
            return false;
        }

        for (int i = 0; i < _ignoredEntitiesBuffer.Length; i++)
        {
            if (_ignoredEntitiesBuffer[i].hitboxEntity == hit.Entity)
            {
                return false;
            }
        }

        // Process valid hit
        if (hit.Fraction < _closestHitFraction)
        {
            _closestHitFraction = hit.Fraction;
            ClosestHit = hit;
        }
        NumHits++;

        return true;
    }
}



[BurstCompile]
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(FixedTickSystem))]
    //[UpdateAfter(typeof(AfterPhysicsSystemGroup))]
    //[UpdateAfter(typeof(AfterPhysicsSystemGroup))]
    //[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
     public partial struct WeaponSystem : ISystem
        {


            [BurstCompile]
            public void OnCreate(ref SystemState state)
            {
                state.RequireForUpdate<PhysicsWorldSingleton>();
                state.RequireForUpdate<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
                //state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Weapon>().Build());

            }

            public void OnDestroy(ref SystemState state)
            {
            }

            //[ReadOnly] ComponentLookup<AimingSights> sightsLookup;

            [BurstCompile]
            public void OnUpdate(ref SystemState state)
            {
                //float DeltaTime = SystemAPI.Time.DeltaTime;
                //PhysicsWorld PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
                //var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                ////sightsLookup = SystemAPI.GetComponentLookup<AimingSights>(true);

                //foreach (var weapon in SystemAPI.Query<Weapon>().WithAll<Weapon>())
                //{
                //    //if (sightsLookup.TryGetComponent(weapon.AimingSightsEntity, out AimingSights sights))
                //    {
                //        if (weapon.IsFired)
                //        {
                //            int bulletCount = weapon.BulletCount;
                //            float3 rayStart = weapon.MuzzlePosition;//sights.MuzzlePosition;
                //            float3 rayDir = weapon.MuzzleForward;//sights.MuzzleForward;
                //            var bullet = weapon.bullet;

                //            for (int i = 0; i < bulletCount; ++i)
                //            {
                //                //PhysicsWorld
                //                switch (bullet.BulletType)
                //                {

                //                    case BulletType.Hitscan:
                //                        switch (bullet.HitscanType)
                //                        {
                //                            case HitscanType.SingleRay:
                //                                RaycastInput raycastInput = new RaycastInput()
                //                                {
                //                                    Start = rayStart,
                //                                    End = rayStart + rayDir * bullet.HitscanRange,
                //                                    Filter = CollisionFilter.Default
                //                                };
                //                                RaycastHit hit = new RaycastHit();
                //                                //RaycastManager.SingleRaycast(PhysicsWorld, raycastInput, ref hit);

                //                                if (PhysicsWorld.CastRay(raycastInput, out hit))
                //                                //if(hit.Entity != Entity.Null)
                //                                {
                //                                    if (bullet.ImpactParticleEntity != Entity.Null)
                //                                    {
                //                                        Entity impactParticle = ecb.Instantiate(bullet.ImpactParticleEntity);
                //                                        ecb.SetComponent(impactParticle, new LocalTransform()
                //                                        {
                //                                            Position = hit.Position,
                //                                            Rotation = quaternion.LookRotation(hit.SurfaceNormal, new float3(0, 1, 0)),
                //                                            Scale = 1
                //                                        }
                //                                        );
                //                                    }


                //                                    PhysicsWorld.ApplyImpulse(hit.RigidBodyIndex, rayDir * 2000f, hit.Position);
                //                                    //var mass = SystemAPI.GetComponent<PhysicsMass>(hit.Entity);
                //                                    ecb.AddComponent(hit.Entity, new Energy() { ForcePosition = hit.Position, ForceNormal = rayDir, ForceAmount = 2000f });
                //                                }
                //                                break;
                //                            case HitscanType.SphereCast:
                //                                //PhysicsWorld.SphereCast(rayStart, weapon.SphereRadius, weapon.AimingSights.MuzzleForward)
                //                                break;
                //                            case HitscanType.PenetrateSphereCast:
                //                                break;
                //                        }


                //                        break;
                //                    case BulletType.Projectile:
                //                        Entity projectile = ecb.Instantiate(bullet.ProjectileEntity);
                //                        ecb.SetComponent(projectile, new LocalTransform()
                //                        {
                //                            Position = rayStart,
                //                            Rotation = quaternion.LookRotation(rayDir, new float3(0, 1, 0)),
                //                            Scale = 1
                //                        }
                //                        );
                //                        ecb.SetComponent(projectile, new PhysicsVelocity() { Linear = rayDir * 100f });

                //                        break;

                //                }
                //            }
                //        }

                //    }
                //}

                //UnityEngine.Debug.Log("Updating Weapon System");

                WeaponJob job = new WeaponJob
                {
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                    ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
                    entityManager = state.EntityManager,
                    sightsLookup = SystemAPI.GetComponentLookup<AimingSights>(false),
                    tagLookup = SystemAPI.GetComponentLookup<PlayerTag>(true),
                    IFF_Lookup = SystemAPI.GetComponentLookup<MagicIFF>(true),
                };

                state.Dependency = job.Schedule(state.Dependency);
                state.Dependency.Complete();
                //job.Schedule();


            }

            [BurstCompile]
            [WithAll(typeof(Simulate))]
     public partial struct WeaponJob : IJobEntity
            {
                public float DeltaTime;
                public PhysicsWorld PhysicsWorld;
                public EntityCommandBuffer ecb;
                public EntityManager entityManager;

                public ComponentLookup<AimingSights> sightsLookup;
                [ReadOnly]
                public ComponentLookup<PlayerTag> tagLookup;
                [ReadOnly]
                public ComponentLookup<MagicIFF> IFF_Lookup;
                //public ComponentLookup<Hand> HandLookup;

                void Execute(
                    Entity entity,
                    ref LocalTransform transform,
                    in ThirdPersonCharacterControl control,
                    //in KinematicCharacterBody body,
                    in Health health,
                    ref Weapon weapon
                    )
                {
                    if (sightsLookup.TryGetComponent(entity, out AimingSights sights))
                    {
                        if (control.Fire)
                        {
                            int bulletCount = weapon.BulletCount;
                            float3 rayDir = //weapon.MuzzleForward;//
                                            sights.MuzzleForward;
                            float3 rayStart = //weapon.MuzzlePosition;//
                                sights.MuzzlePosition;
                            //sights.MuzzlePosition
                            // + body.RelativeVelocity
                            //    * (1 + math.max(0, math.dot(rayDir, math.normalizesafe(body.RelativeVelocity))))
                            //    * DeltaTime;
                            //UnityEngine.Debug.Log(rayStart);
                            var bullet = weapon.bullet;

                            bool shotFromPlayer = tagLookup.TryGetComponent(entity, out PlayerTag tag);

                            if (bullet.MuzzleFlashEntity != Entity.Null)
                            {
                                Entity muzzleFlash = default;

                                if (shotFromPlayer)
                                {
                                    muzzleFlash = ecb.Instantiate(bullet.MyMuzzleFlashEntity);
                                }
                                else
                                {
                                    muzzleFlash = ecb.Instantiate(bullet.MuzzleFlashEntity);
                                }
                                ecb.SetComponent(muzzleFlash, new LocalTransform()
                                {
                                    Position = rayStart,
                                    Rotation = quaternion.LookRotation(rayDir, new float3(0, 1, 0)),
                                    Scale = 1
                                }
                                );
                            }

                            Entity sound = ecb.CreateEntity();
                            ecb.AddComponent(sound, new LoudSound() { Source = rayStart, Range = bullet.LoudSoundRange, ElapsedTime = 0 });
                            if (shotFromPlayer)
                            {
                                ecb.AddComponent(sound, new PlayerTag());
                            }

                            for (int i = 0; i < bulletCount; ++i)
                            {
                                //RaycastInput raycastInput;
                                //RaycastHit hit;
                                //bool impact = false;
                                //PhysicsWorld
                                bool isHitscan = false;
                                var buffer = entityManager.GetBuffer<IgnoreHitboxData>(entity);
                                switch (bullet.BulletType)
                                {
                                    case BulletType.Projectile:
                                    case BulletType.Hitscan:
                                        if (bullet.BulletType == BulletType.Hitscan)
                                        { isHitscan = true; }
                                        RayCastObstructionHitsCollector collector = new RayCastObstructionHitsCollector(buffer, rayDir);
                                        RaycastInput input = new RaycastInput()
                                        {
                                            Start = rayStart,
                                            End = rayStart + rayDir * bullet.HitscanRange,
                                            Filter = bullet.HitscanFilter
                                        };
                                        PhysicsWorld.CastRay(input, ref collector);//CapsuleCastCustom(rayStart, rayStart, 0.001f, )
                                                                                   //.SphereCastCustom<RayCastObstructionHitsCollector>(rayStart, 0.001f, rayDir, bullet.HitscanRange, ref collector,
                                                                                   //bullet.HitscanFilter, QueryInteraction.IgnoreTriggers);

                                        if (collector.NumHits > 0)
                                        {
                                            //UnityEngine.Debug.Log("closest hit position : " + collector.ClosestHit.Position);
                                            if (bullet.ImpactParticleEntity != Entity.Null)
                                            {
                                                Entity particleEntity = ecb.Instantiate(bullet.ImpactParticleEntity);
                                                ecb.SetComponent(particleEntity, new LocalTransform()
                                                {
                                                    Position = collector.ClosestHit.Position,
                                                    Rotation = //new quaternion(0, 0, 0, 1), //
                                                               quaternion.LookRotation(collector.ClosestHit.SurfaceNormal,
                                                    collector.ClosestHit.SurfaceNormal.y > 0.999f ? rayDir : new float3(0, 1, 0)  //math.cross(collector.ClosestHit.SurfaceNormal, math.cross( new float3(0, 1, 0), collector.ClosestHit.SurfaceNormal))
                                                    ),
                                                    Scale = 1
                                                }
                                            );
                                                //ecb.AddComponent(particleEntity, new LocalToWorld()
                                                //{
                                                //    Value = new float4x4(
                                                //    quaternion.LookRotation(collector.ClosestHit.SurfaceNormal, new float3(0, 1, 0)), collector.ClosestHit.Position)
                                                //});
                                            }

                                            ColliderKey hitColliderKey = collector.ClosestHit.Material.CustomTags;
                                            if (hitColliderKey != bullet.MagicIFF_Key)
                                            {
                                                ecb.AddComponent(collector.ClosestHit.Entity, new Energy() {
                                                    SourcePosition = rayStart,
                                                    ForcePosition = collector.ClosestHit.Position,
                                                    ForceNormal = rayDir, ForceNormalPhysically = collector.ClosestHit.SurfaceNormal,
                                                    ForceAmount = bullet.EnergyAmount });


                                            }
                                            if (isHitscan)
                                            {
                                                ecb.AddComponent(entity, new HitscanLineComponent()
                                                {
                                                    StartPos = rayStart,
                                                    EndPos = collector.ClosestHit.Position
                                                });
                                            }
                                        }
                                        else
                                        {
                                            if (isHitscan)
                                            {
                                                ecb.AddComponent(entity, new HitscanLineComponent()
                                                {
                                                    StartPos = rayStart,
                                                    EndPos = input.End
                                                });
                                            }
                                        }
                                        break;
                                }
                                //case BulletType.Projectile:
                                if (bullet.BulletType == BulletType.Projectile)
                                {
                                    Entity projectile = ecb.Instantiate(bullet.ProjectileEntity);

                                    ecb.SetComponent(projectile, new LocalTransform()
                                    {
                                        Position = rayStart + rayDir * bullet.HitscanRange,
                                        Rotation = quaternion.LookRotation(rayDir, rayDir.y > 0.99f ? transform.Forward() * -1 : new float3(0, 1, 0)),
                                        Scale = 1
                                    }
                                    );
                                    ecb.SetComponent(projectile, new PhysicsVelocity() { Linear = rayDir * bullet.ProjectileSpeed });
                                    if (bullet.MagicIFF_Key != ColliderKey.Empty)
                                    {
                                        ecb.AddComponent(projectile, new MagicIFF() { Key = bullet.MagicIFF_Key });
                                    }
                                    ecb.AddComponent(projectile, new TriggedWarheadData() {
                                        FiredPosition = rayStart,
                                        WarheadForward = rayDir, WarheadForceAmount = bullet.EnergyAmount
                                    });
                                    var bufferOnProjectile = ecb.AddBuffer<IgnoreHitboxData>(projectile);
                                    for (int j = 0; j < buffer.Length; ++j)
                                    {
                                        bufferOnProjectile.Add(buffer[j]);
                                    }
                                }
                            }
                        }

                    }


                }
            }
        } }