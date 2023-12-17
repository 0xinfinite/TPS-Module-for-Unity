using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

//public enum WeaponType { Hitscan = 0, ProjectileSpawner}
public enum HitscanType { SingleRay, SphereCast, PenetrateSphereCast}

[Serializable]
public struct Weapon : IComponentData
{
    //public WeaponType WeaponType;
    //public float hitscanRange;
    //public HitscanType HitscanType;
    //public float SphereRadius;
    //public Entity Projectile;
    public Bullet bullet;
    public Entity AimingSightsEntity;
    public Entity HandEntity;
    //public float RigidbodyPushForce;
    public int BulletCount;
    public float BulletSpread;

    //[HideInInspector]
    //public bool IsFired;
    //[HideInInspector]
    //public float3 MuzzlePosition;
    //[HideInInspector]
    //public float3 MuzzleForward;

    public static Weapon GetDefault()
    {
        Weapon w = new Weapon()
        {
            //WeaponType = WeaponType.Hitscan,
            //hitscanRange = 100f,
            //HitscanType = HitscanType.SingleRay,
            //SphereRadius = 0.01f,
            //RigidbodyPushForce = 100f,
            BulletCount = 1,
            BulletSpread = 0,
            //IsFired = false,
        };

        return w;
    }
}