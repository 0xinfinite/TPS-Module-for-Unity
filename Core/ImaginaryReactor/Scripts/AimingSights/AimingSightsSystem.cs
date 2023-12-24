using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.CharacterController;
using Unity.Entities.UniversalDelegates;
using System.Numerics;



namespace ImaginaryReactor
{

    //[UpdateInGroup(typeof(SimulationSystemGroup))]
    //[UpdateInGroup(typeof(SimulationSystemGroup))]
    //[UpdateAfter(typeof(PhysicsSimulationGroup))]
    //[UpdateBefore(typeof(PhysicsSimulationGroup))]
    public partial struct AimingSightsSystem : ISystem
    {

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            //state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<OrbitCamera, OrbitCameraControl>().Build());

            //UnityEngine.Debug.Log("Aiming Sights Created");
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach(var(
                //entity,
                //ref LocalTransform transform,
                AimingSights,playerTag,entity
                ) in SystemAPI.Query<RefRW<AimingSights>,PlayerTag>().WithEntityAccess())
            {
                var ignoredHitboxesBuffer = SystemAPI.GetBuffer<IgnoreHitboxData>(entity);
                bool tracking = false;
                var BodyLookup = SystemAPI.GetComponentLookup<KinematicCharacterBody>(false);

                var DeltaTime = SystemAPI.Time.DeltaTime;
                var PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
                var LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(false);
                var targetCameraEntityLocalToWorld = LocalToWorldLookup[AimingSights.ValueRW.FollowedCameraEntity];

                float3 rayStart = targetCameraEntityLocalToWorld.Position +
                    targetCameraEntityLocalToWorld.Forward * math.max(math.distance(targetCameraEntityLocalToWorld.Position, AimingSights.ValueRW.MuzzlePosition), 1);

                //if (isPlayer)// && rayHit)
                {
                    


                    //if (PhysicsWorld.CastRay(trackingRay, out hit))


                    if (BodyLookup.HasComponent(entity))
                    {
                        RaycastInput trackingRay = new RaycastInput()
                        {
                            Start = rayStart,
                            End = rayStart + targetCameraEntityLocalToWorld.Forward * 1000,
                            Filter = AimingSights.ValueRW.TrackingRayFilter
                        };

                        var myPosition = targetCameraEntityLocalToWorld.Position;

                        AimAssistHitsCollector collector = new AimAssistHitsCollector(entity, ignoredHitboxesBuffer, rayStart, AimingSights.ValueRW.CameraMovementDirection,
                            BodyLookup, PhysicsWorld, AimingSights.ValueRW.ObstacleFilter);

                        if (PhysicsWorld.CastRay(trackingRay, ref collector))
                        {
                            RaycastHit targetHit = collector.TargetHit;
                            KinematicCharacterBody myBody = BodyLookup[entity];
                            //KinematicCharacterBody targetBody = default;
                            Entity targetEntity = Entity.Null;
                            bool targetFound = false;
                            if (BodyLookup.HasComponent(targetHit.Entity))
                            {
                                targetFound = true;
                                targetEntity = targetHit.Entity;
                            }
                            //else if (HitboxLookup.HasComponent(targetHit.Entity))
                            //{
                            //    var targetHitbox = HitboxLookup[targetHit.Entity];
                            //    if (BodyLookup.HasComponent(targetHitbox.Owner))
                            //    {
                            //        targetFound = true;
                            //        targetEntity = targetHitbox.Owner;
                            //    }
                            //}


                            if (targetFound)
                            {
                                //AimingSights.ValueRW.TargetPosition = new float4(collector.TargetHit.Position , 1);

                                var myVelocity = myBody.RelativeVelocity;
                                var targetVelocity = BodyLookup[targetEntity].RelativeVelocity;


                                //var myPosition = targetCameraEntityLocalToWorld.Position;//
                                                                                         //LocalToWorldLookup[entity].Position;
                                var targetPosition = LocalToWorldLookup[targetEntity].Position;
                                //UnityEngine.Debug.Log(targetPosition);
                                var myEstimatePosition = (myPosition + myVelocity);
                                var toTargetVector = targetPosition - myPosition;

                                var myMat = new float4x4(quaternion.LookRotation(math.normalizesafe(toTargetVector),
                                    new float3(0, 1, 0)), myEstimatePosition);
                                ////var targetMat = new float4x4(quaternion.LookRotation(math.normalizesafe(toTargetVector),
                                ////    new float3(0, 1, 0)), targetPosition);



                                var lead =
                                    //myMat.InverseTransformRotation(quaternion.LookRotation( 
                                    //math.normalizesafe((targetPosition+targetVelocity)-myPosition), new float3(0,1,0)));
                                    myMat.InverseTransformRotation(quaternion.LookRotation(
                                    math.normalizesafe((targetPosition + targetVelocity) - myEstimatePosition), new float3(0, 1, 0)));
                                ////var negativeLead =
                                ////    targetMat.InverseTransformRotation(
                                ////        quaternion.LookRotation(
                                ////        math.normalizesafe((myPosition + myVelocity)- targetPosition), new float3(0,1,0))
                                ////        );
                                ////targetLTW.Value.InverseTransformRotation(quaternion.LookRotation(
                                ////math.normalizesafe((myPosition + myVelocity) - targetPosition), new float3(0,1,0))) ;

                                var leadingRotation = lead;//negativeLead;//
                                                           //           math.mul(lead, /*math.inverse*/(negativeLead));
                                var leadingForward = MathUtilities.GetForwardFromRotation(leadingRotation);//math.mul(leadingRotation, new float3(0, 0, 1));

                                var leadUp = leadingForward.y * math.PI * 0.5f;//math.dot(leadingForward, new float3(0, 1, 0));
                                var leadRight = leadingForward.x* math.PI * 0.5f;// math.dot(leadingForward, new float3(1, 0, 0));
                                //UnityEngine.Debug.Log("Lead Right : " + leadRight);
                                AimingSights.ValueRW.TargetVector = //new float3(
                                    targetPosition;//+ new float3(0, 0.75f, 0);
                                if(AimingSights.ValueRW.CurrentlyTracking < 0.5f)
                                {
                                    AimingSights.ValueRW.TargetLocalVector =
                                        targetCameraEntityLocalToWorld.Value.InverseTransformPoint(targetPosition);

                                   // UnityEngine.Debug.Log("Save Local Vector: " + AimingSights.ValueRW.TargetLocalVector);
                                    //targetHit.Position - targetPosition;
                                    //new float4x4(
                                    //    quaternion.LookRotation(targetCameraEntityLocalToWorld.Forward//math.normalizesafe( toTargetVector)
                                    //                            , math.up())
                                    //    , targetPosition)
                                    //.TransformPoint(targetHit.Position);
                                }

                                //   0//math.radians( (1 - math.sqrt(math.cos(leadRight * math.PI * 0.5f))) * 90f * (leadRight > 0 ? AimingSights.ValueRW.TrackingOffset : -1f))//*DeltaTime
                                //    , 0//math.radians((1 - math.sqrt(math.cos(leadUp * math.PI * 0.5f))) * 90f * (leadUp > 0 ? 1f : -1f))// * DeltaTime
                                ////math.degrees( MathUtilities.DotRatioToAngleRadians(leadRight)) * (leadRight>0?AimingSights.ValueRW.TrackingOffset:-1) * DeltaTime
                                ////, math.degrees( MathUtilities.DotRatioToAngleRadians(leadUp)) * (leadUp > 0 ? 1 : -1)* DeltaTime
                                //, 0 
                                //);
                                //UnityEngine.Debug.Log("lead right : "+math.degrees(math.asin(leadRight)));
                                AimingSights.ValueRW.TrackingAngle = new float2(
                                       //math.radians((1 - math.sqrt(math.cos(leadRight * math.PI * 0.5f))) * 90f * (leadRight > 0 ? AimingSights.ValueRW.TrackingOffset : -1f))//*DeltaTime
                                       // , math.radians((1 - math.sqrt(math.cos(leadUp * math.PI * 0.5f))) * 90f * (leadUp > 0 ? 1f : -1f))// * DeltaTime
                                       math.degrees(math.asin(math.saturate(math.abs(leadRight))))* (leadRight>0?1:-1f) //* DeltaTime
                                       ,
                                       math.degrees(math.asin(math.saturate(math.abs(leadUp)))) * (leadUp > 0 ? 1 : -1f)  //* DeltaTime
                                       );
                                //UnityEngine.Debug.Log("Tracking Angle X : " + AimingSights.ValueRW.TrackingAngle.x);
                                //);
                                //UnityEngine.Debug.Log(leadUp+" / "+ math.degrees(MathUtilities.DotRatioToAngleRadians(leadUp)) * (leadUp > 0 ? 1 : -1));
                                //AimingSights.ValueRW.TrackingAngle = new float2( //right*-90, up*-90
                                //    (1 - math.sqrt(math.cos(leadRight * math.PI * 0.5f))) * 90f * (leadRight > 0 ? 1f : -1f)
                                //    , (1 - math.sqrt(math.cos(leadUp * math.PI * 0.5f))) * 90f * (leadUp > 0 ? 1f : -1f)
                                //    );

                                ////var negativeLeadingForward = math.mul(leadingRotation, new float3(0, 0, 1));
                                ////var negativeLeadUp = math.dot(negativeLeadingForward, new float3(0, 1, 0));
                                ////var negativeLeadRight = math.dot(negativeLeadingForward, new float3(1, 0, 0));
                                ////AimingSights.TrackingAngle += new float2( //right*-90, up*-90
                                ////    (1 - math.sqrt(math.cos(negativeLeadRight * math.PI * 0.5f))) * -90f * (negativeLeadRight > 0 ? 1f : -1f)
                                ////    , (1 - math.sqrt(math.cos(negativeLeadUp * math.PI * 0.5f))) * -90f * (negativeLeadUp > 0 ? 1f : -1f)
                                ////    );

                                //AimingSights.ValueRW.TrackingAngle *= AimingSights.ValueRW.TrackingOffset;
                                //AimingSights.ValueRW.TrackingAngle *= DeltaTime;
                                AimingSights.ValueRW.CurrentlyTracking =math.saturate( AimingSights.ValueRW.CurrentlyTracking + 0.3f);
                                tracking = true;
                            }
                        }
                    }
                }


                if (!tracking)
                {
                    //AimingSights.ValueRW.TrackingAngle = 0;
                    AimingSights.ValueRW.CurrentlyTracking = 0;
                    AimingSights.ValueRW.TargetLocalVector = 0;
                    AimingSights.ValueRW.CachedLookInput = 0;
                    AimingSights.ValueRW.
                        TargetVector//TargetPosition 
                        = 0;
                }
            }

            state.Dependency =// AimingSightsJob job =
                              new AimingSightsJob()
                              {
                                  DeltaTime = SystemAPI.Time.DeltaTime,
                                  PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                                  entityManager = state.EntityManager,
                                  LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(false),
                                  LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(false),
                                  HitboxLookup = SystemAPI.GetComponentLookup<Hitbox>(false),
                                  BodyLookup = SystemAPI.GetComponentLookup<KinematicCharacterBody>(false),
                                  PlayerTagLookup = SystemAPI.GetComponentLookup<PlayerTag>(false)

                              }.Schedule(state.Dependency);
            //job.Schedule();
            state.Dependency.Complete();
        }

        [BurstCompile]
        public struct AimAssistHitsCollector : ICollector<RaycastHit>
        {
            public bool EarlyOutOnFirstHit => false;
            public float MaxFraction => 1f;
            public int NumHits { get; private set; }

            public RaycastHit TargetHit;

            private float _closestDirectionFraction;
            private float3 _cameraPosition;
            private float3 _cameraMovementDirection;
            private Entity _followedCharacter;
            private DynamicBuffer<IgnoreHitboxData> _ignoredEntitiesBuffer;
            private ComponentLookup<KinematicCharacterBody> _bodyLookup;
            private PhysicsWorld _world;
            private CollisionFilter _obstacleFilter;

            public AimAssistHitsCollector(Entity followedCharacter, DynamicBuffer<IgnoreHitboxData> ignoredEntitiesBuffer, float3 cameraPosition, float3 cameraMovementDirection,
                ComponentLookup<KinematicCharacterBody> bodyLookup, PhysicsWorld World, CollisionFilter filter)
            {
                NumHits = 0;
                TargetHit = default;

                _closestDirectionFraction = -1;
                _cameraPosition = cameraPosition;
                _cameraMovementDirection = cameraMovementDirection;
                _followedCharacter = followedCharacter;
                _ignoredEntitiesBuffer = ignoredEntitiesBuffer;
                _bodyLookup = bodyLookup;
                _world = World;
                _obstacleFilter = filter;
            }

            public bool AddHit(RaycastHit hit)
            {
                if (_followedCharacter == hit.Entity)
                {
                    return false;
                }

                //if (math.dot(hit.SurfaceNormal, ) < 0f)
                //{
                //    return false;
                //}

                for (int i = 0; i < _ignoredEntitiesBuffer.Length; i++)
                {
                    if (_ignoredEntitiesBuffer[i].hitboxEntity == hit.Entity)
                    {
                        return false;
                    }
                }

                RaycastInput input = new RaycastInput()
                {
                    Start = _cameraPosition,
                    End = hit.Position,
                    Filter = _obstacleFilter
                };

                if (_world.CastRay(input, out RaycastHit rayHit))
                {
                    return false;
                }
                TargetHit = hit;
                return true;
                //if (_bodyLookup.TryGetComponent(hit.Entity, out KinematicCharacterBody targetBody))
                //{

                //    float fraction = math.dot(_cameraMovementDirection, math.normalizesafe(targetBody.RelativeVelocity));
                //    // Process valid hit
                //    if (fraction > _closestDirectionFraction)
                //    {
                //        _closestDirectionFraction = fraction;
                //        TargetHit = hit;
                //    }
                //    NumHits++;

                //    return true;
                //}
                //return false;

            }
        }


        [BurstCompile]
        [WithAll(typeof(Simulate))]
        public partial struct AimingSightsJob : IJobEntity
        {
            public float DeltaTime;
            public PhysicsWorld PhysicsWorld;
            public EntityManager entityManager;

            public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            public ComponentLookup<LocalTransform> LocalTransformLookup;
            public ComponentLookup<Hitbox> HitboxLookup;
            public ComponentLookup<KinematicCharacterBody> BodyLookup;
            public ComponentLookup<PlayerTag> PlayerTagLookup;

            void Execute(
                Entity entity,
                //ref LocalTransform transform,
                ref AimingSights AimingSights
                )
            {
                LocalToWorld muzzleLocalToWorld = default;
                //LocalTransform laserLocalTransform = default;
                //float3 laserPointLocation = float3.zero;
                LocalToWorld targetCameraEntityLocalToWorld = default;

                bool isPlayer = PlayerTagLookup.HasComponent(entity);
              
                // if there is a followed entity, place the camera relatively to it
                if (LocalToWorldLookup.TryGetComponent(AimingSights.MuzzleEntity, out LocalToWorld muzzleLTW))
                {

                    // Select the real camera target
                    muzzleLocalToWorld = muzzleLTW;
                    targetCameraEntityLocalToWorld = muzzleLTW;
                    AimingSights.MuzzlePosition = muzzleLocalToWorld.Position;
                    if (LocalToWorldLookup.TryGetComponent(AimingSights.FollowedCameraEntity, out LocalToWorld cameraTarget))
                    {
                        targetCameraEntityLocalToWorld = cameraTarget;

                        float range = 100f;

                        float3 rayStart = targetCameraEntityLocalToWorld.Position + 
                            targetCameraEntityLocalToWorld.Forward * math.max( math.distance(targetCameraEntityLocalToWorld.Position, muzzleLocalToWorld.Position),1);
                        RaycastInput ray = new RaycastInput()
                        {
                            Start = rayStart,
                            End = rayStart + targetCameraEntityLocalToWorld.Forward * range,
                            Filter = AimingSights.RayFilter//CollisionFilter.Default
                        };

                        bool tracking = false;
                        var buffer = entityManager.GetBuffer<IgnoreHitboxData>(entity);

                        if (PhysicsWorld.CastRay(ray, out Unity.Physics.RaycastHit hit))
                        {
                            //if (HitboxLookup.HasComponent(hit.Entity)){

                            //}
                     
                            //UnityEngine.Debug.Log(tracking+"/" +AimingSights.TrackingOffset);

                            float3 targetVector = hit.Position - muzzleLocalToWorld.Position;
                            float3 targetDir = math.normalizesafe(targetVector);

                            RaycastInput secondRay = new RaycastInput()
                            {
                                Start = muzzleLocalToWorld.Position,
                                End = hit.Position,
                                Filter = AimingSights.RayFilter
                            };
                            RayCastObstructionHitsCollector collector = new RayCastObstructionHitsCollector(buffer, targetDir);
                            PhysicsWorld.CastRay(secondRay, ref collector);
                            //SphereCastCustom<RayCastObstructionHitsCollector>(ray.Start, 0.001f, targetDir, math.length(targetVector), ref collector, AimingSights.RayFilter,
                            //QueryInteraction.IgnoreTriggers);

                            if (collector.NumHits > 0)
                            {
                                AimingSights.LaserPointerPosition = collector.ClosestHit.Position;
                                AimingSights.MuzzleForward = math.normalizesafe(collector.ClosestHit.Position - muzzleLocalToWorld.Position);
                            }
                            else
                            {
                                AimingSights.LaserPointerPosition = hit.Position;
                                AimingSights.MuzzleForward = math.normalizesafe(hit.Position - muzzleLocalToWorld.Position);
                            }
                            //} while (hitMyself && remainRange > 0);
                        }
                        else
                        {
                            float3 targetVector = ray.End - muzzleLocalToWorld.Position;
                            float3 targetDir = math.normalizesafe(targetVector);

                            RaycastInput secondRay = new RaycastInput()
                            {
                                Start = muzzleLocalToWorld.Position,
                                End = ray.End,
                                Filter = AimingSights.RayFilter
                            };
                            RayCastObstructionHitsCollector collector = new RayCastObstructionHitsCollector(buffer, targetDir);
                            PhysicsWorld.CastRay(secondRay, ref collector);
                            //SphereCastCustom(ray.Start, 0.001f, targetDir, math.length(targetVector), ref collector, AimingSights.RayFilter,
                            //QueryInteraction.IgnoreTriggers);

                            if (collector.NumHits > 0)
                            {
                                AimingSights.LaserPointerPosition = collector.ClosestHit.Position;
                                AimingSights.MuzzleForward = math.normalizesafe(collector.ClosestHit.Position - muzzleLocalToWorld.Position);
                            }
                            else
                            {
                                AimingSights.LaserPointerPosition = ray.End;
                                AimingSights.MuzzleForward = math.normalizesafe(ray.End - muzzleLocalToWorld.Position);
                            }
                        }

                        //if (isPlayer)// && rayHit)
                        //{
                        //    RaycastInput trackingRay = new RaycastInput()
                        //    {
                        //        Start = rayStart,
                        //        End = rayStart + targetCameraEntityLocalToWorld.Forward * range,
                        //        Filter = AimingSights.TrackingRayFilter
                        //    };

                        //    AimAssistHitsCollector collector = new AimAssistHitsCollector(entity, ignoredHitboxesBuffer, rayStart, AimingSights.CameraMovementDirection, 
                        //        BodyLookup, PhysicsWorld, AimingSights.ObstacleFilter);

                        //    //if (PhysicsWorld.CastRay(trackingRay, out hit))

                        //    if(PhysicsWorld.CastRay(trackingRay, ref collector))
                        //    {
                        //        if (BodyLookup.HasComponent(entity))
                        //        {
                        //            RaycastHit targetHit = collector.TargetHit;
                        //            KinematicCharacterBody myBody = BodyLookup[entity];
                        //            //KinematicCharacterBody targetBody = default;
                        //            Entity targetEntity = Entity.Null;
                        //            bool targetFound = false;
                        //            if (BodyLookup.HasComponent(targetHit.Entity))
                        //            {
                        //                targetFound = true;
                        //                targetEntity = targetHit.Entity;
                        //            }
                        //            else if (HitboxLookup.HasComponent(targetHit.Entity))
                        //            {
                        //                var targetHitbox = HitboxLookup[targetHit.Entity];
                        //                if (BodyLookup.HasComponent(targetHitbox.Owner))
                        //                {
                        //                    targetFound = true;
                        //                    targetEntity = targetHitbox.Owner;
                        //                }
                        //            }
                                    

                        //            if (targetFound) {
                        //                var myVelocity = myBody.RelativeVelocity;
                        //                var targetVelocity = BodyLookup[targetEntity].RelativeVelocity;

                        //                var myPosition = LocalToWorldLookup[entity].Position;
                        //                var targetPosition = LocalToWorldLookup[targetEntity].Position;
                        //                var myEstimatePosition = (myPosition + myVelocity);
                        //                var toTargetVector = targetPosition - myPosition;

                        //                var myMat = new float4x4(quaternion.LookRotation(math.normalizesafe(toTargetVector),
                        //                    new float3(0, 1, 0)), myEstimatePosition);
                        //                //var targetMat = new float4x4(quaternion.LookRotation(math.normalizesafe(toTargetVector),
                        //                //    new float3(0, 1, 0)), targetPosition);



                        //                var lead =
                        //                    //myMat.InverseTransformRotation(quaternion.LookRotation( 
                        //                    //math.normalizesafe((targetPosition+targetVelocity)-myPosition), new float3(0,1,0)));
                        //                    myMat.InverseTransformRotation(quaternion.LookRotation(
                        //                    math.normalizesafe((targetPosition + targetVelocity) - myEstimatePosition), new float3(0, 1, 0)));
                        //                //var negativeLead =
                        //                //    targetMat.InverseTransformRotation(
                        //                //        quaternion.LookRotation(
                        //                //        math.normalizesafe((myPosition + myVelocity)- targetPosition), new float3(0,1,0))
                        //                //        );
                        //                //targetLTW.Value.InverseTransformRotation(quaternion.LookRotation(
                        //                //math.normalizesafe((myPosition + myVelocity) - targetPosition), new float3(0,1,0))) ;

                        //                var leadingRotation = lead;//negativeLead;//
                        //                                           //           math.mul(lead, /*math.inverse*/(negativeLead));
                        //                var leadingForward = math.mul(leadingRotation, new float3(0, 0, 1));
                        //                var leadUp = math.dot(leadingForward, new float3(0, 1, 0));
                        //                var leadRight = math.dot(leadingForward, new float3(1, 0, 0));
                        //                AimingSights.TrackingAngle = new float2( //right*-90, up*-90
                        //                    (1 - math.sqrt(math.cos(leadRight * math.PI * 0.5f))) * 90f * (leadRight > 0 ? 1f : -1f)
                        //                    , (1 - math.sqrt(math.cos(leadUp * math.PI * 0.5f))) * 90f * (leadUp > 0 ? 1f : -1f)
                        //                    );

                        //                //var negativeLeadingForward = math.mul(leadingRotation, new float3(0, 0, 1));
                        //                //var negativeLeadUp = math.dot(negativeLeadingForward, new float3(0, 1, 0));
                        //                //var negativeLeadRight = math.dot(negativeLeadingForward, new float3(1, 0, 0));
                        //                //AimingSights.TrackingAngle += new float2( //right*-90, up*-90
                        //                //    (1 - math.sqrt(math.cos(negativeLeadRight * math.PI * 0.5f))) * -90f * (negativeLeadRight > 0 ? 1f : -1f)
                        //                //    , (1 - math.sqrt(math.cos(negativeLeadUp * math.PI * 0.5f))) * -90f * (negativeLeadUp > 0 ? 1f : -1f)
                        //                //    );

                        //                AimingSights.TrackingAngle *= AimingSights.TrackingOffset;
                        //                AimingSights.TrackingAngle *= DeltaTime;
                        //                tracking = true;
                        //            }
                        //        }
                        //    }
                        //}


                        //if (!tracking)
                        //    AimingSights.TrackingAngle = 0;
                        //if (LocalTransformLookup.TryGetComponent(AimingSights.LaserPointEntity, out LocalTransform laserLTW))
                        //{
                        //    //laserLocalTransform = laserLTW;
                        //    /*laserLocalTransform*/
                        //    laserLTW.Position = laserPointLocation;
                        //}
                        //if(LocalToWorldLookup.TryGetComponent(AimingSights.LaserPointEntity, out LocalToWorld laserLTW))
                        //{
                        //    UnityEngine.Debug.Log(laserPointLocation);
                        //    laserLTW.Value = float4x4.TRS(laserPointLocation, laserLTW.Rotation, laserLTW.Value.Scale());
                        //}

                        //if(WeaponLookup.TryGetComponent(AimingSights.WeaponEntity, out Weapon weapon))
                        //{
                        //    weapon.MuzzlePosition = AimingSights.MuzzlePosition;
                        //    weapon.MuzzleForward = AimingSights.MuzzleForward;
                        //}
                    }
                    //transform.Rotation = quaternion.LookRotationSafe(targetCameraEntityLocalToWorld.Forward, new float3(0, 1, 0));

                    //transform.Position = targetEntityLocalToWorld.Position;




                }
            }
        }
    }
}