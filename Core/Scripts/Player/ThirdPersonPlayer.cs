using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

[Serializable]
public struct ThirdPersonPlayer : IComponentData
{
    public Entity ControlledCharacter;
    public Entity ControlledCamera;
    //public Entity InitWeapon;
}

[Serializable]
public struct ThirdPersonPlayerInputs : IComponentData
{
    public float2 MoveInput;
    public float2 CameraLookInput;
    public float CameraZoomInput;
    public FixedInputEvent JumpPressed;
    public float FloatingInput;
    public FixedInputEvent FirePressed;
    public FixedInputEvent InteractPressed;
}

public struct PlayerTag : IComponentData
{
}
