using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Systems;
using Unity.Physics;
using Unity.CharacterController;
using Unity.Physics.Authoring;

namespace ImaginaryReactor
{
    [BurstCompile]
public struct SeekerCastObstructionHitsCollector : ICollector<ColliderCastHit>
{

    public bool EarlyOutOnFirstHit => false;
    public float MaxFraction => 1f;
    public int NumHits { get; private set; }

    public ColliderCastHit ClosestHit;

    private float _closestHitFraction;
    //private Entity _followedCharacter;
    private DynamicBuffer<IgnoreHitboxData> _ignoredEntitiesBuffer;
    private ColliderKey _targetKey;
    //[ReadOnly]
    private ComponentLookup<MagicIFF> _iffLookUp;
    public NativeArray<ColliderCastHit> Hits;
    //private int currentIndex;

    public SeekerCastObstructionHitsCollector(//Entity followedCharacter,
        DynamicBuffer<IgnoreHitboxData> ignoredEntitiesBuffer, ColliderKey targetKey, ComponentLookup<MagicIFF> iffLookUp, int capacity = 1)
    {
        NumHits = 0;
        ClosestHit = default;
        _closestHitFraction = float.MaxValue;
        //_followedCharacter = followedCharacter;
        _ignoredEntitiesBuffer = ignoredEntitiesBuffer;
        _targetKey= targetKey;
        _iffLookUp = iffLookUp;

        Hits = new NativeArray<ColliderCastHit>(capacity, Allocator.Temp);
        //currentIndex = 0;
    }

    [BurstCompile]
    public bool AddHit(ColliderCastHit hit)
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
        if (_iffLookUp.HasComponent(hit.Entity))
        {
            if (_iffLookUp[hit.Entity].Key//hit.Material.CustomTags 
                    == _targetKey.Value)
            {
                bool reachedEnd = NumHits + 1 >= Hits.Length;
                // Process valid hit
                if (hit.Fraction < _closestHitFraction)
                {
                    _closestHitFraction = hit.Fraction;
                    ClosestHit = hit;
                }

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

        return false;
    }

    public void Dispose()
    {
        Hits.Dispose();
    }
}


[BurstCompile]
public struct ColliderCastObstructionHitsCollector : ICollector<ColliderCastHit>
{

    public bool EarlyOutOnFirstHit => false;
    public float MaxFraction => 1f;
    public int NumHits { get; private set; }

    public ColliderCastHit ClosestHit;

    private float _closestHitFraction;
    //private Entity _followedCharacter;
    private DynamicBuffer<IgnoreHitboxData> _ignoredEntitiesBuffer;

    public NativeArray<ColliderCastHit> Hits;
    //private int currentIndex;

    public ColliderCastObstructionHitsCollector(//Entity followedCharacter,
        DynamicBuffer<IgnoreHitboxData> ignoredEntitiesBuffer, int capacity = 1)
    {
        NumHits = 0;
        ClosestHit = default;
        _closestHitFraction = float.MaxValue;
        //_followedCharacter = followedCharacter;
        _ignoredEntitiesBuffer = ignoredEntitiesBuffer;

        Hits = new NativeArray<ColliderCastHit>( capacity, Allocator.Temp);
        //currentIndex = 0;
    }

    [BurstCompile]
    public bool AddHit(ColliderCastHit hit)
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

        bool reachedEnd = NumHits + 1 >= Hits.Length;
        // Process valid hit
        if (hit.Fraction < _closestHitFraction)
        {
            _closestHitFraction = hit.Fraction;
            ClosestHit = hit;
        }

        if (reachedEnd)
        {
            Hits[Hits.Length-1] = ClosestHit;
        }
        else
        {
            Hits[NumHits] = hit;
            //currentIndex++;
            NumHits++;
        }


        return true;
    }
    
    public void Dispose()
    {
        Hits.Dispose();
    }
}


//[UpdateInGroup(typeof(SimulationSystemGroup))]
//[UpdateBefore(typeof(
//))]
//[UpdateInGroup(typeof(SimulationSystemGroup))]
//[UpdateAfter(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
//[UpdateInGroup(typeof(InitializationSystemGroup))]
//[UpdateBefore(typeof(ThirdPersonAIInputsSystem))]
[UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
[BurstCompile]
     public partial struct SeekerSystem : ISystem
        {
            //[BurstCompile]
            //void OnCreate(ref SystemState state)
            //{
            //    //state.RequireForUpdate<FixedTickSystem.Singleton>();
            //    //state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<ThirdPersonPlayer, ThirdPersonPlayerInputs, Brain>().Build());
            //}

            [BurstCompile]
            unsafe
            void OnUpdate(ref SystemState state)
            {
                //uint _fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

                //double ElapsedTime = SystemAPI.Time.ElapsedTime;
                //Entity targetEntity;
                //var PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
                //foreach (var brain in SystemAPI.Query<RefRW<Brain>>().WithAll<Brain>())
                //{
                //    var foundTarget = false;
                //    foreach (var (seeker, seekerLTW) in SystemAPI.Query<Seeker, LocalToWorld>())//.WithAll<Seeker, LocalToWorld>())
                //    {
                //        if(brain.ValueRW.Owner == seeker.BrainEntity)
                //        {
                //            foreach (var (hitbox, hitboxLTW, playerTag) in SystemAPI.Query<Hitbox, LocalToWorld, PlayerTag>().WithAll<Hitbox, LocalToWorld, PlayerTag>())//.WithAll<Hitbox, LocalToWorld>())
                //            {
                //                //if (state.GetComponentLookup<Brain>(false).TryGetComponent(seeker.BrainEntity, out Brain brain))
                //                {
                //                    float3 targetVector = (hitboxLTW.Position ) - (seekerLTW.Position );
                //                    float3 targetDir = math.normalizesafe(targetVector);
                //                    RaycastInput input = new RaycastInput()
                //                    {
                //                        Start = seekerLTW.Position /*+ targetDir*/ + brain.ValueRW.AimOffset,// + targetDir,
                //                        End = hitboxLTW.Position /*- targetDir*/ + brain.ValueRW.AimOffset,// - targetDir,//* hitbox.BoundSize + brain.ValueRW.AimOffset,
                //                        Filter = brain.ValueRW.Filter
                //                    };

                //                    bool raycasting = PhysicsWorld.CastRay(input, out RaycastHit hit);// || math.dot(targetDir, math.normalizesafe(input.End - input.Start)) < 0;
                //                    if(raycasting)
                //                    {
                //                        brain.ValueRW.DesirePositionToMove = hit.Position;
                //                    }

                //                    //UnityEngine.Debug.Log("Seek : "+ math.length(targetVector)+" / "+ math.dot(targetDir, seekerLTW.Forward) + " / "+raycasting);
                //                    //UnityEngine.Debug.Log("Seek : " + !(math.length(targetVector) > seeker.Range) + "/" + !(math.dot(targetDir, seekerLTW.Forward) < math.cos(math.radians(seeker.FieldOfView)))
                //                    //    + "/" + !raycasting);
                //                    //if (raycasting)
                //                    //{
                //                    //    UnityEngine.Debug.Log(hit.Position);
                //                    //}
                //                    if (!(math.length(targetVector) > seeker.Range) && !(math.dot(targetDir, seekerLTW.Forward) < math.cos(math.radians(seeker.FieldOfView)))
                //                        && !(raycasting))
                //                    {
                //                        //UnityEngine.Debug.Log("Target Found!");

                //                        {
                //                          //  UnityEngine.Debug.Log("Brain Activate");
                //                            foundTarget = true;
                //                            //brain.ValueRW.DesirePositionToLook = hitboxLTW.Position; //TargetEntity = hitbox.Owner;
                //                            brain.ValueRW.TargetRealPosition = hitboxLTW.Position;
                //                            brain.ValueRW.TargetRealVelocity = float3.zero;
                //                            if(SystemAPI.GetComponentLookup<KinematicCharacterBody>(true).TryGetComponent(hitbox.Owner, out KinematicCharacterBody body))
                //                            {
                //                                brain.ValueRW.TargetRealVelocity = body.RelativeVelocity;
                //                            }
                //                            SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged)
                //                                .AddComponent(brain.ValueRW.Owner, new TrackInfo() { Position = hitboxLTW.Position });
                //                        }
                //                    }


                //                }
                //            }
                //        }
                //    }

                //    brain.ValueRW.TargetFound = foundTarget;
                //}
                var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

                //foreach (var (seeker, seekerLTW, seekerEntity) in SystemAPI.Query<RefRW<Seeker>, LocalToWorld>().WithEntityAccess())//WithAll<Seeker, LocalToWorld>())
                //{ 
                //    foreach (var (hitbox, hitboxLTW, hitboxEntity) in SystemAPI.Query<Hitbox, LocalToWorld>().WithEntityAccess())   //temporary  // WithAll<Hitbox, LocalToWorld, PlayerTag>())//.WithAll<Hitbox, LocalToWorld>())
                //    {
                //        //if (state.GetComponentLookup<Brain>(false).TryGetComponent(seeker.BrainEntity, out Brain brain))
                //        ecb.AddComponent(hitboxEntity, new SweepInfo()
                //        {
                //            SeekerEntity = seekerEntity,
                //            SeekerLTW = seekerLTW
                //        });
                //    }

                //}

                var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
                var entityManager = state.EntityManager;
                SeekerCastObstructionHitsCollector collector;
                foreach (var (seeker, collider, ltw, entity) in SystemAPI.Query<Seeker, PhysicsCollider, LocalToWorld>().WithEntityAccess())//WithAll<Seeker, PhysicsCollider, LocalToWorld>())
                {
                    ColliderCastInput input = new ColliderCastInput()
                    {
                        Collider = collider.ColliderPtr,
                        Start = ltw.Position, End = ltw.Position,
                        Orientation = ltw.Rotation
                    };

                    var buffer = entityManager.GetBuffer<IgnoreHitboxData>(entity);
                    collector = new SeekerCastObstructionHitsCollector(buffer, seeker.TargetSideKey, SystemAPI.GetComponentLookup<MagicIFF>(true),
                        8);
                    if (physicsWorld.CastCollider(input, ref collector))
                    {
                        for (int i = 0; i < collector.NumHits; ++i)
                        {
                            bool haveToIgnore = false;

                            ColliderCastHit hit = collector.Hits[i]; //var hit = collector.NumHits[i];
                                                                     //if (SystemAPI.HasComponent<MagicIFF>(hit.Entity))
                                                                     //{
                                                                     //    var iff = SystemAPI.GetComponent<MagicIFF>(hit.Entity);
                                                                     //    if (iff.Key == seeker.SideKey)
                                                                     //    {
                                                                     //        haveToIgnore = true;
                                                                     //        break;
                                                                     //    }
                                                                     //}


                            for (int j = 0; j < buffer.Length; j++)
                            {
                                if (buffer[j].hitboxEntity == hit.Entity)
                                {
                                    haveToIgnore = true;
                                    break;
                                }
                            }

                            if (!haveToIgnore)
                            {
                                bool noObstacle = false;
                                //bool hitsOnHitbox = false;
                                var hitbox = new Hitbox();
                                float3 hitboxPos = hit.Position;
                                ColliderKey key = ColliderKey.Empty;
                                if (SystemAPI.HasComponent<Hitbox>(hit.Entity))
                                {
                                    hitbox = SystemAPI.GetComponent<Hitbox>(hit.Entity);
                                    //if (SystemAPI.HasComponent<LocalToWorld>(hit.Entity))
                                    //{
                                    //    hitboxPos = GetActualPositionOfHitbox(SystemAPI.GetComponent<LocalToWorld>(hit.Entity), hitbox);
                                    //}
                                    key = hitbox.IFF_Key;
                                }


                                float3 rayDir = math.normalizesafe(hit.Position - ltw.Position);
                                RaycastInput rayInput = new RaycastInput()
                                {
                                    Start = ltw.Position,
                                    End = hitboxPos - rayDir * 0.5f, //+ hit.SurfaceNormal * -0.01f,
                                    Filter = seeker.RaycastFilter
                                };
                                RayCastObstructionHitsCollector rayCollector = new RayCastObstructionHitsCollector(buffer, rayDir);

                                physicsWorld.CastRay(rayInput, ref rayCollector);//CapsuleCastCustom(rayStart, rayStart, 0.001f, )
                                                                                 //.SphereCastCustom<RayCastObstructionHitsCollector>(rayStart, 0.001f, rayDir, bullet.HitscanRange, ref collector,
                                                                                 //bullet.HitscanFilter, QueryInteraction.IgnoreTriggers);

                                if (rayCollector.NumHits <= 0)
                                //{
                                //    if(rayCollector.ClosestHit.Entity == hit.Entity)
                                //    {
                                //        hitsOnHitbox = true;
                                //    }
                                //}
                                //else
                                {
                                    noObstacle = true;
                                }

                                if (noObstacle || !seeker.CheckRaycast)//|| hitsOnHitbox)
                                {
                                    //UnityEngine.Debug.Log("Target Found!");
                                    float3 velocity = float3.zero;
                                    //ColliderKey key = ColliderKey.Empty;

                                    if (SystemAPI.HasComponent<PhysicsVelocity>(hit.Entity))
                                    {
                                        velocity = SystemAPI.GetComponent<PhysicsVelocity>(hit.Entity).Linear;
                                    }
                                    //bool hasHitbox = false;
                                    //var hitbox = new Hitbox();
                                    //float3 hitboxOwnerPos = float3.zero;
                                    //if (SystemAPI.HasComponent<Hitbox>(hit.Entity))
                                    {
                                        //hasHitbox = true;
                                        //hitbox = SystemAPI.GetComponent<Hitbox>(hit.Entity);
                                        if (SystemAPI.HasComponent<PhysicsVelocity>(hitbox.Owner))
                                        {
                                            velocity = SystemAPI.GetComponent<PhysicsVelocity>(hitbox.Owner).Linear;
                                        }
                                        //hitboxOwnerPos = SystemAPI.GetComponent<LocalToWorld>(hitbox.Owner).Position;

                                    }

                                    //float3 aimOffset = float3.zero;
                                    //if (SystemAPI.HasComponent<Brain>(seeker.BrainEntity))
                                    //{
                                    //    aimOffset = SystemAPI.GetComponent<Brain>(seeker.BrainEntity).AimOffset;
                                    //}
                                    //hitboxOwnerPos.y += aimOffset.y;

                                    ecb.AddComponent(seeker.BrainEntity, new TrackInfo()
                                    {
                                        TargetEntity = //hasHitbox ? hitbox.Owner :
                                        hit.Entity,
                                        IsDirection = seeker.GetDirection,
                                        LastKnownVector = seeker.GetDirection ? //(hasHitbox ? math.normalizesafe(hitboxOwnerPos - ltw.Position) : 
                                        rayDir//)
                                        : //(hasHitbox ? hitboxOwnerPos :
                                          hitboxPos, //),
                                        TargetVelocity = velocity,
                                        TargetKey = key//triggerEvent.ColliderKeyA
                                    });
                                }
                            }
                        }
                    }
                    collector.Dispose();
                }




                //    SeekerTriggerJob triggerJob = new SeekerTriggerJob()
                //    {
                //        SeekerLoopUp = SystemAPI.GetComponentLookup<Seeker>(true),
                //        HitboxLoopUp = SystemAPI.GetComponentLookup<Hitbox>(true),
                //        LTWLoopUp = SystemAPI.GetComponentLookup<LocalToWorld>(true),
                //        //VelocityLoopUp = SystemAPI.GetComponentLookup<PhysicsVelocity>(true),
                //        entityManager = state.EntityManager,
                //        ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
                //        physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld
                //};

                //state.Dependency = triggerJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

                //state.Dependency.Complete();

                //foreach(var(sweep, hitbox, hitboxLTW) in SystemAPI.Query<SweepInfo, Hitbox, LocalToWorld>())
                //{
                //    float3 targetVector = (hitboxLTW.Position) - (sweep.SeekerLTW.Position);
                //    float3 targetDir = math.normalizesafe(targetVector);
                //    RaycastInput input = new RaycastInput()
                //    {
                //        Start = sweep.SeekerLTW.Position,// + seeker.ValueRO.SeekerOffset,
                //        End = hitboxLTW.Position,// + seeker.ValueRO.SeekerOffset,
                //        Filter = sweep.Filter//seeker.ValueRO.Filter
                //    };

                //    bool raycasting = PhysicsWorld.CastRay(input, out RaycastHit hit);// || math.dot(targetDir, math.normalizesafe(input.End - input.Start)) < 0;

                //    if (!(math.length(targetVector) > sweep.Range) && !(math.dot(targetDir, sweep.SeekerLTW.Forward) < math.cos(math.radians(sweep.FieldOfView)))
                //        && !(raycasting))
                //    {
                //        if(SystemAPI.GetComponentLookup<Seeker>().TryGetComponent(sweep.SeekerEntity, out Seeker seeker))
                //        {

                //        }

                //            //seeker.ValueRW.TargetEntity = entity;
                //            //seeker.ValueRW.LastKnownVector = seeker.ValueRO.GetDirection ? math.normalizesafe(hitboxLTW.Position - seekerLTW.Position) : hitboxLTW.Position;

                //            //if (seeker.ValueRW.BrainEntity != Entity.Null)
                //            //{

                //            //            ecb.AddComponent(seeker.ValueRW.BrainEntity, new TrackInfo()
                //            //            {
                //            //                TargetEntity = entity,
                //            //                LastKnownVector = hitboxLTW.Position,
                //            //            });
                //            //}

                //    }
                //    else
                //    {
                //        //seeker.ValueRW.TargetEntity = Entity.Null;
                //    }

                //}
            }
        }
        //[BurstCompile]
        //namespace ImaginaryReactor { public partial  struct SeekerTriggerJob : ITriggerEventsJob
        //{
        //    [ReadOnly] public ComponentLookup<Seeker> SeekerLoopUp;
        //    [ReadOnly] public ComponentLookup<Hitbox> HitboxLoopUp;
        //    [ReadOnly] public ComponentLookup<LocalToWorld> LTWLoopUp;
        //    //[ReadOnly]public ComponentLookup<PhysicsVelocity> VelocityLoopUp;

        //    [ReadOnly]public PhysicsWorld physicsWorld;

        //    public EntityManager entityManager;
        //    public EntityCommandBuffer ecb;

        //    [BurstCompile]
        //    public void Execute(TriggerEvent triggerEvent)
        //    {
        //        Seeker seeker;
        //        Hitbox hitbox;



        //        //if (SeekerLoopUp.HasComponent(triggerEvent.EntityA)) { UnityEngine.Debug.Log("Seeker A Sweep"); }
        //        //if (SeekerLoopUp.HasComponent(triggerEvent.EntityB)) { UnityEngine.Debug.Log("Seeker B Sweep"); }

        //        if (SeekerLoopUp.HasComponent(triggerEvent.EntityA) && HitboxLoopUp.HasComponent(triggerEvent.EntityB))
        //        {
        //            seeker = SeekerLoopUp[triggerEvent.EntityA];//.GetRefRO(triggerEvent.EntityA);
        //            //UnityEngine.Debug.Log("Seeker A Sweep");
        //            //if(HitboxLoopUp.HasComponent(triggerEvent.EntityB))
        //            {
        //                UnityEngine.Debug.Log("B is Hitbox");
        //                hitbox = HitboxLoopUp[triggerEvent.EntityB];//.GetRefRO(triggerEvent.EntityB);
        //                //seeker.TargetEntity = triggerEvent.EntityB;
        //                UnityEngine.Debug.Log(LTWLoopUp[triggerEvent.EntityB].Position);
        //                bool haveToIgnore = false;

        //                var buffer = entityManager.GetBuffer<IgnoreHitboxData>(triggerEvent.EntityA);
        //                for(int i = 0; i< buffer.Length; i++)
        //                {
        //                    //UnityEngine.Debug.Log("Comparing " + buffer[i].hitboxEntity);
        //                    if (buffer[i].hitboxEntity == triggerEvent.EntityB)
        //                    {
        //                        haveToIgnore = true;
        //                        break;
        //                    }
        //                }

        //                if (!haveToIgnore && LTWLoopUp.TryGetComponent(triggerEvent.EntityB, out LocalToWorld ltwB))
        //                {
        //                    bool noObstacle = false;

        //                    if (seeker.CheckRaycast)
        //                    {
        //                        if (LTWLoopUp.TryGetComponent(triggerEvent.EntityA, out LocalToWorld ltwA))
        //                        {
        //                            //UnityEngine.Debug.Log("Collided " + triggerEvent.EntityA.Index + " between " + triggerEvent.EntityB.Index); 
        //                            RaycastInput input = new RaycastInput()
        //                            {
        //                                Start = ltwA.Position,
        //                                End = ltwB.Position,
        //                                Filter = seeker.RaycastFilter
        //                            };
        //                            if (!physicsWorld.CastRay(input, out RaycastHit hit))
        //                            {
        //                               noObstacle = true;
        //                                //UnityEngine.Debug.Log("Trying to send tracking info to A brain");
        //                            }
        //                        }
        //                    }

        //                    if(noObstacle || !seeker.CheckRaycast)
        //                    {
        //                        float3 velocity = float3.zero;
        //                        //if (VelocityLoopUp.TryGetComponent(triggerEvent.EntityB, out PhysicsVelocity v))
        //                        //{
        //                        //    velocity = v.Linear;
        //                        //}
        //                        ecb.AddComponent(seeker.BrainEntity, new TrackInfo()
        //                        {
        //                            TargetEntity = triggerEvent.EntityB,
        //                            IsDirection = seeker.GetDirection,
        //                            LastKnownVector = ltwB.Position,
        //                            TargetVelocity = velocity,
        //                            TargetKey = hitbox.IFF_Key//triggerEvent.ColliderKeyB
        //                        });
        //                    }
        //                }
        //            }
        //        }
        //        if (SeekerLoopUp.HasComponent(triggerEvent.EntityB) && HitboxLoopUp.HasComponent(triggerEvent.EntityA))
        //        {
        //            seeker = SeekerLoopUp[triggerEvent.EntityB];//SeekerLoopUp.GetRefRO(triggerEvent.EntityB);
        //            //UnityEngine.Debug.Log("Seeker B Sweep");
        //            //if(HitboxLoopUp.HasComponent(triggerEvent.EntityB))
        //            {
        //                UnityEngine.Debug.Log("A is Hitbox");
        //                hitbox = HitboxLoopUp[triggerEvent.EntityA];//.GetRefRO(triggerEvent.EntityA);
        //                UnityEngine.Debug.Log(LTWLoopUp[triggerEvent.EntityA].Position);
        //                //seeker.TargetEntity = triggerEvent.EntityA;
        //                bool haveToIgnore = false;

        //                var buffer = entityManager.GetBuffer<IgnoreHitboxData>(triggerEvent.EntityB);
        //                for (int i = 0; i < buffer.Length; i++)
        //                {
        //                    if (buffer[i].hitboxEntity == triggerEvent.EntityA)
        //                    {
        //                        haveToIgnore = true;
        //                        break;
        //                    }
        //                }

        //                if (!haveToIgnore && LTWLoopUp.TryGetComponent(triggerEvent.EntityB, out LocalToWorld ltwB))
        //                {
        //                    bool noObstacle = false;
        //                    //UnityEngine.Debug.Log("Collided " + triggerEvent.EntityA.Index + " between " + triggerEvent.EntityB.Index);
        //                    if (seeker.CheckRaycast) {
        //                        if (LTWLoopUp.TryGetComponent(triggerEvent.EntityA, out LocalToWorld ltwA))
        //                        {
        //                            //UnityEngine.Debug.Log("Collided " + triggerEvent.EntityA.Index + " between " + triggerEvent.EntityB.Index); 
        //                            RaycastInput input = new RaycastInput()
        //                            {
        //                                Start = ltwB.Position,
        //                                End = ltwA.Position,
        //                                Filter = seeker.RaycastFilter
        //                            };
        //                            if (!physicsWorld.CastRay(input, out RaycastHit hit))
        //                            {
        //                                noObstacle = true;
        //                                //UnityEngine.Debug.Log("Trying to send tracking info to B brain");
        //                            }
        //                        } 
        //                    }

        //                    if (noObstacle || !seeker.CheckRaycast)
        //                    { 
        //                        float3 velocity = float3.zero;
        //                        //if (VelocityLoopUp.TryGetComponent(triggerEvent.EntityA, out PhysicsVelocity v))
        //                        //{
        //                        //    velocity = v.Linear;
        //                        //}

        //                        ecb.AddComponent(seeker.BrainEntity, new TrackInfo()
        //                        {
        //                            TargetEntity = triggerEvent.EntityA,
        //                            IsDirection = seeker.GetDirection,
        //                            LastKnownVector = ltwB.Position,
        //                            TargetVelocity = velocity,
        //                            TargetKey = hitbox.IFF_Key//triggerEvent.ColliderKeyA
        //                        }); 
        //                    }
        //                }
        //            }
        //        }



        //    }
        //}

        //[BurstCompile]
        //namespace ImaginaryReactor { public partial  struct SeekerJob : IJobEntity
        //{
        //    [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        //    [ReadOnly] public float3 targetPosition;
        //    public uint fixedTick;

        //    [BurstCompile]
        //    void Execute(
        //        Entity entity,
        //        ref Seeker seeker,
        //        in ThirdPersonPlayer player
        //        )
        //    {
        //        float3 moveInput = float3.zero;
        //        float2 viewInput = float2.zero;
        //        //if(LocalToWorldLookup.TryGetComponent(brain.TargetEntity, out LocalToWorld targetLTW))
        //        //{
        //        //    brain.DesirePositionToLook = targetLTW.Position;
        //        //}

        //    }
        //}

        //public struct SweepInfo : IComponentData
        //{
        //    public Entity SeekerEntity;
        //    public LocalToWorld SeekerLTW;
        //    public CollisionFilter Filter;
        //    public float Range;
        //    public float FieldOfView;
        //}
    }