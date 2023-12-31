using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics.Systems;
using Unity.CharacterController;
using UnityEngine.InputSystem;

namespace ImaginaryReactor { 
[UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial  class ThirdPersonPlayerInputsSystem : SystemBase
{
        private ThirdPersonControlInput input;
        //public bool fired;
        public bool switchView;
        public bool throwRelease;
        public bool jump;

    protected override void OnCreate()
    {
        RequireForUpdate<FixedTickSystem.Singleton>();
        RequireForUpdate(SystemAPI.QueryBuilder().WithAll<ThirdPersonPlayer, ThirdPersonPlayerInputs, PlayerTag>().Build());

            input = new ThirdPersonControlInput();
            //input.Jumper.Fire.performed += OnFirePressing;
            input.Jumper.Jump.performed += OnJumpPress;
            input.Jumper.ThrowRelease.performed += OnThrowRelease;
            input.Jumper.SwitchView.performed += OnSwitchViewPress;
            input.Enable();
    }

        //private void OnAimDownSights(InputAction.CallbackContext obj)
        //{

        //}

        protected override void OnDestroy()
        {
            //input.Jumper.Fire.performed -= OnFirePressing;
            input.Jumper.Jump.performed -= OnJumpPress;
            input.Jumper.ThrowRelease.performed -= OnThrowRelease;
            input.Jumper.SwitchView.performed -= OnSwitchViewPress;
            input.Disable();
            
        }
        private void OnJumpPress(InputAction.CallbackContext obj)//ref ThirdPersonPlayerInputs playerInputs, uint fixedTick)
        {
            jump = true;
        }
        private void OnSwitchViewPress(InputAction.CallbackContext obj)//ref ThirdPersonPlayerInputs playerInputs, uint fixedTick)
        {
            switchView = true;
        }
        private void OnThrowRelease(InputAction.CallbackContext obj)//ref ThirdPersonPlayerInputs playerInputs, uint fixedTick)
        {
            throwRelease = true;
        }

        //private void OnFirePressing(InputAction.CallbackContext obj )//ref ThirdPersonPlayerInputs playerInputs, uint fixedTick)
        //{
        //    fired = true;//playerInputs.FirePressed.Set(fixedTick);
        //}

        protected override void OnUpdate()
    {
        uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

            foreach (var (playerInputs, player, playerTag) in SystemAPI.Query<RefRW<ThirdPersonPlayerInputs>, ThirdPersonPlayer, PlayerTag>())
            {
                //if (InputSystemManager.instance != null)
                {
                    

                    playerInputs.ValueRW.MoveInput = input.Jumper.Move.ReadValue<Vector2>();// new float2();

                    float ads = input.Jumper.ADS.ReadValue<float>();
                    playerInputs.ValueRW.CameraZoomInput = ads;
                    //UnityEngine.Debug.Log("ADS : "+ads);
                    float2 padInput = math.tan(
                        input.Jumper.Look.ReadValue<Vector2>()
                        ) / (math.PI * 0.5f)
                        ;
                    float2 mouseInput = input.Jumper.MouseLook.ReadValue<Vector2>();
                    playerInputs.ValueRW.CameraLookInput = padInput;
                    bool isXInputMouse = math.abs(padInput.x) < math.abs(mouseInput.x);
                    bool isYInputMouse = math.abs(padInput.y) < math.abs(mouseInput.y);
                    playerInputs.ValueRW.CameraLookInput = new float2(isXInputMouse ? mouseInput.x : padInput.x,
                        isYInputMouse ? mouseInput.y : padInput.y
                        );
                    playerInputs.ValueRW.IsMouseInput = isXInputMouse || isYInputMouse;

                    //playerInputs.ValueRW.CameraLookInput.x = (1 - math.sqrt(math.cos(playerInputs.ValueRW.CameraLookInput.x * math.PI * 0.5f))) * playerInputs.ValueRW.CameraLookInput.x > 0 ? 1 : -1;
                    //playerInputs.ValueRW.CameraLookInput.y = (1 - math.sqrt(math.cos(playerInputs.ValueRW.CameraLookInput.y * math.PI * 0.5f))) * playerInputs.ValueRW.CameraLookInput.y > 0 ? 1 : -1;
                    //* math.lerp(1,0.25f,ads) ;//new float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                    //playerInputs.ValueRW.CameraZoomInput = -Input.mouseScrollDelta.y;
                    float verticalDeadzone = 0.1f* (1-ads);
                    playerInputs.ValueRW.CameraLookInput.y =
                         math.abs(playerInputs.ValueRW.CameraLookInput.y) < verticalDeadzone ? 0 :
                        (playerInputs.ValueRW.CameraLookInput.y > 0 ? 
                        math.remap(verticalDeadzone, 1,0,1, playerInputs.ValueRW.CameraLookInput.y) :
                        math.remap(-verticalDeadzone, -1, 0, -1, playerInputs.ValueRW.CameraLookInput.y));


                    // For button presses that need to be queried during fixed update, use the "FixedInputEvent" helper struct.
                    // This is part of a strategy for proper handling of button press events that are consumed during the fixed update group
                    if (jump||Input.GetKeyDown(KeyCode.Space))
                    {
                        playerInputs.ValueRW.JumpPressed.Set(fixedTick);
                    }
                    playerInputs.ValueRW.FloatingInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
                    if (input.Jumper.Fire.ReadValue<float>()>0.5f)//(fired)//(Input.GetMouseButton(0))
                    {
                        playerInputs.ValueRW.FirePressed.Set(fixedTick);
                    }
                    //fired = false;
                    if (Input.GetMouseButtonDown(2))
                    {
                        playerInputs.ValueRW.ThrowPress.Set(fixedTick);
                    }
                    if (throwRelease||Input.GetMouseButtonUp(2))
                    {
                        playerInputs.ValueRW.ThrowRelease.Set(fixedTick);
                    }
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        playerInputs.ValueRW.InteractPressed.Set(fixedTick);
                    }
                    if (switchView||Input.GetKeyDown(KeyCode.Tab))
                    {
                        playerInputs.ValueRW.SwitchViewPressed.Set(fixedTick);
                    }
                }
                //else
                //{
                //    playerInputs.ValueRW.MoveInput = new float2();
                //    playerInputs.ValueRW.MoveInput.y += Input.GetKey(KeyCode.W) ? 1f : 0f;
                //    playerInputs.ValueRW.MoveInput.y += Input.GetKey(KeyCode.S) ? -1f : 0f;
                //    playerInputs.ValueRW.MoveInput.x += Input.GetKey(KeyCode.D) ? 1f : 0f;
                //    playerInputs.ValueRW.MoveInput.x += Input.GetKey(KeyCode.A) ? -1f : 0f;

                //    playerInputs.ValueRW.CameraLookInput = new float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                //    //playerInputs.ValueRW.CameraZoomInput = -Input.mouseScrollDelta.y;
                //    playerInputs.ValueRW.CameraZoomInput = Input.GetMouseButton(1) ? 1 : 0;

                //    // For button presses that need to be queried during fixed update, use the "FixedInputEvent" helper struct.
                //    // This is part of a strategy for proper handling of button press events that are consumed during the fixed update group
                //    if (Input.GetKeyDown(KeyCode.Space))
                //    {
                //        playerInputs.ValueRW.JumpPressed.Set(fixedTick);
                //    }
                //    playerInputs.ValueRW.FloatingInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
                //    if (Input.GetMouseButton(0))
                //    {
                //        playerInputs.ValueRW.FirePressed.Set(fixedTick);
                //    }
                //    if (Input.GetMouseButtonDown(2))
                //    {
                //        playerInputs.ValueRW.ThrowPress.Set(fixedTick);
                //    }
                //    if (Input.GetMouseButtonUp(2))
                //    {
                //        playerInputs.ValueRW.ThrowRelease.Set(fixedTick);
                //    }
                //    if (Input.GetKeyDown(KeyCode.F))
                //    {
                //        playerInputs.ValueRW.InteractPressed.Set(fixedTick);
                //    }
                //    if (Input.GetKeyDown(KeyCode.Tab))
                //    {
                //        playerInputs.ValueRW.SwitchViewPressed.Set(fixedTick);
                //    }
                //}
                switchView = false;
                throwRelease = false;
                jump = false;
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
            var tick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;
        foreach (var (playerInputs, player, playerTag) in SystemAPI.Query<ThirdPersonPlayerInputs, ThirdPersonPlayer, PlayerTag>().WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<OrbitCameraControl>(player.ControlledCamera))
            {
                OrbitCameraControl cameraControl = SystemAPI.GetComponent<OrbitCameraControl>(player.ControlledCamera);
                
                cameraControl.FollowedCharacterEntity = player.ControlledCharacter;
                cameraControl.Look = playerInputs.CameraLookInput;
                    cameraControl.ToggleZoom =// playerInputs.CameraZoomInput > 0.5f ?
                        cameraControl.Zoom < 0.0001f ? (playerInputs.CameraZoomInput > 0.0001f ? true : false) : false
                        ;
                            //:
                        //cameraControl.Zoom > 0.9999f ? (playerInputs.CameraZoomInput < 0.9999f?true:false) : false
                        //    ;
                cameraControl.Zoom = playerInputs.CameraZoomInput;
                    cameraControl.IsMouseInput = playerInputs.IsMouseInput;

                    cameraControl.SwitchView = playerInputs.SwitchViewPressed.IsSet(tick);

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

                    characterControl.ThrowPress = playerInputs.ValueRW.ThrowPress.IsSet(fixedTick);
                    characterControl.ThrowRelease = playerInputs.ValueRW.ThrowRelease.IsSet(fixedTick);

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