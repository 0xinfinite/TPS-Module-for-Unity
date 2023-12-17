using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Extensions;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//[UpdateInGroup(typeof(KinematicCharacterPhysicsUpdateGroup))]
//[UpdateAfter(typeof(ThirdPersonCharacterPhysicsUpdateSystem))]
public partial class ModelSyncSystem : SystemBase
{
    const int Idle = 1;
    const int Movement = 2;


    protected override void OnCreate()
    {
    }

    protected override void OnUpdate()
    {
        if (ModelSyncManager.instance)
        {
            var manager = ModelSyncManager.instance;
            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            
            foreach (var (mc, ltw, entity) in SystemAPI.Query<ModelSyncComponent, LocalToWorld>().WithEntityAccess())
            {
                manager.SetPosition(mc.ID, ltw.Position);

                var isValuting = SystemAPI.HasComponent<Vaulting>(entity);

                manager.SetBool(mc.ID, "Vault", isValuting);

                for(int i = 0; manager.clearBodyQueue.Count > 0 && i < manager.clearBodyQueue.Count ;)
                {
                    int clearRequestId = manager.clearBodyQueue.Peek();

                    if(mc.ID == clearRequestId)
                    {
                        manager.clearBodyQueue.Dequeue();
                        ecb.AddComponent(entity, new ClearThisCharacterBody());
                        ecb.RemoveComponent<CharacterInterpolation>(entity);
                    }
                    else
                    {
                        ++i;
                    }
                }
                bool pushRootMotion = false;
                if (!SystemAPI.HasComponent<Health>(entity))
                {
                    manager.SetTrigger(mc.ID, "KnockedOut");

                    pushRootMotion = true;
                }
                else
                {
                    if (SystemAPI.HasComponent<KinematicCharacterBody>(entity))
                    {
                            var body = SystemAPI.GetComponent<KinematicCharacterBody>(entity);

                        bool isMoving = math.lengthsq(body.RelativeVelocity) > 0.01;
                        if (isMoving && (mc.rootMotionFlag & Movement) != 0)
                        {
                            pushRootMotion = true;
                        }
                        else if(!isMoving && (mc.rootMotionFlag & Idle) != 0)
                        {
                            pushRootMotion = true;
                        }   
                        else
                        {
                            var velocity = body.RelativeVelocity;
                            var moveDirection = math.normalizesafe(velocity);
                            manager.SetBool(mc.ID, "IsGrounded", body.IsGrounded || isValuting);
                            //if (SystemAPI.HasComponent<CancelVaulting>(entity))
                            //{
                            //}
                            manager.SetFloat(mc.ID, "SpeedForwardal", math.length(math.projectsafe(velocity, ltw.Forward)) * (math.dot(moveDirection, ltw.Forward) > 0 ? 1 : -1));
                            manager.SetFloat(mc.ID, "SpeedStrafe", math.length(math.projectsafe(velocity, ltw.Right)) * (math.dot(moveDirection, ltw.Right) > 0 ? 1 : -1));

                            if (manager.modelDict[mc.ID].alwaysTrackingSight)
                            {
                                manager.SetRotation(mc.ID, ltw.Rotation);
                            }
                            else
                            {
                                if (SystemAPI.HasComponent<ThirdPersonCharacterControl>(entity))
                                {
                                    var control = SystemAPI.GetComponent<ThirdPersonCharacterControl>(entity);
                                    if (math.lengthsq(control.MoveVector) > 0.01 || control.Fire)
                                    {
                                        manager.SetRotation(mc.ID, ltw.Rotation);
                                    }
                                }
                            }
                        }

                    }
                    if (SystemAPI.HasComponent<CancelVaulting>(entity))
                    {
                        manager.SetBool(mc.ID, "VaultCancel", true);
                        ecb.RemoveComponent<CancelVaulting>(entity);
                    }
                    else
                    {
                        manager.SetBool(mc.ID, "VaultCancel", false);
                    }

                    if (SystemAPI.HasComponent<HitscanLineComponent>(entity))
                    {
                        var lineComponent = SystemAPI.GetComponent<HitscanLineComponent>(entity);
                        manager.PushLine(mc.ID, lineComponent.StartPos, lineComponent.EndPos);
                        ecb.RemoveComponent<HitscanLineComponent>(entity);
                    }
                }

                if (pushRootMotion)
                {
                    if (SystemAPI.HasComponent<KinematicCharacterBody>(entity))//&& SystemAPI.HasComponent<ThirdPersonCharacterComponent>(entity))
                    {
                        ecb.AddComponent(entity, new RootMotionComponent()
                        {
                            Velocity = (float3)manager.modelDict[mc.ID].animator.velocity
                        });
                    }
                    else
                    {
                        if (SystemAPI.HasComponent<PhysicsVelocity>(entity))
                        {
                            var pv = SystemAPI.GetComponent<PhysicsVelocity>(entity);
                            float y = pv.Linear.y;
                            //    pv.ApplyLinearImpulse(SystemAPI.GetComponent<PhysicsMass>(entity), (float3)manager.modelDict[mc.ID].animator.velocity);//
                            pv.Linear = (float3)manager.modelDict[mc.ID].animator.velocity + new float3(0, y, 0);
                            SystemAPI.SetComponent(entity, pv);
                        }

                        else
                        {
                            float3 newPos = ltw.Position + (float3)manager.modelDict[mc.ID].animator.velocity * SystemAPI.Time.DeltaTime;
                            SystemAPI.SetComponent(entity, new LocalTransform()
                            {
                                Position = newPos,
                                Rotation = ltw.Rotation
                            });

                            SystemAPI.SetComponent(entity, new LocalToWorld()
                            {
                                Value = new float4x4(ltw.Rotation, newPos)
                            });
                        }
                    }
                }
            }

            //foreach(var (tag, entity) in SystemAPI.Query<CancelVaulting>().WithEntityAccess())
            //{
            //    ecb.RemoveComponent<CancelVaulting>(entity);
            //}
        }
        }
    }
