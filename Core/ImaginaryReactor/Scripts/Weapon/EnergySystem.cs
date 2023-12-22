
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
            var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (energy, mass, velocity,entity) in SystemAPI.Query<Energy, PhysicsMass, RefRW<PhysicsVelocity>>().WithAll<Energy, PhysicsMass, PhysicsVelocity>()
                .WithEntityAccess())
            {
                float3 impulse = energy.ForceVector;
                velocity.ValueRW.ApplyImpulse(in mass, in mass.Transform.pos, in mass.Transform.rot, in impulse, in energy.ForcePosition);
                ecb.RemoveComponent<Energy>(entity);
            }
            foreach (var (energy, hitbox) in SystemAPI.Query<Energy, Hitbox>().WithAll<Energy, Hitbox>())
            {
                if (SystemAPI.HasComponent<Health>(hitbox.Owner))
                {
                    var appliedEnergy = energy;
                    //var damage = 
                    appliedEnergy.BaseDamage =
                    hitbox.IsCritical ?
                        appliedEnergy.CriticalDamage * hitbox.DamageMultiply
                        : appliedEnergy.BaseDamage * hitbox.DamageMultiply;

                    //if (SystemAPI.HasBuffer<StackedEnergy>(hitbox.Owner))
                    //{
                    //    ecb.AddBuffer<StackedEnergy>(hitbox.Owner); 
                    //}
                        var stackedEnergy = SystemAPI.GetBuffer<StackedEnergy>(hitbox.Owner);//ecb.AddBuffer<>(entity)
                        stackedEnergy.Add(new StackedEnergy() { Energy = appliedEnergy });
                    //}
                    //else
                    //{
                    //    ecb.AddBuffer<StackedEnergy>(hitbox.Owner);
                    //}
                    //if (SystemAPI.HasComponent<Energy>(hitbox.Owner))
                    //{
                    //    var energyOnOwner = SystemAPI.GetComponent<Energy>(hitbox.Owner);

                    //    if(energyOnOwner.BaseDamage < appliedEnergy.BaseDamage)//damage)
                    //    {
                    //        UnityEngine.Debug.Log("override damage");
                    //        //                            appliedEnergy.BaseDamage = damage;
                    //        ecb.SetComponent(hitbox.Owner, appliedEnergy);
                    //    }
                    //    else
                    //    {
                    //        UnityEngine.Debug.Log("filter energy");
                    //    }
                    //}
                    //else
                    //{
                    //    UnityEngine.Debug.Log("apply damage first time");
                    //    ecb.AddComponent(hitbox.Owner, appliedEnergy);
                    //}
                    //UnityEngine.Debug.Log(hitbox.Owner.Index+"said Ouch");
                    //health.ValueRW.RemainHealth -= energy.BaseDamage;
                    //Health health = SystemAPI.GetComponent<Health>(hitbox.Owner);
                    //health.RemainHealth -= energy.BaseDamage;
                    //SystemAPI.SetComponent(hitbox.Owner, health);

                    //if (SystemAPI.HasComponent<PlayerTag>(hitbox.Owner))
                    //{
                    //    ecb.AddComponent(hitbox.Owner, new TrackInfo()
                    //    {
                    //        IsDirection = true,    //false,
                    //        LastKnownVector = energy.SourcePosition, //energy.ForceNormal * energy.BaseDamage
                    //        DamageAmount = energy.BaseDamage
                    //    });
                    //}
                }
            }
            foreach (var (/*energy,*/ health,entity) in SystemAPI.Query</*Energy,*/ RefRW<Health>>().WithAll</*Energy,*/ Health>().WithEntityAccess())
            {
                //UnityEngine.Debug.Log(hitbox.ValueRO.Owner.Index+"said Ouch");
                //health.ValueRW.RemainHealth -= energy.BaseDamage;
                if (!SystemAPI.HasBuffer<StackedEnergy>(entity))
                    continue;

                var stackedEnergy = SystemAPI.GetBuffer<StackedEnergy>(entity);

                if (stackedEnergy.Length <= 0) continue;

                var energy = stackedEnergy[0].Energy;
                for (int i = 1; i < stackedEnergy.Length; ++i)
                {
                    if(energy.BaseDamage < stackedEnergy[i].Energy.BaseDamage)
                    {
                        energy = stackedEnergy[i].Energy;
                    }
                }
                stackedEnergy.Clear();

                    health.ValueRW.RemainHealth -= energy.BaseDamage;
                    

                    if (SystemAPI.HasComponent<PlayerTag>(entity))
                    {
                        
                        ecb.AddComponent(entity, new TrackInfo()
                        {
                            IsDirection = true,    //false,
                            LastKnownVector = energy.SourcePosition, //energy.ForceNormal * energy.BaseDamage
                            DamageAmount = energy.BaseDamage
                        });
                    }
                
            }

            //foreach(var v in SystemAPI.Query<PhysicsCollider>().WithAll<PhysicsCollider>())
            //{


            //}
            //foreach (var (energy, health) in SystemAPI.Query<Energy,  RefRW<Health>>().WithAll<Energy, Health>())
            //{
            //    //UnityEngine.Debug.Log("Losing Health");
            //    health.ValueRW.RemainHealth -= energy.BaseDamage;
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