using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using Unity.CharacterController;
using Unity.Physics;
using UnityEngine.Serialization;

namespace ImaginaryReactor
{
    [DisallowMultipleComponent]
    public class ThirdPersonCharacterAuthoring : MonoBehaviour
    {
        public AuthoringKinematicCharacterProperties CharacterProperties = AuthoringKinematicCharacterProperties.GetDefault();
        public ThirdPersonCharacterComponent Character = ThirdPersonCharacterComponent.GetDefault();

        public GameObject muzzle;
        public GameObject laserPoint;
        public GameObject targetOrbitCamera;
        public Transform ClimbHandStart;
        public Transform ClimbHandEnd;
        public Transform VaultHandStart;
        public Transform VaultHandEnd;
        public PhysicsCategoryTags VaultFilter;
        public float VaultDuration = 0;
        public Vector3 VaultOffset = Vector3.zero;
        public float ClimbMaxHeight = 0.2f;

        //public LayerMask layerMask;
        public PhysicsCategoryTags AimRayFilter;
        //public GameObject[] IgnoreHitboxList;

        public class Baker : Baker<ThirdPersonCharacterAuthoring>
        {
            public override void Bake(ThirdPersonCharacterAuthoring authoring)
            {
                KinematicCharacterUtilities.BakeCharacter(this, authoring, authoring.CharacterProperties);

                Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
                AddComponent(entity, new AimingSights()
                {
                    MuzzleEntity = GetEntity(authoring.muzzle, TransformUsageFlags.Dynamic),
                    LaserPointEntity = GetEntity(authoring.laserPoint, TransformUsageFlags.Dynamic),
                    FollowedCameraEntity = GetEntity(authoring.targetOrbitCamera, TransformUsageFlags.Dynamic),
                    RayFilter = new CollisionFilter() { BelongsTo = authoring.AimRayFilter.Value, CollidesWith = authoring.AimRayFilter.Value } //FilterConversion.LayerMaskToFilter(authoring.layerMask) //new CollisionFilter() { BelongsTo = 1u, CollidesWith = (uint)authoring.layerMask.value }
                    
                });


                bool vault = (authoring.ClimbHandStart && authoring.ClimbHandEnd) || (authoring.VaultHandStart && authoring.VaultHandEnd);

                if (vault)
                {
                    AddComponent(entity, new VaultComponent()
                    {
                        VaultDuration = authoring.VaultDuration,
                        VaultOffset = authoring.VaultOffset,
                        //VaultRemainTime = authoring.VaultDuration,
                        Filter = new CollisionFilter() { BelongsTo = authoring.VaultFilter.Value, CollidesWith = authoring.VaultFilter.Value }
                    });
                    DynamicBuffer<VaultHand> vaultBuffer = AddBuffer<VaultHand>(entity);
                    if (authoring.ClimbHandStart && authoring.ClimbHandEnd)
                    {
                        vaultBuffer.Add(new VaultHand()
                        {
                            ValutRayStart = GetEntity(authoring.ClimbHandStart, TransformUsageFlags.Dynamic),
                            ValutRayEnd = GetEntity(authoring.ClimbHandEnd, TransformUsageFlags.Dynamic)
                        });
                    }
                    if (authoring.VaultHandStart && authoring.VaultHandEnd)
                    {
                        vaultBuffer.Add(new VaultHand()
                        {
                            ValutRayStart = GetEntity(authoring.VaultHandStart, TransformUsageFlags.Dynamic),
                            ValutRayEnd = GetEntity(authoring.VaultHandEnd, TransformUsageFlags.Dynamic)
                        });
                    }
                }

                AddComponent(entity, authoring.Character);
                AddComponent(entity, new ThirdPersonCharacterControl());
            }
        }

    }
}