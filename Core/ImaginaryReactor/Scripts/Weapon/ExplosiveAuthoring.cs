
using Unity.Entities;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Authoring;

namespace ImaginaryReactor
{
    public class ExplosiveAuthoring : MonoBehaviour
    {
        public BulletType DamageType;
        public GameObject FragmentPrefab;
        public GameObject ExplosionParticle;
        public float EffectRange;
        public float Damage;
        public int FragmentCount;
        public float FragmentSpread;
        public float FragmentForce;
        public float DetonatorDelay;
        public bool DetonateWhenContact;
        public PhysicsCategoryTags EffectFilter;
        public PhysicsCategoryTags RayCheckFilter;

        public class Baker : Baker<ExplosiveAuthoring>
        {
            public override void Bake(ExplosiveAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                //if (authoring.DetonatorDelay > 0)
                //{
                //    AddComponent(entity, new DelayedWarhead()
                //    {
                //        DamageType = authoring.DamageType,
                //        Fragment = GetEntity(authoring.Fragment, TransformUsageFlags.Dynamic),
                //        ImpactParticle = GetEntity(authoring.ImpactParticle, TransformUsageFlags.Dynamic),
                //        EffectRange = authoring.EffectRange,
                //        Damage = authoring.Damage,
                //        FragmentCount = authoring.FragmentCount,
                //        FragmentSpread = authoring.FragmentSpread,
                //        FragmentForce = authoring.FragmentForce,
                //        RemainTime = authoring.DetonatorDelay,
                //    });
                //}
                //else
                //{
                AddComponent(entity, new Warhead()
                {
                    //DamageType = authoring.DamageType,
                    //Fragment = GetEntity(authoring.Fragment, TransformUsageFlags.Dynamic),
                    //ImpactParticle = GetEntity(authoring.ImpactParticle, TransformUsageFlags.Dynamic),
                    //EffectRange = authoring.EffectRange,
                    //Damage = authoring.Damage,
                    Fragment = new Bullet()
                    {
                        BulletType = authoring.DamageType,
                        HitscanRange = authoring.EffectRange,
                        SphereRadius = authoring.EffectRange,
                        RigidbodyPushForce = authoring.FragmentForce,
                        HitscanFilter = new CollisionFilter() { BelongsTo = authoring.RayCheckFilter.Value, CollidesWith = authoring.RayCheckFilter.Value },
                        ShapeFilter = new CollisionFilter() { BelongsTo = authoring.EffectFilter.Value , CollidesWith = authoring.EffectFilter.Value },
                        EnergyAmount = authoring.Damage,
                        HitscanType = HitscanType.SingleRay,
                     ProjectileEntity = GetEntity(authoring.FragmentPrefab, TransformUsageFlags.Dynamic),
                     ImpactParticleEntity = GetEntity(authoring.ExplosionParticle, TransformUsageFlags.Dynamic),
                    },
                    FragmentCount = authoring.FragmentCount,
                    FragmentSpread = authoring.FragmentSpread,
                    DetonateWhenContact = authoring.DetonateWhenContact
                    //FragmentForce = authoring.FragmentForce,
                });
                if(authoring.DetonatorDelay > 0)
                {
                    AddComponent(entity, new Dissolvable() { RemainTime = authoring.DetonatorDelay });
                }
                //}
                //AddComponent(entity, new Detonator()
                //{
                //    Delay = authoring.DetonatorDelay,
                //    DetonateWhenContact = authoring.DetonateWhenContact
                //});
            }
        }
    }
}
