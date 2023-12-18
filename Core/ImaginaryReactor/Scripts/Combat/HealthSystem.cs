using Unity.Entities;
using Unity.Burst;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.CharacterController;
using Unity.Physics;

namespace ImaginaryReactor { 
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(EnergySystem))]
    public partial  struct HealthSystem : ISystem
{
    //public EntityCommandBuffer _ecb;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        //_ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
    }

    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var _ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (clear, entity)in SystemAPI.Query<ClearThisCharacterBody>().WithEntityAccess())
        {
            if (SystemAPI.HasComponent<KinematicCharacterBody>(entity))
            {
                var body = SystemAPI.GetComponent<KinematicCharacterBody>(entity);
                if (body.IsGrounded)
                {
                    _ecb.RemoveComponent<ThirdPersonCharacterControl>(entity);
                    _ecb.RemoveComponent<CharacterInterpolation>(entity);
                    _ecb.RemoveComponent<KinematicCharacterBody>(entity);
                    _ecb.RemoveComponent<KinematicCharacterDeferredImpulse>(entity);
                    _ecb.RemoveComponent<KinematicCharacterHit>(entity);
                    _ecb.RemoveComponent<KinematicCharacterProperties>(entity);
                    _ecb.RemoveComponent<KinematicVelocityProjectionHit>(entity);
                    _ecb.RemoveComponent<StatefulKinematicCharacterHit>(entity);
                    _ecb.RemoveComponent<StoredKinematicCharacterData>(entity);
                    _ecb.RemoveComponent<ThirdPersonCharacterComponent>(entity);
                    _ecb.RemoveComponent<PhysicsCollider>(entity);
                    _ecb.RemoveComponent<ClearThisCharacterBody>(entity);
                }
            }

        }

        AliveJob job = new AliveJob
        {
            ecb = _ecb //_ecb//SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
            //entityManager = state.EntityManager,
        };
        //job.Schedule();
        state.Dependency = job.Schedule(state.Dependency);
        state.Dependency.Complete();
        HitboxRemoveJob removeJob = new HitboxRemoveJob
        {
            ecb = _ecb,
            HealthLookUp = state.GetComponentLookup<Health>(),
        };

        state.Dependency = removeJob.Schedule(state.Dependency);
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct AliveJob : IJobEntity
        {
            //public float DeltaTime;
            //public PhysicsWorld PhysicsWorld;
            public EntityCommandBuffer ecb;
            //public EntityManager entityManager;

            void Execute(
                Entity entity,
                //ref ThirdPersonCharacterControl cc,
                in Health health//,
                                //in WeaponControl cameraControl,
                                //in DynamicBuffer<WeaponIgnoredEntityBufferElement> ignoredEntitiesBuffer
                )
            {
                if (health.RemainHealth <= 0)
                {
                    //foreach(var hitbox in SystemAPI.Query<Hitbox>().WithAll<Hitbox>())
                    //{

                    //}

                    //UnityEngine.Debug.Log("I'm dead");
                    //ecb.RemoveComponent<ThirdPersonCharacterControl>(entity);
                    ecb.RemoveComponent<PlayerTag>(entity);
                    //ecb.RemoveComponent<CharacterInterpolation>(entity);
                    //ecb.RemoveComponent<KinematicCharacterBody>(entity);
                    //ecb.RemoveComponent<KinematicCharacterDeferredImpulse>(entity);
                    //ecb.RemoveComponent<KinematicCharacterHit>(entity);
                    //ecb.RemoveComponent<KinematicCharacterProperties>(entity);
                    //ecb.RemoveComponent<KinematicVelocityProjectionHit>(entity);
                    //ecb.RemoveComponent<StatefulKinematicCharacterHit>(entity);
                    //ecb.RemoveComponent<StoredKinematicCharacterData>(entity);
                    //ecb.RemoveComponent<ThirdPersonCharacterComponent>(entity);
                    //ecb.RemoveComponent<PhysicsCollider>(entity);


                    //var buffer = entityManager.GetBuffer<IncludedHitbox>(entity);
                    //for(int i = buffer.Length-1; i>=0; --i)
                    //{
                    //    ecb.RemoveComponent<PhysicsCollider>(buffer[i].HitboxEntity);
                    //}
                    //foreach(var Hitbox in )
                    ecb.RemoveComponent<Health>(entity);
                }
            }
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
         public partial struct HitboxRemoveJob : IJobEntity
            {
                //public float DeltaTime;
                //public PhysicsWorld PhysicsWorld;
                public ComponentLookup<Health> HealthLookUp;
                public EntityCommandBuffer ecb;

                void Execute(
                    Entity entity,
                    //ref ThirdPersonCharacterControl cc,
                    in Hitbox hitbox//,
                                    //in WeaponControl cameraControl,
                                    //in DynamicBuffer<WeaponIgnoredEntityBufferElement> ignoredEntitiesBuffer
                    )
                {
                    if (!HealthLookUp.TryGetComponent(hitbox.Owner, out Health health))
                    {
                        ecb.RemoveComponent<PhysicsCollider>(entity);
                    }
                }
            }
        }


    }