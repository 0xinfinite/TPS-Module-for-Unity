using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

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
    public float3 LaserPointerPosition;
    //public float AimingSightsHeight;

    //[HideInInspector]
    //public float3 PlanarForward;

    public static AimingSights GetDefault()
    {
        AimingSights c = new AimingSights
        {
            
        };
        return c;
    }
}

public struct PlayerLaserPointer : IComponentData
{
}

//[Serializable]
//public struct AimingSightsControl : IComponentData
//{
//    public Entity FollowedCharacterEntity;
//    public Entity FollowedCameraEntity;
//}
