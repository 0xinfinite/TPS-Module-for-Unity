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