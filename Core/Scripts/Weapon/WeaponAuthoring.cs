using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;
using Unity.Physics.Authoring;

namespace ImaginaryReactor
{
    //[UpdateAfter(typeof(AimingSightsAuthoring))]
    [DisallowMultipleComponent]
    public class WeaponAuthoring : MonoBehaviour
    {
        //public GameObject sights;
        public GameObject projectile;
        public float projectileSpeed = 100f;
        public GameObject impactParticle;
        public GameObject muzzleFlash;
        public GameObject playerMuzzleFlash;
        public BulletType bulletType;
        public HitscanType hitscanType;
        public float hitscanRange;
        //public LayerMask hitscanMask = 1;
        public PhysicsMaterialProperties physicsMaterial;
        public CollisionFilter hitscanFilter;
        public float sphereRadius;
        public float energyAmount = 100f;
        public float rigidbodyPushForce = 10f;
        public float loudSoundRange = 200f;
        public int bulletCount = 1;
        public float bulletSpread = 0;
        //public GameObject[] ignoreHitboxes;

        public class Baker : Baker<WeaponAuthoring>
        {
            public override void Bake(WeaponAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

                AddComponent(entity, //authoring.HeadCam);
                    new Weapon()
                    {
                        //WeaponType = authoring.weaponType,
                        //HitscanType = authoring.hitscanType,
                        //SphereRadius = authoring.sphereRadius,
                        ////AimingSightsEntity = GetEntity( authoring.sights, TransformUsageFlags.Dynamic),
                        //RigidbodyPushForce = authoring.rigidbodyPushForce,
                        //AimingSights = new AimingSights()
                        //{
                        //    MuzzleEntity = GetEntity( authoring.sights.muzzle, TransformUsageFlags.Dynamic),
                        //    FollowedCameraEntity = GetEntity(authoring.sights.targetOrbitCamera, TransformUsageFlags.Dynamic)
                        //},
                        //AimingSightsEntity = GetEntity(authoring.sights, TransformUsageFlags.Dynamic),
                        bullet = new Bullet()
                        {
                            BulletType = authoring.bulletType,
                            HitscanType = authoring.hitscanType,
                            HitscanFilter = new CollisionFilter() { BelongsTo = authoring.physicsMaterial.BelongsTo.Value, CollidesWith = authoring.physicsMaterial.CollidesWith.Value },//FilterConversion.LayerMaskToFilter(authoring.hitscanMask),// new CollisionFilter() { BelongsTo = (uint)authoring.hitscanMask.value, CollidesWith = (uint)authoring.hitscanMask.value },
                            HitscanRange = authoring.hitscanRange,
                            SphereRadius = authoring.sphereRadius,
                            ProjectileEntity = authoring.bulletType == BulletType.Projectile && authoring.projectile != null ?
                            GetEntity(authoring.projectile, TransformUsageFlags.Dynamic) : Entity.Null,
                            ProjectileSpeed = authoring.projectileSpeed,
                            ImpactParticleEntity = authoring.impactParticle != null ? GetEntity(authoring.impactParticle, TransformUsageFlags.Dynamic) : Entity.Null,
                            MuzzleFlashEntity = GetEntity(authoring.muzzleFlash, TransformUsageFlags.Dynamic),
                            MyMuzzleFlashEntity = GetEntity(authoring.playerMuzzleFlash, TransformUsageFlags.Dynamic),
                            EnergyAmount = authoring.energyAmount,
                            LoudSoundRange = authoring.loudSoundRange,
                            RigidbodyPushForce = authoring.rigidbodyPushForce,
                            MagicIFF_Key = authoring.physicsMaterial.CustomTags.Value,
                        },
                        BulletCount = authoring.bulletCount,
                        BulletSpread = authoring.bulletSpread,
                    });
            }
        }
    }
}