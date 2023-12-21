using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
namespace ImaginaryReactor
{
    public enum BulletType { Hitscan = 0, Projectile, SphericalExplosive, ShapeExplosive }

    [Serializable]
    public struct Bullet : IComponentData
    {
        public BulletType BulletType;
        public HitscanType HitscanType;
        public CollisionFilter HitscanFilter;
        public float HitscanRange;
        public float SphereRadius;
        public CollisionFilter ShapeFilter;
        public Entity ProjectileEntity;
        public float ProjectileSpeed;
        public Entity ImpactParticleEntity;
        public Entity MuzzleFlashEntity;
        public Entity MyMuzzleFlashEntity;
        public float EnergyAmount;
        public float CriticalMultiply;
        public float RigidbodyPushForce;
        public ColliderKey MagicIFF_Key;
        public float LoudSoundRange;

        public static Bullet GetDefault()
        {
            Bullet b = new Bullet()
            {
                BulletType = BulletType.Hitscan,
                HitscanType = HitscanType.SingleRay,
                HitscanFilter = CollisionFilter.Default,
                ShapeFilter = CollisionFilter.Default,
                ProjectileSpeed = 100f,
                CriticalMultiply = 1,
                HitscanRange = 100f,
                SphereRadius = 0.01f,
                EnergyAmount = 100f,
                RigidbodyPushForce = 10f,
                LoudSoundRange = 200f,
            };

            return b;
        }
    }
}