using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace ImaginaryReactor
{
    [Serializable]
    public struct Warhead : IComponentData
    {
        //public BulletType DamageType;
        public Bullet Fragment;
        public Entity ImpactParticle;
        //public float EffectRange;
        //public float Damage;
        public int FragmentCount;
        public float FragmentSpread;
        //public float FragmentForce;
        //public ColliderKey MagicIFF_Key;


        //public static Warhead GetDefault()
        //{
        //    Warhead w = new Warhead()
        //    {
        //        FragmentCount = 1,
        //        FragmentSpread = 0,
        //        //Damage = 100f,
        //        //FragmentForce = 100f,
        //        //EffectRange = 10f
        //    };

        //    return w;
        //}
    }

    //public struct DelayedWarhead : IComponentData
    //{
    //    public BulletType DamageType;
    //    public Bullet Fragment;
    //    public Entity ImpactParticle;
    //    //public float EffectRange;
    //    //public float Damage;
    //    public int FragmentCount;
    //    public float FragmentSpread;
    //    //public float FragmentForce;
    //    public float RemainTime;
    //    public Entity LinkedCharacter;
    //    //public ColliderKey MagicIFF_Key;


    //    //public static DelayedWarhead GetDefault()
    //    //{
    //    //    DelayedWarhead w = new DelayedWarhead()
    //    //    {
    //    //        FragmentCount = 1,
    //    //        FragmentSpread = 0,
    //    //        //Damage = 100f,
    //    //        //FragmentForce = 100f,
    //    //        //EffectRange = 10f,
    //    //        RemainTime = 3
    //    //    };

    //    //    return w;
    //    //}
    //}

    public struct TriggedWarheadData : IComponentData
    {
        public float3 FiredPosition;
        public float3 WarheadForward;
        public float WarheadForceAmount;

    }
}