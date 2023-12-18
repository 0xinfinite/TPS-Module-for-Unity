using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics.Systems;
using Unity.CharacterController;

namespace ImaginaryReactor { 
[UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial  class ThirdPersonPlayerInputsSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<FixedTickSystem.Singleton>();
        RequireForUpdate(SystemAPI.QueryBuilder().WithAll<ThirdPersonPlayer, ThirdPersonPlayerInputs, PlayerTag>().Build());
    }
    
    protected override void OnUpdate()
    {
        uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

        foreach (var (playerInputs, player, playerTag) in SystemAPI.Query<RefRW<ThirdPersonPlayerInputs>, ThirdPersonPlayer, PlayerTag>())
        {
            playerInputs.ValueRW.MoveInput = new float2();
            playerInputs.ValueRW.MoveInput.y += Input.GetKey(KeyCode.W) ? 1f : 0f;
            playerInputs.ValueRW.MoveInput.y += Input.GetKey(KeyCode.S) ? -1f : 0f;
            playerInputs.ValueRW.MoveInput.x += Input.GetKey(KeyCode.D) ? 1f : 0f;
            playerInputs.ValueRW.MoveInput.x += Input.GetKey(KeyCode.A) ? -1f : 0f;
            
            playerInputs.ValueRW.CameraLookInput = new float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            //playerInputs.ValueRW.CameraZoomInput = -Input.mouseScrollDelta.y;
            
            // For button presses that need to be queried during fixed update, use the "FixedInputEvent" helper struct.
            // This is part of a strategy for proper handling of button press events that are consumed during the fixed update group
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerInputs.ValueRW.JumpPressed.Set(fixedTick);
            }
            playerInputs.ValueRW.FloatingInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
            if (Input.GetMouseButton(0))
            {
                playerInputs.ValueRW.FirePressed.Set(fixedTick); 
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                playerInputs.ValueRW.InteractPressed.Set(fixedTick);
            }
        }
    }
}

/// <summary>
/// Apply inputs that need to be read at a variable rate
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
 public partial  struct ThirdPersonPlayerVariableStepControlSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<ThirdPersonPlayer, ThirdPersonPlayerInputs, PlayerTag>().Build());
    }

    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerInputs, player, playerTag) in SystemAPI.Query<ThirdPersonPlayerInputs, ThirdPersonPlayer, PlayerTag>().WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<OrbitCameraControl>(player.ControlledCamera))
            {
                OrbitCameraControl cameraControl = SystemAPI.GetComponent<OrbitCameraControl>(player.ControlledCamera);
                
                cameraControl.FollowedCharacterEntity = player.ControlledCharacter;
                cameraControl.Look = playerInputs.CameraLookInput;
                cameraControl.Zoom = playerInputs.CameraZoomInput;

                SystemAPI.SetComponent(player.ControlledCamera, cameraControl);
            }
        }
    }
}

/// <summary>
/// Apply inputs that need to be read at a fixed rate.
/// It is necessary to handle this as part of the fixed step group, in case your framerate is lower than the fixed step rate.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
[BurstCompile]
         public partial struct ThirdPersonPlayerFixedStepControlSystem : ISystem
            {
                [BurstCompile]
                public void OnCreate(ref SystemState state)
                {
                    state.RequireForUpdate<FixedTickSystem.Singleton>();
                    state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<ThirdPersonPlayer, ThirdPersonPlayerInputs>().Build());

                    //foreach (var player in SystemAPI.Query<ThirdPersonPlayer>().WithAll<Simulate>())
                    //{
                    //    if(player.InitWeapon != Entity.Null)
                    //    {
                    //        SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).
                    //            AddComponent<Weapon>(player.ControlledCharacter,SystemAPI.GetComponent<Weapon>( player.InitWeapon) );
                    //    }
                    //}
                }

                public void OnDestroy(ref SystemState state)
                { }

                [BurstCompile]
                public void OnUpdate(ref SystemState state)
                {
                    uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

                    foreach (var (playerInputs, player) in SystemAPI.Query<RefRW<ThirdPersonPlayerInputs>, ThirdPersonPlayer>().WithAll<Simulate>())
                    {
                        if (SystemAPI.HasComponent<ThirdPersonCharacterControl>(player.ControlledCharacter))
                        {
                            ThirdPersonCharacterControl characterControl = SystemAPI.GetComponent<ThirdPersonCharacterControl>(player.ControlledCharacter);

                            float3 characterUp = MathUtilities.GetUpFromRotation(SystemAPI.GetComponent<LocalTransform>(player.ControlledCharacter).Rotation);

                            // Get camera rotation data, since our movement is relative to it
                            quaternion cameraRotation = quaternion.identity;
                            if (SystemAPI.HasComponent<LocalTransform>(player.ControlledCamera))
                            {
                                cameraRotation = SystemAPI.GetComponent<LocalTransform>(player.ControlledCamera).Rotation;
                            }
                            float3 cameraForwardOnUpPlane = math.normalizesafe(MathUtilities.ProjectOnPlane(MathUtilities.GetForwardFromRotation(cameraRotation), characterUp));
                            float3 cameraRight = MathUtilities.GetRightFromRotation(cameraRotation);

                            characterControl.CameraForwardVector = cameraForwardOnUpPlane;

                            // Move
                            characterControl.MoveVector = (playerInputs.ValueRW.MoveInput.y * cameraForwardOnUpPlane) + (playerInputs.ValueRW.MoveInput.x * cameraRight);
                            characterControl.MoveVector = MathUtilities.ClampToMaxLength(characterControl.MoveVector, 1f);

                            // Jump
                            // We use the "FixedInputEvent" helper struct here to detect if the event needs to be processed.
                            // This is part of a strategy for proper handling of button press events that are consumed during the fixed update group.
                            //characterControl.Jump = playerInputs.ValueRW.JumpPressed.IsSet(fixedTick);
                            characterControl.LastJumpPressedTime++;
                            if (playerInputs.ValueRW.JumpPressed.IsSet(fixedTick))//characterControl.Jump)
                            {
                                characterControl.Jump = true;
                                characterControl.LastJumpPressedTime = 0;
                            }
                            if (characterControl.LastJumpPressedTime > 10)
                            {
                                characterControl.Jump = false;
                            }
                            characterControl.Floating = playerInputs.ValueRW.FloatingInput;

                            bool trigged = playerInputs.ValueRW.FirePressed.IsSet(fixedTick);
                            characterControl.Fire = trigged;
                            //if (hasWeapon)
                            //{
                            //    Weapon weapon = SystemAPI.GetComponent<Weapon>(player.ControlledWeapon);

                            //    weapon.IsFired = trigged;
                            //    //Debug.Log(trigged);
                            //    SystemAPI.SetComponent(player.ControlledWeapon, weapon);
                            //}

                            bool grabbed = playerInputs.ValueRW.InteractPressed.IsSet(fixedTick);
                            characterControl.Interact = grabbed;

                            SystemAPI.SetComponent(player.ControlledCharacter, characterControl);

                        }
                    }


                }
            } }