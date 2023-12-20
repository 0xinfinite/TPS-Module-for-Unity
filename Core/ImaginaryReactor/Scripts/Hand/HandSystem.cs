using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Collections;

namespace ImaginaryReactor {
    [UpdateBefore(typeof(ThirdPersonCharacterControl))]
    public partial struct HandSystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //foreach (var characterControl in SystemAPI.Query<RefRO<ThirdPersonCharacterControl>>().WithAll<Simulate>())
            //{
            //    if (characterControl.ValueRO.Interact)
            //    {

            //    }

            //    //if (characterControl.ValueRO.Fire)
            //    //{
            //    //var triggingWeaponJob = new TriggingWeaponJob
            //    //{
            //    //    trigged = characterControl.ValueRO.Fire,
            //    //};

            //    //triggingWeaponJob.Schedule();
            //    //}
            //}
            var grabJob = new GrabWeaponJob
            {
                CCLookup = state.GetComponentLookup<ThirdPersonCharacterControl>(true),
                WeaponLookup = state.GetComponentLookup<Weapon>(true),
                //LocalToWorldLookup = state.GetComponentLookup<LocalToWorld>(false),
                //HandLookup = state.GetComponentLookup<Hand>(true),
                //AimingSightsLookup = state.GetComponentLookup<AimingSights>(true),
                ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
            };//.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(),state.Dependency);//.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

            state.Dependency = grabJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
            state.Dependency.Complete();



        }

        //[BurstCompile]
        //namespace ImaginaryReactor { public partial  struct TriggingWeaponJob : IJobEntity
        //{
        //    [ReadOnly]public bool trigged;

        //    [BurstCompile]
        //    public void Execute(Entity entity, Weapon weapon)
        //    {
        //        weapon.IsFired = trigged;
        //    }
        //}

        [BurstCompile]
    public partial struct GrabWeaponJob : ITriggerEventsJob
        {
            [ReadOnly]
            public ComponentLookup<ThirdPersonCharacterControl> CCLookup;
            [ReadOnly]
            public ComponentLookup<Weapon> WeaponLookup;
            //public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            //[ReadOnly]
            //public ComponentLookup<Hand> HandLookup;
            //public ComponentLookup<AimingSights> AimingSightsLookup;
            public EntityCommandBuffer ecb;

            [BurstCompile]
            public void Execute(TriggerEvent triggerEvent)
            {


                if (WeaponLookup.TryGetComponent(triggerEvent.EntityA, out Weapon weapon) && CCLookup.TryGetComponent(triggerEvent.EntityB, out ThirdPersonCharacterControl cc)) //AimingSightsLookup.TryGetComponent(triggerEvent.EntityB, out AimingSights sights))
                {
                    if (cc.Interact)
                    {
                        ecb.AddComponent(triggerEvent.EntityB, weapon);
                    }
                }
                else if (WeaponLookup.TryGetComponent(triggerEvent.EntityB, out weapon) && CCLookup.TryGetComponent(triggerEvent.EntityA, out cc)) //AimingSightsLookup.TryGetComponent(triggerEvent.EntityA, out sights))
                {
                    if (cc.Interact)
                    {
                        ecb.AddComponent(triggerEvent.EntityA, weapon);
                    }
                }



            }


        }
    }
}