using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;


public struct Hitbox : IComponentData
{
    public Entity Owner;
    public float3 Center;
    public bool IsCritical;
    public float DamageMultiply;
    public ColliderKey IFF_Key;
    //public float BoundSize;
}

public struct SeperatedChild : IComponentData
{
    public Entity Parent;
    public float3 LocalPosition;
    public float3 LocalForward;
    public float3 LocalUp;
    public quaternion LocalRotation;
}