using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;


[Serializable]
public struct Warhead : IComponentData
{
    public BulletType DamageType;
    public Entity Fragment;
    public Entity ImpactParticle;
    public int FragmentCount;
    public float FragmentSpread;
    public float FragmentRange;
    public float FragmentDamage;
    public float FragmentForce;
    //public ColliderKey MagicIFF_Key;


    public static Warhead GetDefault()
    {
        Warhead w = new Warhead()
        {
            FragmentCount = 1,
            FragmentSpread = 0,
            FragmentDamage = 100f,
            FragmentForce = 100f,
        };

        return w;
    }
}

public struct TriggedWarheadData : IComponentData
{
    public float3 FiredPosition;
    public float3 WarheadForward;
    public float WarheadForceAmount;
    
}
