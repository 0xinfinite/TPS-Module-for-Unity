//using System.Collections.Generic;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Physics;
//using Unity.Transforms;
//using UnityEngine;

//[DisallowMultipleComponent]
//public class AimingSightsAuthoring : MonoBehaviour
//{
//    public GameObject muzzle;
//    public GameObject targetOrbitCamera;
//    //public GameObject weapon;
//    public LayerMask layerMask;
//    //public float AimingSightsHeight;

//    public AimingSights AimingSights = AimingSights.GetDefault();

//    public class Baker : Baker<AimingSightsAuthoring>
//    {
//        public override void Bake(AimingSightsAuthoring authoring)
//        {
//            //authoring.AimingSights.PlanarForward = -math.forward();

//            Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

//            AddComponent(entity, //authoring.AimingSights);
//                new AimingSights()
//                {
//                    //PlanarForward = -math.forward(),
//                    MuzzleEntity = GetEntity(authoring.muzzle, TransformUsageFlags.Dynamic),
//                    FollowedCameraEntity = GetEntity(authoring.targetOrbitCamera, TransformUsageFlags.Dynamic),
//                    //WeaponEntity = GetEntity(authoring.weapon, TransformUsageFlags.Dynamic),
//                    RayFilter = new CollisionFilter() { BelongsTo = 1u, CollidesWith = (uint)authoring.layerMask.value } 
//                    //AimingSightsHeight = authoring.AimingSightsHeight
//                });
//            //AddComponent(entity, new AimingSightsControl());
            
//        }
//    }
//}