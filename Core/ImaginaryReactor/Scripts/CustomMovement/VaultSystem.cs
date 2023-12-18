using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.CharacterController;

//[UpdateInGroup(typeof(InitializationSystemGroup))]

namespace ImaginaryReactor
{
    [UpdateInGroup(typeof(KinematicCharacterPhysicsUpdateGroup))]
    [UpdateAfter(typeof(ThirdPersonCharacterPhysicsUpdateSystem))]
    [BurstCompile]
    public partial struct VaultSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            //state.RequireForUpdate<ThirdPersonCharacterPhysicsUpdateSystem>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        unsafe
        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
            var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var DeltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (vault, cc, vc, ltw, transform,
                interpolation,// velocity,
                entity) in SystemAPI.Query<RefRW<Vaulting>, ThirdPersonCharacterControl, VaultComponent
                , RefRW<LocalToWorld>
                , RefRW<LocalTransform>
                , RefRW<
                    CharacterInterpolation
                    >
                //, RefRW<PhysicsVelocity>
                >()
                .WithEntityAccess())
            {
                bool isCancelVaulting = (math.lengthsq(cc.MoveVector) > 0.01f && math.dot(math.normalizesafe(vault.ValueRO.VaultTargetPosition - vault.ValueRO.VaultStartPosition), cc.MoveVector) < 0);

                if (vault.ValueRO.VaultRemainTime <= 0 || isCancelVaulting)
                {
                    vault.ValueRW.VaultRemainTime = vc.VaultDuration + 0.001f;
                    interpolation.ValueRW.InterpolationFromTransform.pos = ltw.ValueRO.Position;
                    interpolation.
                        ValueRW.
                        SkipNextInterpolation();
                    if (SystemAPI.HasComponent<KinematicCharacterBody>(entity))
                    {
                        KinematicCharacterBody body = SystemAPI.GetComponent<KinematicCharacterBody>(entity);
                        body.RelativeVelocity = float3.zero;
                        SystemAPI.SetComponent(entity, body);
                    }
                    if (isCancelVaulting)
                    {
                        ecb.AddComponent(entity, new CancelVaulting());
                    }
                    ecb.SetComponentEnabled<KinematicCharacterBody>(entity, true);
                    //interpolation.ValueRW.InterpolatePosition = (byte)1;
                    //ecb.RemoveComponent<CharacterInterpolation>(entity);
                    ecb.RemoveComponent<Vaulting>(entity);
                }
                else if (vault.ValueRO.VaultRemainTime > 0 && vault.ValueRO.VaultRemainTime < vc.VaultDuration)
                {
                    //UnityEngine.Debug.Log("Vaulting : "+interpolation.ValueRW.InterpolationFromTransform.pos);
                    float time = (vc.VaultDuration - vault.ValueRO.VaultRemainTime) / vc.VaultDuration;
                    float3 pos = math.lerp(vault.ValueRO.VaultStartPosition, vault.ValueRO.VaultTargetPosition,
                        time);

                    pos.y = math.lerp(vault.ValueRO.VaultStartPosition.y, vault.ValueRO.VaultTargetPosition.y, math.sqrt(math.sin(time * math.PI * 0.5f)));

                    ltw.ValueRW.Value = new float4x4(ltw.ValueRO.Rotation, pos);
                    //interpolation.ValueRW.InterpolationFromTransform.pos = ltw.ValueRO.Position;
                    transform.ValueRW.Position = pos;
                    //interpolation.
                    //    ValueRW.
                    //    SkipNextInterpolation();
                    vault.ValueRW.VaultRemainTime -= DeltaTime;
                }

            }

            foreach (var (vc, cc, body, ltw, /*interpolation,*/ col, entity) in SystemAPI.Query<RefRW<VaultComponent>, ThirdPersonCharacterControl, KinematicCharacterBody
                , RefRW<LocalToWorld>
                , PhysicsCollider
                //,RefRW<CharacterInterpolation>
                >()
            //.WithAll<VaultComponent, ThirdPersonCharacterControl, KinematicCharacterBody, LocalToWorld>()
            .WithEntityAccess())
            {
                var hands = entityManager.GetBuffer<VaultHand>(entity);

                if (!body.IsGrounded)
                {
                    //UnityEngine.Debug.Log("Vault Ready");
                    for (int i = 0; i < hands.Length; i++)
                    {
                        RaycastInput input = new RaycastInput()
                        {
                            Start = SystemAPI.GetComponent<LocalToWorld>(hands[i].ValutRayStart).Position,
                            End = SystemAPI.GetComponent<LocalToWorld>(hands[i].ValutRayEnd).Position,
                            Filter = vc.ValueRO.Filter
                        };

                        if (math.lengthsq(cc.MoveVector) > 0.01f && math.dot(math.normalizesafe(input.End - input.Start), cc.MoveVector) > 0)
                        {
                            //UnityEngine.Debug.Log("Inputing Vault");
                            if (physicsWorld.CastRay(input, out RaycastHit hit))
                            {
                                //UnityEngine.Debug.Log("Hit something while Vault input : " + hit.SurfaceNormal.y);
                                if (hit.SurfaceNormal.y > 0.713024f)
                                {
                                    //UnityEngine.Debug.Log("Try to Vault");
                                    float3 targetPos = hit.Position +
                                        ltw.ValueRO.Right * vc.ValueRO.VaultOffset.x + ltw.ValueRO.Up * vc.ValueRO.VaultOffset.y + ltw.ValueRO.Forward * vc.ValueRO.VaultOffset.z;

                                    ColliderCastInput colliderInput = new ColliderCastInput()
                                    {
                                        Collider = col.ColliderPtr,
                                        Start = targetPos,
                                        End = targetPos
                                    };

                                    if (!physicsWorld.CastCollider(colliderInput, out ColliderCastHit colHit))
                                    {
                                        ecb.AddComponent(entity, new Vaulting()
                                        {
                                            VaultStartPosition = ltw.ValueRO.Position,
                                            VaultTargetPosition = targetPos,
                                            VaultRemainTime = vc.ValueRO.VaultDuration - 0.001f
                                        });

                                        ecb.SetComponentEnabled<KinematicCharacterBody>(entity, false);
                                        //interpolation.ValueRW.InterpolatePosition = (byte)0;
                                        break;
                                    }
                                }
                            }
                        }

                    }
                }
            }

        }
    }
}