using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace ImaginaryReactor
{
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
        public FixedInputEvent SwitchViewPressed;
        public FixedInputEvent FirePressed;
        public FixedInputEvent InteractPressed;
        public FixedInputEvent ThrowPress;
        public FixedInputEvent ThrowRelease;
    }

    public struct PlayerTag : IComponentData
    {
    }
}