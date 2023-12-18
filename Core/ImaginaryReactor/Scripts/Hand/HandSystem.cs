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
                CCLookUp = state.GetComponentLookup<ThirdPersonCharacterControl>(true),
                WeaponLookUp = state.GetComponentLookup<Weapon>(true),
                //LocalToWorldLookUp = state.GetComponentLookup<LocalToWorld>(false),
                //HandLookUp = state.GetComponentLookup<Hand>(true),
                //AimingSightsLookUp = state.GetComponentLookup<AimingSights>(true),
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
            public ComponentLookup<ThirdPersonCharacterControl> CCLookUp;
            [ReadOnly]
            public ComponentLookup<Weapon> WeaponLookUp;
            //public ComponentLookup<LocalToWorld> LocalToWorldLookUp;
            //[ReadOnly]
            //public ComponentLookup<Hand> HandLookUp;
            //public ComponentLookup<AimingSights> AimingSightsLookUp;
            public EntityCommandBuffer ecb;

            [BurstCompile]
            public void Execute(TriggerEvent triggerEvent)
            {


                if (WeaponLookUp.TryGetComponent(triggerEvent.EntityA, out Weapon weapon) && CCLookUp.TryGetComponent(triggerEvent.EntityB, out ThirdPersonCharacterControl cc)) //AimingSightsLookUp.TryGetComponent(triggerEvent.EntityB, out AimingSights sights))
                {
                    if (cc.Interact)
                    {
                        ecb.AddComponent(triggerEvent.EntityB, weapon);
                    }
                }
                else if (WeaponLookUp.TryGetComponent(triggerEvent.EntityB, out weapon) && CCLookUp.TryGetComponent(triggerEvent.EntityA, out cc)) //AimingSightsLookUp.TryGetComponent(triggerEvent.EntityA, out sights))
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