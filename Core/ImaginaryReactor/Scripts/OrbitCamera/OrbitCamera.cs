﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ImaginaryReactor
{

    [Serializable]
    public struct OrbitCamera : IComponentData
    {
        [Header("Rotation")]
        public float RotationSpeed;
        public float MaxVAngle;
        public float MinVAngle;
        public bool RotateWithCharacterParent;

        [Header("Zooming")]
        public float TargetDistance;
        public float MinDistance;
        public float MaxDistance;
        public float DistanceMovementSpeed;
        public float DistanceMovementSharpness;
        public float2 PositionOffset;

        [Header("Obstructions")]
        public float ObstructionRadius;
        public float ObstructionInnerSmoothingSharpness;
        public float ObstructionOuterSmoothingSharpness;
        public bool PreventFixedUpdateJitter;

        // Data in calculations
        [HideInInspector]
        public float CurrentDistanceFromMovement;
        [HideInInspector]
        public float CurrentDistanceFromObstruction;
        [HideInInspector]
        public float PitchAngle;
        [HideInInspector]
        public float3 PlanarForward;
        [HideInInspector]
        public float3 PlanarFirstPersonForward;
        [HideInInspector]
        public quaternion ThirdPersonRotation;
        [HideInInspector]
        public float viewDirectionRate;

        public static OrbitCamera GetDefault()
        {
            OrbitCamera c = new OrbitCamera
            {
                RotationSpeed = 150f,
                MaxVAngle = 89f,
                MinVAngle = -89f,

                TargetDistance = 5f,
                MinDistance = 0f,
                MaxDistance = 10f,
                DistanceMovementSpeed = 50f,
                DistanceMovementSharpness = 20f,
                PositionOffset = float2.zero,

                ObstructionRadius = 0.1f,
                ObstructionInnerSmoothingSharpness = float.MaxValue,
                ObstructionOuterSmoothingSharpness = 5f,
                PreventFixedUpdateJitter = true,

                CurrentDistanceFromObstruction = 0f,
                ThirdPersonRotation = Quaternion.identity,
                viewDirectionRate = 1f
            };
            return c;
        }
    }

    [Serializable]
    public struct OrbitCameraControl : IComponentData
    {
        public Entity FollowedCharacterEntity;
        public float2 Look;
        public bool IsMouseInput;
        public float Zoom;
        public bool ToggleZoom;
        public bool SwitchView;
    }

    [Serializable]
    public struct OrbitCameraIgnoredEntityBufferElement : IBufferElementData
    {
        public Entity Entity;
    }
}