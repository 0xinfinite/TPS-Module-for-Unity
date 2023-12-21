using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Authoring;

namespace ImaginaryReactor
{
    [DisallowMultipleComponent]
    public class WarheadAuthoring : MonoBehaviour
    {
        public GameObject fragmentPrefab;
        public GameObject impactParticle;
        public BulletType damageType;
        public HitscanType hitscanType;
        public float hitscanRange = 10f;
        public float sphereRadius = 10f;
        public float rigidbodyPushForce = 100f;
        public int fragmentCount = 1;
        public float fragmentSpread = 0;
        public float fragmentDamage = 0;
        public float projectileSpeed = 100f;
        public float loudSoundRange = 100f;
        public PhysicsMaterialProperties physicsMaterial;
        public CollisionFilter hitscanFilter;
        public bool detonateWhenContact = true;
        public float criticalMultiply = 2;

        public class Baker : Baker<WarheadAuthoring>
        {
            public override void Bake(WarheadAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

                AddComponent(entity, //authoring.HeadCam);
                    new Warhead()
                    {
                        //Fragment = GetEntity(authoring.fragment, TransformUsageFlags.Dynamic),
                        Fragment = new Bullet()
                        {
                            BulletType = authoring.damageType,
                             ProjectileEntity = authoring.damageType == BulletType.Projectile && authoring.fragmentPrefab!= null ?
                            GetEntity(authoring.fragmentPrefab, TransformUsageFlags.Dynamic) : Entity.Null,
                            HitscanType = authoring.hitscanType,
                            HitscanFilter = new CollisionFilter() { BelongsTo = authoring.physicsMaterial.BelongsTo.Value,
                                CollidesWith = authoring.physicsMaterial.CollidesWith.Value },//FilterConversion.LayerMaskToFilter(authoring.hitscanMask),// new CollisionFilter() { BelongsTo = (uint)authoring.hitscanMask.value, CollidesWith = (uint)authoring.hitscanMask.value },
                            HitscanRange = authoring.hitscanRange,
                            SphereRadius = authoring.sphereRadius,
                            ProjectileSpeed = authoring.projectileSpeed,
                            ImpactParticleEntity = authoring.impactParticle != null ? GetEntity(authoring.impactParticle, TransformUsageFlags.Dynamic) : Entity.Null,
                            //MuzzleFlashEntity = GetEntity(authoring.muzzleFlash, TransformUsageFlags.Dynamic),
                            //MyMuzzleFlashEntity = GetEntity(authoring.playerMuzzleFlash, TransformUsageFlags.Dynamic),
                            EnergyAmount = authoring.fragmentDamage,
                            LoudSoundRange = authoring.loudSoundRange,
                            RigidbodyPushForce = authoring.rigidbodyPushForce,
                            MagicIFF_Key = authoring.physicsMaterial.CustomTags.Value,
                            CriticalMultiply = authoring.criticalMultiply
                        },
                        
                        ImpactParticle = GetEntity(authoring.impactParticle, TransformUsageFlags.Dynamic),
                        FragmentCount = authoring.fragmentCount,
                        FragmentSpread = authoring.fragmentSpread,
                        DetonateWhenContact = authoring.detonateWhenContact,
                        //FragmentCount

                    });
            }
        }
    }
}