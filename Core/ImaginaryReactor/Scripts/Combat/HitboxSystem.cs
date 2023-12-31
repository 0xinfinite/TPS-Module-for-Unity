using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

namespace ImaginaryReactor {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    //[UpdateBefore(typeof(EnergySystem))]
    //[UpdateAfter(typeof(SimulationSystem))]
    public partial struct /*HitboxSystem*/SeperatedChildSystem : ISystem
    {
        //public EntityCommandBuffer _ecb;
        //ComponentLookup<LocalToWorld> _LTWLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            //_LTWLookup = state.GetComponentLookup<LocalToWorld>(false);
            //_ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        }

        public void OnDestroy(ref SystemState state)
        { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

            ChildJob job = new ChildJob
            {
                LTWLookup = state.GetComponentLookup<LocalToWorld>(false)//_LTWLookup
                                                                         //ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged) //_ecb//SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
            };
            //job.Schedule();
            state.Dependency = job.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
     public partial struct ChildJob : IJobEntity
        {
            //public float DeltaTime;
            //public PhysicsWorld PhysicsWorld;
            //public EntityCommandBuffer ecb;
            public ComponentLookup<LocalToWorld> LTWLookup;

            void Execute(
                Entity entity,
                //ref ThirdPersonCharacterControl cc,
                ref //LocalToWorld ltw,
                    LocalTransform transform,
                in SeperatedChild child//,
                                       //in WeaponControl cameraControl,
                                       //in DynamicBuffer<WeaponIgnoredEntityBufferElement> ignoredEntitiesBuffer
                )
            {
                //ltw.Value = 
                if (LTWLookup.TryGetComponent(child.Parent, out LocalToWorld parentLTW))
                {
                    //UnityEngine.Debug.Log("Parenting");
                    //transform.Position = child.LocalPosition;
                    //transform.Rotation = child.LocalRotation;
                    //ltw.Value = float4x4.TRS(
                    transform.Position = parentLTW.Value.TransformPoint(in child.LocalPosition); //,
                    transform.Rotation = quaternion.LookRotation(parentLTW.Value.TransformDirection(in child.LocalForward), parentLTW.Value.TransformDirection(in child.LocalUp));  // , ltw.Value.Scale());
                }
            }
        }
    }
}