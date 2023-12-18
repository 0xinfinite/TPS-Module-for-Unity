using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;


[Serializable]
public struct Seeker : IComponentData
{
    public Entity BrainEntity;
    public Entity TargetEntity;
    public float3 LastKnownVector;
    public bool GetDirection;
    //public float Range;
    //public float FieldOfView;
    //public CollisionFilter ShapeFilter;
    public bool CheckRaycast;
    //public CollisionFilter ColliderFilter;
    public CollisionFilter RaycastFilter;
    public ColliderKey TargetSideKey;
    public ColliderKey SideKey;
    public float3 SeekerOffset;
}

[InternalBufferCapacity(128)]
public struct IgnoreHitboxData : IBufferElementData
{
    public Entity hitboxEntity;
}




