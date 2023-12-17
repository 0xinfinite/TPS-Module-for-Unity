using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.CharacterController;

[Serializable]
public struct ThirdPersonCharacterComponent : IComponentData
{
    public bool AlwaysLookForwardOfCamera;
    public float RotationSharpness;
    public float GroundMaxSpeed;
    public float GroundedMovementSharpness;
    public float AirAcceleration;
    public float AirMaxSpeed;
    public float AirDrag;
    public float AirBreak;
    public float JumpSpeed;
    public float WallJumpSpeed;
    public bool IgnoreInertiaWhenJump;
    public int AirJump;
    public float AirVerticalAcceleration;
    public float AirVerticalAccelerationWhenDescend;
    public float MaxClimbFuel;
    public float MaxClimbSpeed;
    public bool AbleToWallRun;
    public float WallRunSlope;
    public float WallRunMaxSpeed;
    //public float WallRunAcceleration;
    public float WallRunMovementSharpness;
    public float MaxWallSlideSpeed;
    public float GravityMultiplierWhenWallSlide;
    public float WallSlideDuration;
    public float3 Gravity;
    public bool PreventAirAccelerationAgainstUngroundedHits;
    public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling;


    public float GroundedTime;
    public float3 WallNormal;
    public float3 LastWallNormal;
    public float WallSlideTime;
    public int ContactCount;
    public int AirJumpedCount;

    public static ThirdPersonCharacterComponent GetDefault()
    {
        return new ThirdPersonCharacterComponent
        {
            AlwaysLookForwardOfCamera = false,
            RotationSharpness = 25f,
            GroundMaxSpeed = 10f,
            GroundedMovementSharpness = 15f,
            AirAcceleration = 50f,
            AirMaxSpeed = 10f,
            AirDrag = 0f,
            AirBreak = 1f,
            JumpSpeed = 10f,
            WallJumpSpeed = 3f,
            IgnoreInertiaWhenJump = false,
            AirJump = 0,
            AirVerticalAcceleration = 0f,
            AirVerticalAccelerationWhenDescend = 0f,
            MaxClimbFuel = 0f,
            MaxClimbSpeed = 0,
            AbleToWallRun = false,
            WallRunSlope = 0.3f,
            //WallRunAcceleration = 0f,
            WallRunMovementSharpness = 10f,
            WallRunMaxSpeed = 0f,
            GravityMultiplierWhenWallSlide = 1f,
            //MaxWallSlideSpeed = 0f,
            Gravity = math.up() * -30f,
            PreventAirAccelerationAgainstUngroundedHits = true,
            StepAndSlopeHandling = BasicStepAndSlopeHandlingParameters.GetDefault(),
        };
    }
}

[Serializable]
public struct ThirdPersonCharacterControl : IComponentData
{
    public float3 MoveVector;
    public bool Jump;
    public uint LastJumpPressedTime;
    public float Floating;
    public float FloatingFuel;
    public float3 CameraForwardVector;
    public bool Fire;
    public bool Interact;
}
