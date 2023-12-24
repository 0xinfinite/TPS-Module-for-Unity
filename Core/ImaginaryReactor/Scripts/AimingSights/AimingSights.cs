using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace ImaginaryReactor
{
    [Serializable]
    public struct AimingSights : IComponentData
    {
        public Entity MuzzleEntity;
        public Entity FollowedCameraEntity;
        public Entity LaserPointEntity;
        //public Entity WeaponEntity;
        public CollisionFilter RayFilter;
        public CollisionFilter TrackingRayFilter;
        public CollisionFilter ObstacleFilter;

        [HideInInspector]
        public float3 MuzzlePosition;
        [HideInInspector]
        public float3 MuzzleForward;
        [HideInInspector]
        public quaternion FirstPersonZoomOffset;
        [HideInInspector]
        public quaternion DefaultShoulderViewOffset;
        [HideInInspector]
        public quaternion AlternativeShoulderViewOffset;
        //[HideInInspector]
        //public quaternion TargetSwitchShoulderViewOffset;
        //[HideInInspector]
        //public quaternion CurrentSwitchShoulderViewOffset;
        [HideInInspector]
        public float3 LaserPointerPosition;
        //[HideInInspector]
        public float3 CameraMovementDirection;
        public float2 TrackingAngle;
        public float TrackingOffset;
        public float3 TargetVector;
        public float3 TargetLocalVector;
        public float2 CachedLookInput;
        public float CurrentlyTracking;
        //[HideInInspector]
        //public quaternion RightShoulderViewOffset;
        //[HideInInspector]
        //public quaternion LeftShoulderViewOffset;

        //public float AimingSightsHeight;

        //[HideInInspector]
        //public float3 PlanarForward;

        public static AimingSights GetDefault()
        {
            AimingSights c = new AimingSights
            {
                //TargetSwitchShoulderViewOffset = quaternion.identity,
                //CurrentSwitchShoulderViewOffset = quaternion.identity
                TrackingOffset = 1
            };
            return c;
        }
    }

    public struct PlayerLaserPointer : IComponentData
    {
        public Entity Owner;
    }

    //[Serializable]
    //public struct AimingSightsControl : IComponentData
    //{
    //    public Entity FollowedCharacterEntity;
    //    public Entity FollowedCameraEntity;
    //}
}