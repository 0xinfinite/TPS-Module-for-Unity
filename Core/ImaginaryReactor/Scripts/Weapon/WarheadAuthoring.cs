using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ImaginaryReactor
{
    [DisallowMultipleComponent]
    public class WarheadAuthoring : MonoBehaviour
    {
        public GameObject fragment;
        public GameObject impactParticle;
        public BulletType damageType;
        public HitscanType hitscanType;
        public float hitscanRange;
        public float sphereRadius;
        public float rigidbodyPushForce = 100f;
        public int fragmentCount = 1;
        public float fragmentSpread = 0;
        public float fragmentDamage = 0;

        public class Baker : Baker<WarheadAuthoring>
        {
            public override void Bake(WarheadAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

                AddComponent(entity, //authoring.HeadCam);
                    new Warhead()
                    {

                        DamageType = authoring.damageType,
                        Fragment = GetEntity(authoring.fragment, TransformUsageFlags.Dynamic),
                        ImpactParticle = GetEntity(authoring.impactParticle, TransformUsageFlags.Dynamic),
                        FragmentCount = authoring.fragmentCount,
                        FragmentForce = authoring.rigidbodyPushForce,
                        FragmentSpread = authoring.fragmentSpread,
                        FragmentDamage = authoring.fragmentDamage,
                        //FragmentCount

                    });
            }
        }
    }
}