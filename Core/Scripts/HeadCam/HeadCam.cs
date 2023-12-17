using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct HeadCam : IComponentData
{
    public Entity FollowedCharacterEntity;
    public Entity FollowedCameraEntity;
    public float3 HeadCamOffset;
    public int ID;

    [HideInInspector]
    public float3 PlanarForward;

    public static HeadCam GetDefault()
    {
        HeadCam c = new HeadCam
        {
            
        };
        return c;
    }
}

//[Serializable]
//public struct HeadCamControl : IComponentData
//{
//    public Entity FollowedCharacterEntity;
//    public Entity FollowedCameraEntity;
//}
