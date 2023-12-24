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
    public struct SphereCastObstructionHitsCollector : ICollector<ColliderCastHit>
    {
        public bool EarlyOutOnFirstHit => false;
        public float MaxFraction => 1f;
        public int NumHits { get; private set; }

        public ColliderCastHit ClosestHit;

        private PhysicsWorld PhysicsWorld;
        private CollisionFilter _raycastFilter;

        private float _closestHitFraction;
        private float3 _origin;
        private float _radius;
        //private Entity _followedCharacter;
       // private DynamicBuffer<IgnoreHitboxData> _ignoredEntitiesBuffer;

        public NativeArray<ColliderCastHit> Hits;

        public SphereCastObstructionHitsCollector(//Entity followedCharacter,
            //DynamicBuffer<IgnoreHitboxData> ignoredEntitiesBuffer,
            float3 origin, PhysicsWorld world, CollisionFilter raycastFilter, float radius,
            int capacity = 1)
        {
            NumHits = 0;
            ClosestHit = default;
            _radius = radius;
            _closestHitFraction = float.MaxValue;
            _origin = origin;

            PhysicsWorld = world;
            _raycastFilter = raycastFilter;
            //_followedCharacter = followedCharacter;
            //_ignoredEntitiesBuffer = ignoredEntitiesBuffer;
            Hits = new NativeArray<ColliderCastHit>(capacity, Allocator.Temp);
        }

        [BurstCompile]
        public bool AddHit(ColliderCastHit hit)
        {
            //if (_followedCharacter == hit.Entity)
            //{
            //    return false;
            //}

            //if (/*math.dot(hit.SurfaceNormal, _bulletDirection) < 0f ||*/ !PhysicsUtilities.IsCollidable(hit.Material))
            //{
            //    return false;
            //}

            //for (int i = 0; i < _ignoredEntitiesBuffer.Length; i++)
            //{
            //    if (_ignoredEntitiesBuffer[i].hitboxEntity == hit.Entity)
            //    {
            //        return false;
            //    }
            //}

            RaycastInput rayInput = new RaycastInput()
            {
                Start = _origin,
                End = hit.Position, //_origin + math.normalizesafe(hit.Position- _origin)/**hit.Fraction*_radius*/,
                Filter = _raycastFilter
            };

            //UnityEngine.Debug.DrawLine(rayInput.Start, rayInput.End, UnityEngine.Color.red, 2f);

            if (PhysicsWorld.CastRay(rayInput, out RaycastHit rayHit))
            {
                //UnityEngine.Debug.Log(rayHit.Position);
                //UnityEngine.Debug.DrawLine(rayInput.Start, rayHit.Position, UnityEngine.Color.white, 3f);
                if (rayHit.Entity != hit.Entity)
                {
                    //UnityEngine.Debug.Log("Ray Filtered");
                    return false; 
                }
            }

            // Process valid hit
            if (hit.Fraction < _closestHitFraction)
            {
                _closestHitFraction = hit.Fraction;
                ClosestHit = hit;
            }

            bool reachedEnd = NumHits + 1 >= Hits.Length;
            if (reachedEnd)
            {
                Hits[Hits.Length - 1] = ClosestHit;
            }
            else
            {
                Hits[NumHits] = hit;
                //currentIndex++;
                NumHits++;
            }

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
            var deltaTime = SystemAPI.Time.DeltaTime;
            var _ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

            ExplosiveJob explosiveJob = new ExplosiveJob()
            {
                DeltaTime = deltaTime,
                ecb = _ecb,
                PhysicsWorld = physicsWorld
            };

            state.Dependency = explosiveJob.Schedule(state.Dependency);
            state.Dependency.Complete();

            WeaponJob job = new WeaponJob
                {
                    DeltaTime = deltaTime,
                    PhysicsWorld = physicsWorld,
                    ecb = _ecb,
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
        public partial struct ExplosiveJob : IJobEntity
        {
            public float DeltaTime;
            public EntityCommandBuffer ecb;
            public PhysicsWorld PhysicsWorld;

            void Execute(
                ref Dissolvable dissolvable,
                ref Warhead warhead,
                in LocalToWorld ltw,
                Entity entity
                )
            {
                dissolvable.RemainTime -= DeltaTime;

                if (dissolvable.RemainTime < 0)
                {

                    var explosionEffect = ecb.Instantiate(warhead.Fragment.ImpactParticleEntity);
                    ecb.SetComponent(explosionEffect, new LocalTransform()
                    {
                        Position = ltw.Position,//rayStart + rayDir * bullet.HitscanRange,
                        Rotation = ltw.Rotation,//quaternion.LookRotation(sights.MuzzleForward, new float3(0, 1, 0)),//(rayDir, rayDir.y > 0.99f ? transformForward * -1 : new float3(0, 1, 0)),
                        Scale = 1
                    }
            );

                    var ignoreHitboxList = new DynamicBuffer<IgnoreHitboxData>();
                    //Ignition(ref warhead, ref physicsWorld, //ref ignoreHitboxList , 
                    //    ltw.Position, ref ecb);
                    //ecb.RemoveComponent<Warhead>(entity);

                    switch (warhead.Fragment.BulletType)
                    {
                        case BulletType.Hitscan:
                            break;
                        case BulletType.Projectile:
                            break;
                        case BulletType.SphericalExplosive:

                            var collector = new SphereCastObstructionHitsCollector(//excludeHitboxes,
                                ltw.Position, PhysicsWorld, warhead.Fragment.HitscanFilter, warhead.Fragment.SphereRadius, 128);
                            if (PhysicsWorld.SphereCastCustom<SphereCastObstructionHitsCollector>(ltw.Position, warhead.Fragment.SphereRadius, new float3(0, -1, 0), warhead.Fragment.SphereRadius,
                                ref collector, warhead.Fragment.ShapeFilter,QueryInteraction.IgnoreTriggers))
                            //(origin, warhead.Fragment.SphereRadius, float3.zero, warhead.Fragment.SphereRadius,
                            //ref collector , CollisionFilter.Default))
                            {
                                //UnityEngine.Debug.Log("Detect Hitbox");
                                for (int i = 0; i < collector.NumHits; ++i)
                                {
                                    var hit = collector.Hits[i];
                                    //  UnityEngine.Debug.Log("Hitbox : " + hit.Entity.Index);
                                    UnityEngine.Debug.DrawLine(ltw.Position, hit.Position, UnityEngine.Color.red, 3);

                                    ecb.AddComponent(hit.Entity, new Energy()
                                    {
                                        SourcePosition = ltw.Position,
                                        ForcePosition = hit.Position,
                                        ForceNormal = hit.SurfaceNormal,
                                        ForceVector = /*hit.SurfaceNormal*/ math.normalizesafe( hit.Position - ltw.Position ) * warhead.Fragment.RigidbodyPushForce,
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
                        case BulletType.ShapeExplosive:
                            break;
                    }

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
        //                    //  UnityEngine.Debug.Log("Hitbox : " + hit.Entity.Index);

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
                    in Health health,
                    ref Weapon weapon
                    )
                {
                    if (sightsLookup.TryGetComponent(entity, out AimingSights sights))
                    {
                        if (control.Fire)
                        {
                            int bulletCount = weapon.BulletCount;
                            float3 rayDir = sights.MuzzleForward;
                            float3 rayStart = sights.MuzzlePosition;
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
                            var buffer = entityManager.GetBuffer<IgnoreHitboxData>(entity);
                            float3 transformForward = transform.Forward();
                            //BulletDamage( transformForward, ref rayDir,ref  rayStart,ref bullet,ref buffer,ref ecb,ref PhysicsWorld);

                            bool isHitscan = false;
                            bool isHit = false;
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
                                        isHit = true;
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
                                            ecb.AddComponent(collector.ClosestHit.Entity, new Energy()
                                            {
                                                SourcePosition = rayStart,
                                                ForcePosition = collector.ClosestHit.Position,
                                                ForceNormal = rayDir,
                                                ForceVector = -collector.ClosestHit.SurfaceNormal * bullet.RigidbodyPushForce,
                                                BaseDamage = bullet.EnergyAmount,
                                                CriticalDamage = bullet.EnergyAmount * bullet.CriticalMultiply  //ApplyCritical = true
                                            });


                                        }
                                        if (isHitscan)
                                        {
                                            var lineEntity = ecb.CreateEntity();
                                            ecb.AddComponent(lineEntity, new HitscanLineComponent()
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
                                            var lineEntity = ecb.CreateEntity();
                                            ecb.AddComponent(lineEntity, new HitscanLineComponent()
                                            {
                                                StartPos = rayStart,
                                                EndPos = input.End
                                            });
                                        }
                                    }
                                    break;
                            }
                            //case BulletType.Projectile:
                            if (bullet.BulletType == BulletType.Projectile && !isHit)
                            {
                                Entity projectile = ecb.Instantiate(bullet.ProjectileEntity);

                                ecb.SetComponent(projectile, new LocalTransform()
                                {
                                    Position = rayStart + rayDir * bullet.HitscanRange,
                                    Rotation = quaternion.LookRotation(rayDir, rayDir.y > 0.99f ? transformForward * -1 : new float3(0, 1, 0)),
                                    Scale = 1
                                }
                                );
                                ecb.SetComponent(projectile, new PhysicsVelocity() { Linear = rayDir * bullet.ProjectileSpeed });
                                if (bullet.MagicIFF_Key != ColliderKey.Empty)
                                {
                                    ecb.AddComponent(projectile, new MagicIFF() { Key = bullet.MagicIFF_Key });
                                }
                                ecb.AddComponent(projectile, new TriggedWarheadData()
                                {
                                    FiredPosition = rayStart,
                                    WarheadForward = rayDir,
                                    WarheadBaseDamage = bullet.EnergyAmount
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

            [BurstCompile]
            public void BulletDamage(in float3 transformForward, ref float3 rayDir, ref float3 rayStart, ref Bullet bullet, ref DynamicBuffer<IgnoreHitboxData> buffer,
           ref EntityCommandBuffer ecb, ref PhysicsWorld physicsWorld)
            {
                bool isHitscan = false;
                bool isHit = false;
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
                        physicsWorld.CastRay(input, ref collector);//CapsuleCastCustom(rayStart, rayStart, 0.001f, )
                                                                   //.SphereCastCustom<RayCastObstructionHitsCollector>(rayStart, 0.001f, rayDir, bullet.HitscanRange, ref collector,
                                                                   //bullet.HitscanFilter, QueryInteraction.IgnoreTriggers);

                        if (collector.NumHits > 0)
                        {
                            isHit = true;
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
                                ecb.AddComponent(collector.ClosestHit.Entity, new Energy()
                                {
                                    SourcePosition = rayStart,
                                    ForcePosition = collector.ClosestHit.Position,
                                    ForceNormal = rayDir,
                                    ForceVector = -collector.ClosestHit.SurfaceNormal * bullet.RigidbodyPushForce,
                                    BaseDamage = bullet.EnergyAmount,
                                    CriticalDamage = bullet.EnergyAmount * bullet.CriticalMultiply  //ApplyCritical = true
                                });


                            }
                            if (isHitscan)
                            {
                                var lineEntity = ecb.CreateEntity();
                                ecb.AddComponent(lineEntity, new HitscanLineComponent()
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
                                var lineEntity = ecb.CreateEntity();
                                ecb.AddComponent(lineEntity, new HitscanLineComponent()
                                {
                                    StartPos = rayStart,
                                    EndPos = input.End
                                });
                            }
                        }
                        break;
                }
                //case BulletType.Projectile:
                if (bullet.BulletType == BulletType.Projectile && !isHit)
                {
                    Entity projectile = ecb.Instantiate(bullet.ProjectileEntity);

                    ecb.SetComponent(projectile, new LocalTransform()
                    {
                        Position = rayStart + rayDir * bullet.HitscanRange,
                        Rotation = quaternion.LookRotation(rayDir, rayDir.y > 0.99f ? transformForward * -1 : new float3(0, 1, 0)),
                        Scale = 1
                    }
                    );
                    ecb.SetComponent(projectile, new PhysicsVelocity() { Linear = rayDir * bullet.ProjectileSpeed });
                    if (bullet.MagicIFF_Key != ColliderKey.Empty)
                    {
                        ecb.AddComponent(projectile, new MagicIFF() { Key = bullet.MagicIFF_Key });
                    }
                    ecb.AddComponent(projectile, new TriggedWarheadData()
                    {
                        FiredPosition = rayStart,
                        WarheadForward = rayDir,
                        WarheadBaseDamage = bullet.EnergyAmount
                    });
                    var bufferOnProjectile = ecb.AddBuffer<IgnoreHitboxData>(projectile);
                    for (int j = 0; j < buffer.Length; ++j)
                    {
                        bufferOnProjectile.Add(buffer[j]);
                    }
                }

                //return transform;
            }

        }


    }

}

