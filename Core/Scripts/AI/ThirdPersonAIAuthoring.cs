using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace ImaginaryReactor
{
    public class ThirdPersonAIAuthoring : MonoBehaviour
    {
        public GameObject ControlledCharacter;
        public GameObject ControlledCamera;
        public Transform RallyPoint;
        public Transform OverwatchPoint;
        //public GameObject CameraTarget;
        public Vector3 AimOffset;
        [Min(0.01f)]
        public float AimReflex = 1;
        public float AimPredictMultiplier = 1;
        [Min(0)]
        public float ReflexDelay = 0.5f;
        public float DistanceThresholdForPredict = 5;
        //public PhysicsMaterialProperties brainFilterAuthoring;
        //public PhysicsCategoryTags BelongsTo;
        //public PhysicsCategoryTags CollidesWith;
        public CustomPhysicsMaterialTags IFF_Key;

        //public GameObject ControlledWeapon;
        //public bool DestroyWeapon;

        public class Baker : Baker<ThirdPersonAIAuthoring>
        {
            public override void Bake(ThirdPersonAIAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new ThirdPersonPlayer
                {
                    ControlledCharacter = GetEntity(authoring.ControlledCharacter, TransformUsageFlags.Dynamic),
                    ControlledCamera = GetEntity(authoring.ControlledCamera, TransformUsageFlags.Dynamic),
                    //InitWeapon = GetEntity(authoring.ControlledWeapon, TransformUsageFlags.Dynamic),
                });
                AddComponent(entity, new ThirdPersonPlayerInputs());
                AddComponent(entity, new Brain()
                {
                    //TargetEntity = GetEntity( authoring.Target, TransformUsageFlags.Dynamic)
                    //    DesirePositionToMove = authoring.Target.transform.position, 
                    //    DesirePositionToLook = authoring.CameraTarget.transform.position,
                    Owner = entity,
                    TargetFoundElapsed = 9999,
                    //TargetFound = false,
                    DesirePositionToMove = authoring.RallyPoint ? authoring.RallyPoint.position : authoring.transform.position,
                    FinalDesirePositionToLook = authoring.OverwatchPoint ? authoring.OverwatchPoint.position :
                (authoring.RallyPoint ? authoring.transform.position + (authoring.RallyPoint.position - authoring.transform.position).normalized * 10f
                : authoring.transform.forward * 10f),
                    AimOffset = authoring.AimOffset,
                    AimReflex = authoring.AimReflex,
                    AimPredictMultiplier = authoring.AimPredictMultiplier,
                    ReflexDelay = authoring.ReflexDelay,
                    DistanceThresholdForPredict = authoring.DistanceThresholdForPredict,
                    //Filter = new CollisionFilter() { BelongsTo = authoring.BelongsTo.Value, CollidesWith = authoring.CollidesWith.Value },
                    ReceivedSignalInfo = new float4(0, 0, 0, -1),
                    IFF_Key = authoring.IFF_Key.Value,
                });
                //AddComponent(entity, new HeardSound() { Recognized = false , SoundSource = new float3(-100,-100,-100)});


            }
        }
    }


    public struct Brain : IComponentData
    {
        //public Entity TargetEntity;
        public Entity Owner;
        public float TargetFoundElapsed;
        public float AimReflex;
        public float AimPredictMultiplier;
        public float DistanceThresholdForPredict;
        public float ReflexDelay;
        public float4 ReceivedSignalInfo;
        public float3 PrevDesirePositionToLook;
        public float3 FinalDesirePositionToLook;
        public float3 TargetRealPosition;
        public float3 TargetRealVelocity;
        public float3 DesirePositionToMove;
        public float3 AimOffset;
        //public CollisionFilter Filter;
        public ColliderKey IFF_Key;
    }

    public struct TrackInfo : IComponentData
    {
        public Entity TargetEntity;
        public float3 LastKnownVector;
        public float3 TargetVelocity;
        public float DamageAmount;
        public bool IsDirection;
        public ColliderKey TargetKey;
    }
    public struct TrackingTarget : IComponentData
    {
        public Entity TargetEntity;
        public float RemainLostTime;
    }
}