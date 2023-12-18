using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ThirdPersonAIInputsSystem : ISystem
{
    [BurstCompile]
    public static void GetActualPositionOfHitbox(ref LocalToWorld ltw, ref Hitbox hitbox, ref float3 pos)
    {
        pos = ltw.Position - ltw.Forward * hitbox.Center.z - ltw.Right * hitbox.Center.x
            - ltw.Up * hitbox.Center.z;
    }

    [BurstCompile]
    void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FixedTickSystem.Singleton>();
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<ThirdPersonPlayer, ThirdPersonPlayerInputs, Brain>().Build());
    }

    [BurstCompile]
    void OnUpdate(ref SystemState state)
    {
        uint _fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

        //double ElapsedTime = SystemAPI.Time.ElapsedTime;
        //Entity targetEntity;
        //float3 _targetPosition = float3.zero;

        //foreach(var (player, ltw) in SystemAPI.Query<PlayerTag, LocalToWorld>())
        //{
        //    _targetPosition = ltw.Position;
        //}

        //foreach(var (brain,heard/*, entity*/) in SystemAPI.Query<RefRW<Brain>,RefRW<HeardSound>>().WithAll<Brain,HeardSound>())//WithEntityAccess())
        //foreach (var (brain, heard/*, entity*/) in SystemAPI.Query<RefRW<Brain>, RefRW<HeardSound>>().WithAll<Brain, HeardSound>())
        //{
        //    if (!heard.ValueRW.Recognized)
        //    {
        //        //brain.ValueRW.FinalDesirePositionToLook = heard.ValueRW.SoundSource;
        //        brain.ValueRW.ReceivedSignalInfo = new float4(heard.ValueRW.SoundSource.x, heard.ValueRW.SoundSource.y, heard.ValueRW.SoundSource.z, brain.ValueRW.ReceivedSignalInfo.w<0? 0 : brain.ValueRW.ReceivedSignalInfo.w);
        //        heard.ValueRW.Recognized = true;
        //    }
        //    //SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).RemoveComponent<HeardSound>(entity);
        //}
        //foreach(var (neural, sweep) in SystemAPI.Query<Neural, SweepInfo>().WithAll<Neural, SweepInfo>())
        //{
        //    //foreach()
        //}

        foreach (var (brain, heard, tp) in SystemAPI.Query<RefRW<Brain>, HeardSound, ThirdPersonPlayer>()
            .WithAll<Brain, HeardSound, ThirdPersonPlayer>())
        {
            if (brain.ValueRO.TargetFoundElapsed > 0.5f)
            {
                if (state.GetComponentLookup<LocalToWorld>().TryGetComponent(tp.ControlledCharacter, out var localToWorld))
                {
                    brain.ValueRW.FinalDesirePositionToLook = heard.SoundSource;//trackInfo.IsDirection ? localToWorld.Position + trackInfo.LastKnownVector * 10f : trackInfo.LastKnownVector;
                    //brain.ValueRW.TargetFoundElapsed = 0;
                }
            }
            //else
            //{ brain.ValueRW.FinalDesirePositionToLook = trackInfo.LastKnownVector; }
        }

        //foreach (var (brain, trackInfo, tp) in SystemAPI.Query<RefRW<Brain>, TrackInfo, ThirdPersonPlayer>().WithAll<Brain, TrackInfo, ThirdPersonPlayer>())
        //{
        //    //UnityEngine.Debug.Log(trackInfo.TargetKey +" VS " + brain.ValueRW.IFF_Key);

        //    bool hostile = trackInfo.TargetKey != brain.ValueRW.IFF_Key;
        //    //UnityEngine.Debug.Log("is this hostile : " + hostile);
        //    if (hostile && state.GetComponentLookup<LocalToWorld>().TryGetComponent(tp.ControlledCharacter, out var localToWorld))
        //    {
        //        brain.ValueRW.FinalDesirePositionToLook = trackInfo.IsDirection ? localToWorld.Position + trackInfo.LastKnownVector * 10f : trackInfo.LastKnownVector;
        //        brain.ValueRW.TargetFoundElapsed = 0;
        //    }
        //    //else
        //    //{ brain.ValueRW.FinalDesirePositionToLook = trackInfo.LastKnownVector; }
        //}
        float _DeltaTime = SystemAPI.Time.DeltaTime;
        var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);


        foreach (var (brain, tracking,entity) in SystemAPI.Query<RefRW<Brain>,  RefRW<TrackingTarget>>()
            .WithAll<Brain, TrackingTarget>()
            .WithEntityAccess())
        {
            //UnityEngine.Debug.Log(trackInfo.TargetKey +" VS " + brain.ValueRW.IFF_Key);

            //bool hostile = trackInfo.TargetKey != brain.ValueRW.IFF_Key;
            ////UnityEngine.Debug.Log("is this hostile : " + hostile);
            //if (hostile && state.GetComponentLookup<LocalToWorld>().TryGetComponent(tp.ControlledCharacter, out var localToWorld))
            //{
            if (SystemAPI.HasComponent<LocalToWorld>(tracking.ValueRO.TargetEntity))
            {
                float3 pos = float3.zero;
                var targetLTW = SystemAPI.GetComponent<LocalToWorld>(tracking.ValueRO.TargetEntity);
                if (SystemAPI.HasComponent<Hitbox>(tracking.ValueRO.TargetEntity))
                {
                    var hitbox = SystemAPI.GetComponent<Hitbox>(tracking.ValueRO.TargetEntity);
                    //UnityEngine.Debug.Log("Tracking Target has Hitbox");
                    GetActualPositionOfHitbox(ref targetLTW, ref hitbox, ref pos);
                }
                else
                {
                    pos = targetLTW.Position + targetLTW.Right * brain.ValueRO.AimOffset.x
                        + targetLTW.Forward * brain.ValueRO.AimOffset.z + targetLTW.Up * brain.ValueRO.AimOffset.y ;
                }
                brain.ValueRW.FinalDesirePositionToLook = pos;// trackInfo.IsDirection ? localToWorld.Position + trackInfo.LastKnownVector * 10f : trackInfo.LastKnownVector;
                brain.ValueRW.TargetFoundElapsed = 0;
            }
            tracking.ValueRW.RemainLostTime -= _DeltaTime;

            if (tracking.ValueRO.RemainLostTime <= 0)
            {
                ecb.RemoveComponent<TrackingTarget>(entity);
            }
            //}
            //else
            //{ brain.ValueRW.FinalDesirePositionToLook = trackInfo.LastKnownVector; }
        }

        foreach (var (brain, trackInfo, entity) in SystemAPI.Query<RefRW<Brain>, TrackInfo>().WithAll<Brain, TrackInfo>().WithNone<TrackingTarget>().WithEntityAccess())
        {
            ecb.AddComponent(entity, new TrackingTarget() { TargetEntity = trackInfo.TargetEntity, RemainLostTime = 0.5f });
        }

        DesireJob job = new DesireJob
        {
            LocalToWorldLookup = state.GetComponentLookup<LocalToWorld>(true),
            CCLookup = state.GetComponentLookup<ThirdPersonCharacterComponent>(true),
            //targetPosition = _targetPosition,
            DeltaTime = _DeltaTime,
            fixedTick = _fixedTick,
            //ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged)
        };

        state.Dependency = job.Schedule(state.Dependency);

        state.Dependency.Complete();


        foreach (var (trackInfo, entity) in SystemAPI.Query<TrackInfo>().WithEntityAccess())
        {
            SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged)
                .RemoveComponent<TrackInfo>(entity);
            //else
            //{ brain.ValueRW.FinalDesirePositionToLook = trackInfo.LastKnownVector; }
        }
        foreach (var (heard, entity) in SystemAPI.Query<HeardSound>().WithEntityAccess())
        {
            SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged)
                .RemoveComponent<HeardSound>(entity);
            //else
            //{ brain.ValueRW.FinalDesirePositionToLook = trackInfo.LastKnownVector; }
        }

        //BrainCleanJob cleanJob = new BrainCleanJob()
        //{
        //    ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged)
        //};

        //state.Dependency = cleanJob.Schedule(state.Dependency);
    }
}

//public partial struct TrackingJob : IJobEntity
//{
    
//}


[BurstCompile]
public partial struct DesireJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
    [ReadOnly] public ComponentLookup<ThirdPersonCharacterComponent> CCLookup;
    //[ReadOnly] public float3 targetPosition;
    public uint fixedTick;
    [ReadOnly] public float DeltaTime;
    //public EntityCommandBuffer ecb;

    [BurstCompile]
    float Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    [BurstCompile]
    void Execute(
        Entity entity,
        ref ThirdPersonPlayerInputs playerInputs,
        ref Brain brain,
        in ThirdPersonPlayer player
        )
    {
        float3 moveInput = float3.zero;
        float2 viewInput = float2.zero;

        bool targetSpotted = brain.TargetFoundElapsed < 0.5;

        if (LocalToWorldLookup.TryGetComponent(player.ControlledCharacter, out LocalToWorld ltw)&& LocalToWorldLookup.TryGetComponent(player.ControlledCamera, out LocalToWorld camLTW)
            && CCLookup.TryGetComponent(player.ControlledCharacter, out ThirdPersonCharacterComponent cc))
        {
            
            float3 targetLookPosition = brain.FinalDesirePositionToLook;

            float3 moveVector = brain.DesirePositionToMove - ltw.Position;
            moveInput = math.normalizesafe (moveVector) * math.saturate( math.length( moveVector)-0.1f);
            moveInput = math.rotate(math.inverse(quaternion.LookRotation(ltw.Forward, new float3(0,1,0))) , moveInput);

            float3 cameraTargetDir = math.lerp(camLTW.Value.InverseTransformPoint(ltw.Position + ltw.Forward), 
                math.normalizesafe( camLTW.Value.InverseTransformPoint(in targetLookPosition)),
                targetSpotted? 1 : math.clamp(math.distance(targetLookPosition,camLTW.Position),0,1)
                );// math.normalizesafe(brain.DesirePositionToLook- ltw.Position);

            viewInput = new float2(cameraTargetDir.z>0 ? cameraTargetDir.x * math.clamp( Map(math.clamp(math.abs(cameraTargetDir.x),0,1),0,1, cc.RotationSharpness,1),0, cc.RotationSharpness)
                : cameraTargetDir.x>0?1:-1,
                cameraTargetDir.y * math.clamp(Map(math.clamp(math.abs(cameraTargetDir.y), 0, 1), 0, 1, cc.RotationSharpness, 1),0, cc.RotationSharpness)
                );
        }
        playerInputs.MoveInput = new float2(moveInput.x, moveInput.z);
        playerInputs.CameraLookInput = viewInput;

        if (math.length(viewInput ) < 0.15f && targetSpotted)
        {
            playerInputs.FirePressed.Set(fixedTick);
        }
        brain.TargetFoundElapsed += DeltaTime;


    }
}

//public partial struct BrainCleanJob : IJobEntity
//{
//    public EntityCommandBuffer ecb;

//    void Execute(Entity entity, in Brain brain)
//    {

//        //ecb.RemoveComponent<HeardSound>(entity);

//    }
//}

/// <summary>
/// Apply inputs that need to be read at a variable rate
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct ThirdPersonAIVariableStepControlSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<ThirdPersonPlayer, ThirdPersonPlayerInputs, Brain>().Build());
    }

    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerInputs, player, aiTag) in SystemAPI.Query<ThirdPersonPlayerInputs, ThirdPersonPlayer, Brain>().WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<OrbitCameraControl>(player.ControlledCamera))
            {
                OrbitCameraControl cameraControl = SystemAPI.GetComponent<OrbitCameraControl>(player.ControlledCamera);
                
                cameraControl.FollowedCharacterEntity = player.ControlledCharacter;
                cameraControl.Look = playerInputs.CameraLookInput;
                //cameraControl.Zoom = playerInputs.CameraZoomInput;

                SystemAPI.SetComponent(player.ControlledCamera, cameraControl);
            }
        }
    }

    
}

///// <summary>
///// Apply inputs that need to be read at a fixed rate.
///// It is necessary to handle this as part of the fixed step group, in case your framerate is lower than the fixed step rate.
///// </summary>
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
//[BurstCompile]
//public partial struct ThirdPersonAIFixedStepControlSystem : ISystem
//{
//    [BurstCompile]
//    public void OnCreate(ref SystemState state)
//    {
//        state.RequireForUpdate<FixedTickSystem.Singleton>();
//        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<ThirdPersonPlayer, ThirdPersonPlayerInputs, AITag>().Build());
//    }

//    public void OnDestroy(ref SystemState state)
//    { }

//    [BurstCompile]
//    public void OnUpdate(ref SystemState state)
//    {
//        uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

//        foreach (var (playerInputs, player, tag) in SystemAPI.Query<RefRW<ThirdPersonPlayerInputs>, ThirdPersonPlayer, AITag>().WithAll<Simulate>())
//        {
//            if (SystemAPI.HasComponent<ThirdPersonCharacterControl>(player.ControlledCharacter))
//            {
//                ThirdPersonCharacterControl characterControl = SystemAPI.GetComponent<ThirdPersonCharacterControl>(player.ControlledCharacter);

//                float3 characterUp = MathUtilities.GetUpFromRotation(SystemAPI.GetComponent<LocalTransform>(player.ControlledCharacter).Rotation);
                
//                // Get camera rotation data, since our movement is relative to it
//                quaternion cameraRotation = quaternion.identity;
//                if (SystemAPI.HasComponent<LocalTransform>(player.ControlledCamera))
//                {
//                    cameraRotation = SystemAPI.GetComponent<LocalTransform>(player.ControlledCamera).Rotation;
//                }
//                float3 cameraForwardOnUpPlane = math.normalizesafe(MathUtilities.ProjectOnPlane(MathUtilities.GetForwardFromRotation(cameraRotation), characterUp));
//                float3 cameraRight = MathUtilities.GetRightFromRotation(cameraRotation);

//                characterControl.CameraForwardVector = cameraForwardOnUpPlane;

//                // Move
//                characterControl.MoveVector = (playerInputs.ValueRW.MoveInput.y * cameraForwardOnUpPlane) + (playerInputs.ValueRW.MoveInput.x * cameraRight);
//                characterControl.MoveVector = MathUtilities.ClampToMaxLength(characterControl.MoveVector, 1f);

//                // Jump
//                // We use the "FixedInputEvent" helper struct here to detect if the event needs to be processed.
//                // This is part of a strategy for proper handling of button press events that are consumed during the fixed update group.
//                //characterControl.Jump = playerInputs.ValueRW.JumpPressed.IsSet(fixedTick);
//                characterControl.LastJumpPressedTime++;
//                if (playerInputs.ValueRW.JumpPressed.IsSet(fixedTick))//characterControl.Jump)
//                {
//                    characterControl.Jump = true;
//                    characterControl.LastJumpPressedTime = 0;
//                }
//                if(characterControl.LastJumpPressedTime > 10)
//                {
//                    characterControl.Jump = false;
//                }
//                    characterControl.Floating = playerInputs.ValueRW.FloatingInput;

//                bool trigged = playerInputs.ValueRW.FirePressed.IsSet(fixedTick);
//                characterControl.Fire = trigged;
//                //if (hasWeapon)
//                //{
//                //    Weapon weapon = SystemAPI.GetComponent<Weapon>(player.ControlledWeapon);
                 
//                //    weapon.IsFired = trigged;
//                //    //Debug.Log(trigged);
//                //    SystemAPI.SetComponent(player.ControlledWeapon, weapon);
//                //}

//                bool grabbed = playerInputs.ValueRW.InteractPressed.IsSet(fixedTick);
//                characterControl.Interact = grabbed;

//                SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
                
//            }
//        }

       
//    }
//}