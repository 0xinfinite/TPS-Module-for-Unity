
using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace ImaginaryReactor
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(ThirdPersonAIInputsSystem))]
    [BurstCompile]
    public partial struct LoudSoundSystem : ISystem
    {
        const float SpeedOfSound = 340;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var _ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            var DeltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (sound, tag) in SystemAPI.Query<RefRW<LoudSound>, PlayerTag>().WithAll<LoudSound, PlayerTag>())
            {
                float NextTickTime = (sound.ValueRO.ElapsedTime + DeltaTime);

                foreach (var (brain, ltw, entity) in SystemAPI.Query<Brain, LocalToWorld>().WithEntityAccess())
                {
                    float distance = math.length(ltw.Position - sound.ValueRO.Source);
                    if (distance < sound.ValueRO.Range && distance > sound.ValueRO.ElapsedTime * SpeedOfSound && distance < NextTickTime * SpeedOfSound)
                    {
                        _ecb.AddComponent(entity, new HeardSound() { SoundSource = sound.ValueRO.Source, Recognized = false, playerSide = true });
                    }
                }
                sound.ValueRW.ElapsedTime = NextTickTime;
            }

            SoundJob soundJob = new SoundJob()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                ecb = _ecb,
            };

            state.Dependency = soundJob.Schedule(state.Dependency);
        }

        [BurstCompile]
    public partial struct SoundJob : IJobEntity
        {
            [ReadOnly] public float DeltaTime;
            public EntityCommandBuffer ecb;

            [BurstCompile]
            void Execute(
                Entity entity,
                ref LoudSound sound
                )
            {
                if (sound.ElapsedTime > (sound.Range + 1) / SpeedOfSound)
                {
                    ecb.DestroyEntity(entity);
                }
                else
                {
                    sound.ElapsedTime += DeltaTime;
                }
            }
        }
    }
}