
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;



namespace ImaginaryReactor {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct EnergySystem : ISystem
    {
        //public EntityCommandBuffer _ecb;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Energy>().Build());
            //_ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (energy, mass, velocity) in SystemAPI.Query<Energy, PhysicsMass, RefRW<PhysicsVelocity>>().WithAll<Energy, PhysicsMass, PhysicsVelocity>())
            {
                float3 impulse = energy.ForceNormal * energy.ForceAmount;
                velocity.ValueRW.ApplyImpulse(in mass, in mass.Transform.pos, in mass.Transform.rot, in impulse, in energy.ForcePosition);
            }
            foreach (var (energy, hitbox) in SystemAPI.Query<Energy, RefRW<Hitbox>>().WithAll<Energy, Hitbox>())
            {
                if (SystemAPI.HasComponent<Health>(hitbox.ValueRO.Owner))
                {
                    //UnityEngine.Debug.Log(hitbox.ValueRO.Owner.Index+"said Ouch");
                    //health.ValueRW.RemainHealth -= energy.ForceAmount;
                    Health health = SystemAPI.GetComponent<Health>(hitbox.ValueRO.Owner);
                    health.RemainHealth -= energy.ForceAmount;
                    SystemAPI.SetComponent(hitbox.ValueRO.Owner, health);

                    if (SystemAPI.HasComponent<PlayerTag>(hitbox.ValueRO.Owner))
                    {
                        var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                        ecb.AddComponent(hitbox.ValueRO.Owner, new TrackInfo()
                        {
                            IsDirection = true,    //false,
                            LastKnownVector = energy.SourcePosition, //energy.ForceNormal * energy.ForceAmount
                            DamageAmount = energy.ForceAmount
                        });
                    }
                }
            }

            //foreach(var v in SystemAPI.Query<PhysicsCollider>().WithAll<PhysicsCollider>())
            //{


            //}
            //foreach (var (energy, health) in SystemAPI.Query<Energy,  RefRW<Health>>().WithAll<Energy, Health>())
            //{
            //    //UnityEngine.Debug.Log("Losing Health");
            //    health.ValueRW.RemainHealth -= energy.ForceAmount;
            //}

            EnergyJob job = new EnergyJob
            {
                //DeltaTime = SystemAPI.Time.DeltaTime,
                //PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
            };
            //job.Schedule();
            state.Dependency = job.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
     public partial struct EnergyJob : IJobEntity
        {
            //public float DeltaTime;
            //public PhysicsWorld PhysicsWorld;
            public EntityCommandBuffer ecb;

            void Execute(
                Entity entity,
                ref Energy energy//,
                                 //in WeaponControl cameraControl,
                                 //in DynamicBuffer<WeaponIgnoredEntityBufferElement> ignoredEntitiesBuffer
                )
            {
                ecb.RemoveComponent<Energy>(entity);
            }
        }

    }
}