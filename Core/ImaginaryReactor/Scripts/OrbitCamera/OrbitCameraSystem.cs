using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.CharacterController;

namespace ImaginaryReactor {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial struct OrbitCameraSystem : ISystem
    {
    

        public struct CameraObstructionHitsCollector : ICollector<ColliderCastHit>
        {
            public bool EarlyOutOnFirstHit => false;
            public float MaxFraction => 1f;
            public int NumHits { get; private set; }

            public ColliderCastHit ClosestHit;

            private float _closestHitFraction;
            private float3 _cameraDirection;
            private Entity _followedCharacter;
            private DynamicBuffer<OrbitCameraIgnoredEntityBufferElement> _ignoredEntitiesBuffer;

            public CameraObstructionHitsCollector(Entity followedCharacter, DynamicBuffer<OrbitCameraIgnoredEntityBufferElement> ignoredEntitiesBuffer, float3 cameraDirection)
            {
                NumHits = 0;
                ClosestHit = default;

                _closestHitFraction = float.MaxValue;
                _cameraDirection = cameraDirection;
                _followedCharacter = followedCharacter;
                _ignoredEntitiesBuffer = ignoredEntitiesBuffer;
            }

            public bool AddHit(ColliderCastHit hit)
            {
                if (_followedCharacter == hit.Entity)
                {
                    return false;
                }

                if (math.dot(hit.SurfaceNormal, _cameraDirection) < 0f || !PhysicsUtilities.IsCollidable(hit.Material))
                {
                    return false;
                }

                for (int i = 0; i < _ignoredEntitiesBuffer.Length; i++)
                {
                    if (_ignoredEntitiesBuffer[i].Entity == hit.Entity)
                    {
                        return false;
                    }
                }

                // Process valid hit
                if (hit.Fraction < _closestHitFraction)
                {
                    _closestHitFraction = hit.Fraction;
                    ClosestHit = hit;
                }
                NumHits++;

                return true;
            }
        }

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<OrbitCamera, OrbitCameraControl>().Build());
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            OrbitCameraJob job = new OrbitCameraJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(false),
                CameraTargetLookup = SystemAPI.GetComponentLookup<CameraTarget>(true),
                KinematicCharacterBodyLookup = SystemAPI.GetComponentLookup<KinematicCharacterBody>(true),
                SightsLookup = SystemAPI.GetComponentLookup<AimingSights>(false),
                TelescopeLookup = SystemAPI.GetComponentLookup<Telescope>(false),
                MainCameraLookup = SystemAPI.GetComponentLookup<MainEntityCamera>(false),
                PlayerTagLookup = SystemAPI.GetComponentLookup<PlayerTag>(false),
            };
            job.Schedule();
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
    public partial struct OrbitCameraJob : IJobEntity
        {
            public float DeltaTime;
            public PhysicsWorld PhysicsWorld;

            public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            [ReadOnly] public ComponentLookup<CameraTarget> CameraTargetLookup;
            [ReadOnly] public ComponentLookup<KinematicCharacterBody> KinematicCharacterBodyLookup;
            public ComponentLookup<AimingSights> SightsLookup;
            public ComponentLookup<Telescope> TelescopeLookup;
            public ComponentLookup<MainEntityCamera> MainCameraLookup;
            public ComponentLookup<PlayerTag> PlayerTagLookup;

            void Execute(
                Entity entity,
                ref LocalTransform transform,
                ref OrbitCamera orbitCamera,
                in OrbitCameraControl cameraControl,
                in DynamicBuffer<OrbitCameraIgnoredEntityBufferElement> ignoredEntitiesBuffer)
            {
                float zoomProgress = 0;
                bool scopeFirstPerson = false;

                if (TelescopeLookup.HasComponent(entity) && MainCameraLookup.HasComponent(entity))
                {
                    Telescope telescope = TelescopeLookup[entity];
                    MainEntityCamera mainCamera = MainCameraLookup[entity];

                    zoomProgress = math.saturate(
                        math.clamp(cameraControl.Zoom, telescope.ZoomProgress - DeltaTime * telescope.ZoomSpeed, telescope.ZoomProgress + DeltaTime * telescope.ZoomSpeed)
                        );
                    telescope.ZoomProgress = zoomProgress;

                    scopeFirstPerson = telescope.FirstPerson;

                    mainCamera.Fov = math.lerp(mainCamera.BaseFov, telescope.FovWhenZoom, telescope.ZoomProgress);

                    MainCameraLookup[entity] = mainCamera;
                    TelescopeLookup[entity] = telescope;
                }

                // if there is a followed entity, place the camera relatively to it
                if (LocalToWorldLookup.TryGetComponent(cameraControl.FollowedCharacterEntity, out LocalToWorld characterLTW))
                {
                    bool hasSights = false;
                    AimingSights sights = default;
                    if (SightsLookup.HasComponent(cameraControl.FollowedCharacterEntity))
                    {
                        hasSights = true;
                        sights = SightsLookup[cameraControl.FollowedCharacterEntity];
                    }
                    // Select the real camera target
                    LocalToWorld targetEntityLocalToWorld = default;
                    LocalToWorld cameraLocalToWorld = LocalToWorldLookup[entity];
                    if (CameraTargetLookup.TryGetComponent(cameraControl.FollowedCharacterEntity, out CameraTarget cameraTarget) &&
                        LocalToWorldLookup.TryGetComponent(cameraTarget.TargetEntity, out LocalToWorld camTargetLTW))
                    {
                        targetEntityLocalToWorld = camTargetLTW;
                    }
                    else
                    {
                        targetEntityLocalToWorld = characterLTW;
                    }

                    // Rotation
                    {
                        float zoomMultiply = math.remap(0, 1, 1, 0.25f, cameraControl.Zoom);

                        float aimAssist = sights.CurrentlyTracking > 0.1f ? 1 : 0;
                        float2 targetInputForTracking = 0;//math.radians(sights.TrackingAngle);
                        if (aimAssist < 0.1f ||
                            math.lengthsq(sights.TrackingAngle) < 0.01f || math.abs(cameraControl.Look.x) < 0.01f ||
                            //((sights.TrackingAngle.y > 0 && pitchAngleChange < 0) || (sights.TrackingAngle.y < 0 && pitchAngleChange > 0)) ||
                            ((sights.TrackingAngle.x > 0 && cameraControl.Look.x < 0) || (sights.TrackingAngle.x < 0 && cameraControl.Look.x > 0)))
                        {
                            //aimAssistStrength = 0;
                            aimAssist = 0;
                        }
                        else
                        {
                            float x = cameraControl.Look.x * zoomMultiply;
                            float a = (targetInputForTracking.x / orbitCamera.RotationSpeed);
                            aimAssist *= (x < a + 0.5f && x > a - 0.5f) ? math.cos((x * math.PI * 2) - (a * math.PI * 2)) * 0.5f + 0.5f : 0;

                        }
                        //aimAssist = sights.CurrentlyTracking > 0 ? 1 : 0;

                        quaternion rot = quaternion.LookRotationSafe(orbitCamera.PlanarForward, targetEntityLocalToWorld.Up);

                        // Handle rotating the camera along with character's parent entity (moving platform)
                        if (orbitCamera.RotateWithCharacterParent && KinematicCharacterBodyLookup.TryGetComponent(cameraControl.FollowedCharacterEntity, out KinematicCharacterBody characterBody))
                        {
                            KinematicCharacterUtilities.AddVariableRateRotationFromFixedRateRotation(ref rot, characterBody.RotationFromParent, DeltaTime, characterBody.LastPhysicsUpdateDeltaTime);
                            orbitCamera.PlanarForward = math.normalizesafe(MathUtilities.ProjectOnPlane(MathUtilities.GetForwardFromRotation(rot), targetEntityLocalToWorld.Up));
                        }
                        // Yaw
                        float yawAngleChange = cameraControl.Look.x * zoomMultiply * orbitCamera.RotationSpeed;     //0.5 -> turn 90 degrees per 1 second
                        //var quat = quaternion.LookRotation(math.left(), math.up());
                        //var dot = math.dot(quat, quaternion.LookRotation(math.forward(), math.up()));
                        //var dir = math.dot(quat, quaternion.LookRotation(math.right(), math.up()));
                        //if (PlayerTagLookup.HasComponent(cameraControl.FollowedCharacterEntity))
                        //{ UnityEngine.Debug.Log(dir); }
                        //yawAngleChange = math.lerp(yawAngleChange,
                        //    //math.degrees(math.asin(math.clamp(sights.TrackingAngle.x , -1, 1)))*sights.TrackingOffset* DeltaTime 
                        //    //math.degrees(math.acos(dot))*DeltaTime*dir>0.5f?1:-1 // 90 degrees turn per second
                        //    math.degrees(math.asin(sights.TrackingAngle.x)) * DeltaTime// * sights.TrackingAngle.z > 0.5f ? 1 : -1
                        //    , aimAssist
                        //    );
                        
                        if (sights.CurrentlyTracking > 0.1f && sights.CurrentlyTracking < 0.5f)
                        {
                            sights.CachedLookInput//.x
                                = cameraControl.Look;//.x;
                        }

                        //float3 vectorToTarget = (sights.TargetVector 
                        //    - cameraLocalToWorld.Up * sights.TargetLocalVector.y - cameraLocalToWorld.Right * sights.TargetLocalVector.x*0.5f
                        //    + cameraLocalToWorld.Up * (cameraControl.Look.y - sights.CachedLookInput.y) * sights.TrackingOffset //* sights.CurrentlyTracking
                        //    + cameraLocalToWorld.Right * (cameraControl.Look.x - sights.CachedLookInput.x)
                        //    //sights.TargetLocalVector
                        //    //sights.TargetLocalVector//new float3(0,sights.TargetLocalVector.y,0)

                        //    ) - LocalToWorldLookup[entity].Position;
                        //if (PlayerTagLookup.HasComponent(cameraControl.FollowedCharacterEntity))
                        //{
                        //    UnityEngine.Debug.Log(cameraControl.Look.y - sights.CachedLookInput.y);
                        //}
                        //sights.CachedLookInput.y = cameraControl.Look.y;

                        //if (hasSights)
                        //{
                        //    yawAngleChange += sights.TargetVector.x * sights.CurrentlyTracking;
                        //}
                        //float2 aimAssistStrength = sights.CurrentlyTracking;
                        //if (hasSights
                        //    && math.abs(sights.TrackingAngle.x) > 0 &&
                        //    !((sights.TrackingAngle.x > 0 && yawAngleChange < 0) || (sights.TrackingAngle.x < 0 && yawAngleChange > 0))
                        //    )
                        //{
                        //    float targetInputForTracking = sights.TrackingAngle.x / orbitCamera.RotationSpeed;
                        //    //UnityEngine.Debug.Log(math.distance(cameraControl.Look.x, targetInputForTracking));

                        //    if (sights.TrackingAngle.x > 0)
                        //    {
                        //        if (yawAngleChange < sights.TrackingAngle.x)
                        //        {
                        //            aimAssistStrength.x = math.smoothstep(0, 0.9f,//math.sqrt(math.sin( //math.smoothstep(0, 0.9f, 
                        //                math.saturate(math.remap(0, math.max(0.1f, targetInputForTracking - 0.1f), 0, 1, cameraControl.Look.x))//;
                        //                );// * math.PI * 0.5f);
                        //        }
                        //        else
                        //        {
                        //            aimAssistStrength.x =// math.sqrt(math.cos(//
                        //               math.smoothstep(0, 0.9f,
                        //                 math.saturate(math.remap(math.min(targetInputForTracking + 0.1f, 0.9f), 1, 1, 0, cameraControl.Look.x))//;
                        //                );// * math.PI * 0.5f) * 0.9f;
                        //            //UnityEngine.Debug.Log(t);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (sights.TrackingAngle.x < yawAngleChange)
                        //        {
                        //            aimAssistStrength.x = math.smoothstep(0, 0.9f, //math.sqrt(math.sin( //math.smoothstep(0, 0.9f, 
                        //              math.saturate(math.remap(0, math.max(0.1f, targetInputForTracking * -1 - 0.1f), 0, 1, cameraControl.Look.x * -1))//;
                        //              );// * math.PI * 0.5f);
                        //        }
                        //        else
                        //        {
                        //            aimAssistStrength.x =// math.sqrt(math.cos(//
                        //               math.smoothstep(0, 0.9f,
                        //                 math.saturate(math.remap(math.min(targetInputForTracking * -1 + 0.1f, 0.9f), 1, 1, 0, cameraControl.Look.x * -1))//;
                        //                );// * math.PI * 0.5f) * 0.9f;
                        //            //UnityEngine.Debug.Log(t);
                        //            // UnityEngine.Debug.Log(t);
                        //        }
                        //    }
                        //    //aimAssistStrength.x = 1;
                        //    //yawAngleChange = math.lerp(yawAngleChange, sights.TrackingAngle.x * sights.TrackingOffset, sights.CurrentlyTracking);
                        //    //yawAngleChange = math.lerp(yawAngleChange, sights.TrackingAngle.x, sights.CurrentlyTracking * t);
                        //    //aimAssistStrength.x *= sights.CurrentlyTracking;
                        //}

                        quaternion yawRotation = quaternion.Euler(targetEntityLocalToWorld.Up * math.radians(yawAngleChange));   //quaternion.Euler(targetEntityLocalToWorld.Up * math.radians(90*DeltaTime)); // turn 90 degrees per second

                        quaternion yawRotationTracking = quaternion.Euler(targetEntityLocalToWorld.Up * math.radians(
                           sights.TrackingAngle.x*DeltaTime //*0.5f//math.degrees(math.asin(sights.TrackingAngle.x))*DeltaTime
                            ));
                        //if (hasSights & sights.CurrentlyTracking > 0)
                        //{
                        //    orbitCamera.PlanarForward = math.normalizesafe( MathUtilities.ProjectOnPlane((sights.TargetVector- LocalToWorldLookup[entity].Position
                        //        ), new float3(0, 1, 0)));
                        //}
                        //else
                        //{
                        orbitCamera.PlanarForward = math.normalizesafe(
                                //math.lerp(
                                    math.rotate(math.slerp( yawRotation, yawRotationTracking, aimAssist), orbitCamera.PlanarForward)//,
                                //math.normalizesafe(MathUtilities.ProjectOnPlane(vectorToTarget, new float3(0, 1, 0)))
                                //, aimAssistStrength.x)
                                );
                        //}

                        // Pitch
                        float pitchAngleChange = cameraControl.Look.y * zoomMultiply * orbitCamera.RotationSpeed;

                        //pitchAngleChange = math.lerp(
                        //    pitchAngleChange,
                        //    math.degrees(math.asin(math.clamp(sights.TrackingAngle.y, -1, 1))) * sights.TrackingOffset * DeltaTime,
                        //    aimAssist*0.9f);
                        //if (hasSights)
                        //{
                        //    pitchAngleChange += sights.TargetVector.y * sights.TargetVector.w;
                        //}
                        //if (hasSights
                        //    && math.abs(sights.TrackingAngle.y) > 0 &&
                        //   !((sights.TrackingAngle.y > 0 && pitchAngleChange < 0) || (sights.TrackingAngle.y < 0 && pitchAngleChange > 0))
                        //   )
                        //{
                        //    float targetInputForTracking = sights.TrackingAngle.y / orbitCamera.RotationSpeed;
                        //    //UnityEngine.Debug.Log(math.distance(cameraControl.Look.y, targetInputForTracking));
                        //    //float t = 0;
                        //    if (sights.TrackingAngle.y > 0)
                        //    {
                        //        if (pitchAngleChange < sights.TrackingAngle.y)
                        //        {
                        //            aimAssistStrength.y = math.smoothstep(0, 0.9f,//math.sqrt(math.sin( //math.smoothstep(0, 0.9f, 
                        //                math.saturate(math.remap(0, math.max(0.1f, targetInputForTracking - 0.1f), 0, 1, cameraControl.Look.y))//;
                        //                );// * math.PI * 0.5f);
                        //        }
                        //        else
                        //        {
                        //            aimAssistStrength.y =// math.sqrt(math.cos(//
                        //               math.smoothstep(0, 0.9f,
                        //                 math.saturate(math.remap(math.min(targetInputForTracking + 0.1f, 0.9f), 1, 1, 0, cameraControl.Look.y))//;
                        //                );// * math.PI * 0.5f) * 0.9f;
                        //            //UnityEngine.Debug.Log(t);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (sights.TrackingAngle.y < pitchAngleChange)
                        //        {
                        //            aimAssistStrength.y = math.smoothstep(0, 0.9f, //math.sqrt(math.sin( //math.smoothstep(0, 0.9f, 
                        //              math.saturate(math.remap(0, math.max(0.1f, targetInputForTracking * -1 - 0.1f), 0, 1, cameraControl.Look.y * -1))//;
                        //              );// * math.PI * 0.5f);
                        //        }
                        //        else
                        //        {
                        //            aimAssistStrength.y =// math.sqrt(math.cos(//
                        //               math.smoothstep(0, 0.9f,
                        //                 math.saturate(math.remap(math.min(targetInputForTracking * -1 + 0.1f, 0.9f), 1, 1, 0, cameraControl.Look.y * -1))//;
                        //                );// * math.PI * 0.5f) * 0.9f;
                        //            //UnityEngine.Debug.Log(t);
                        //            // UnityEngine.Debug.Log(t);
                        //        }
                        //    }
                        //    //aimAssistStrength.y = 1;
                        //    //pitchAngleChange = math.lerp(pitchAngleChange, sights.TrackingAngle.y, sights.CurrentlyTracking);
                        //    //pitchAngleChange = math.lerp(pitchAngleChange, sights.TrackingAngle.y, sights.CurrentlyTracking * 1);
                        //    //aimAssistStrength.y *= sights.CurrentlyTracking;
                        //}
                        //aimAssistStrength *= 0.5f;

                        //orbitCamera.PitchAngle += -pitchAngleChange;
                        //if(hasSights)
                        //{
                        //    orbitCamera.PitchAngle += sights.TrackingAngle.y * sights.TrackingOffset * DeltaTime;
                        //}

                        //if (hasSights & sights.TargetPosition.w > 0)
                        //{
                        // orbitCamera.PlanarForward = math.normalizesafe(sights.TargetPosition.xyz - targetEntityLocalToWorld.Position);
                        //}
                        //else
                        //{
                        //    orbitCamera.PitchAngle 
                        //}



                        //aimAssist *= aimAssistStrength.x;

                        //aimAssist = math.saturate(aimAssist);
                        //aimAssistStrength *= math.abs(targetInputForTracking.x)> cameraControl.Look.x?1- math.abs(cameraControl.Look) : math.abs(cameraControl.Look);

                        //aimAssistStrength *= math.remap( 0,orbitCamera.RotationSpeed,1,0,
                        //    math.distance(new float2(yawAngleChange, pitchAngleChange) ,sights.TrackingAngle ));

                        orbitCamera.PitchAngle = math.lerp(orbitCamera.PitchAngle - pitchAngleChange, orbitCamera.PitchAngle - sights.TrackingAngle.y*DeltaTime, aimAssist);
                        orbitCamera.PitchAngle = math.clamp(orbitCamera.PitchAngle, orbitCamera.MinVAngle, orbitCamera.MaxVAngle);
                        quaternion pitchRotation = quaternion.Euler(math.right() * 
                             math.radians(orbitCamera.PitchAngle)
                            );
                        //quaternion pitchRotationForTracking = quaternion.Euler(math.right() *
                        //     math.radians(orbitCamera.PitchAngle)
                        //    );

                        //pitchRotation = math.slerp(pitchRotation, pitchRotationForTracking, aimAssist);

                                                // Final rotation
                        rot = quaternion.LookRotationSafe(orbitCamera.PlanarForward, targetEntityLocalToWorld.Up);

                        //float3 directionToTarget = math.normalizesafe(vectorToTarget);
                        orbitCamera.ThirdPersonRotation =
                            //math.nlerp(
                            math.mul(rot, pitchRotation);//,
                            
                            //   quaternion.LookRotationSafe(directionToTarget, directionToTarget.y>0.99? -orbitCamera.PlanarForward : math.up()
                            //   ) //math.normalizesafe(MathUtilities.ProjectOnPlane(vectorToTarget, new float3(0, 1, 0)))
                            
                            //,
                            //aimAssist//math.saturate( math.length(aimAssistStrength )) 
                            //);
                        //if (PlayerTagLookup.HasComponent(cameraControl.FollowedCharacterEntity))
                        //    UnityEngine.Debug.Log(aimAssist);

                        orbitCamera.PlanarForward = math.normalizesafe(MathUtilities.ProjectOnPlane(MathUtilities.GetForwardFromRotation(orbitCamera.ThirdPersonRotation), targetEntityLocalToWorld.Up));
                        float upDot = MathUtilities.GetForwardFromRotation(orbitCamera.ThirdPersonRotation).y;
                        orbitCamera.PitchAngle = math.degrees( math.asin(math.clamp(-upDot, -1, 1)));  // math.degrees( MathUtilities.DotRatioToAngleRadians(math.dot(orbitCamera.PlanarForward,math.up())));

                        //float upDot = MathUtilities.GetForwardFromRotation(orbitCamera.ThirdPersonRotation).y;//math.dot(MathUtilities.GetForwardFromRotation(orbitCamera.ThirdPersonRotation), math.up());
                        //UnityEngine.Debug.Log("Pitch Angle : " +// math.sqrt(
                        //     math.degrees(math.asin(math.abs(upDot)) )
                        //    //* (upDot > 0 ? 90 : -90) //math.degrees(MathUtilities.DotRatioToAngleRadians(math.dot(orbitCamera.PlanarForward, math.up())))
                        //    );

                        if (hasSights)
                        {
                            sights.CameraMovementDirection = math.mul(orbitCamera.ThirdPersonRotation, orbitCamera.PlanarForward);
                        }
                    }

                    float3 cameraForward = MathUtilities.GetForwardFromRotation(orbitCamera.ThirdPersonRotation);//transform.Rotation);

                    // Distance input
                    float desiredDistanceMovementFromInput = cameraControl.Zoom * orbitCamera.DistanceMovementSpeed;
                    float desireDistance = math.clamp(orbitCamera.TargetDistance + desiredDistanceMovementFromInput, orbitCamera.MinDistance, orbitCamera.MaxDistance);
                    orbitCamera.TargetDistance = //scopeFirstPerson ? 
                        //math.lerp(desireDistance, 0, zoomProgress)
                        //:
                        desireDistance;
                    orbitCamera.CurrentDistanceFromMovement = math.lerp(orbitCamera.CurrentDistanceFromMovement, orbitCamera.TargetDistance, MathUtilities.GetSharpnessInterpolant(orbitCamera.DistanceMovementSharpness, DeltaTime));

                    // Obstructions
                    if (orbitCamera.ObstructionRadius > 0f)
                    {
                        float obstructionCheckDistance = orbitCamera.CurrentDistanceFromMovement;

                        CameraObstructionHitsCollector collector = new CameraObstructionHitsCollector(cameraControl.FollowedCharacterEntity, ignoredEntitiesBuffer, cameraForward);
                        PhysicsWorld.SphereCastCustom<CameraObstructionHitsCollector>(
                            targetEntityLocalToWorld.Position,
                            orbitCamera.ObstructionRadius,
                            -cameraForward,
                            obstructionCheckDistance,
                            ref collector,
                            CollisionFilter.Default,
                            QueryInteraction.IgnoreTriggers);

                        float newObstructedDistance = obstructionCheckDistance;
                        if (collector.NumHits > 0)
                        {
                            newObstructedDistance = obstructionCheckDistance * collector.ClosestHit.Fraction;

                            // Redo cast with the interpolated body transform to prevent FixedUpdate jitter in obstruction detection
                            if (orbitCamera.PreventFixedUpdateJitter)
                            {
                                RigidBody hitBody = PhysicsWorld.Bodies[collector.ClosestHit.RigidBodyIndex];
                                if (LocalToWorldLookup.TryGetComponent(hitBody.Entity, out LocalToWorld hitBodyLocalToWorld))
                                {
                                    hitBody.WorldFromBody = new RigidTransform(quaternion.LookRotationSafe(hitBodyLocalToWorld.Forward, hitBodyLocalToWorld.Up), hitBodyLocalToWorld.Position);

                                    collector = new CameraObstructionHitsCollector(cameraControl.FollowedCharacterEntity, ignoredEntitiesBuffer, cameraForward);
                                    hitBody.SphereCastCustom<CameraObstructionHitsCollector>(
                                        targetEntityLocalToWorld.Position,
                                        orbitCamera.ObstructionRadius,
                                        -cameraForward,
                                        obstructionCheckDistance,
                                        ref collector,
                                        CollisionFilter.Default,
                                        QueryInteraction.IgnoreTriggers);

                                    if (collector.NumHits > 0)
                                    {
                                        newObstructedDistance = obstructionCheckDistance * collector.ClosestHit.Fraction;
                                    }
                                }
                            }
                        }

                        // Update current distance based on obstructed distance
                        if (orbitCamera.CurrentDistanceFromObstruction < newObstructedDistance)
                        {
                            // Move outer
                            orbitCamera.CurrentDistanceFromObstruction = math.lerp(orbitCamera.CurrentDistanceFromObstruction, newObstructedDistance, MathUtilities.GetSharpnessInterpolant(orbitCamera.ObstructionOuterSmoothingSharpness, DeltaTime));
                        }
                        else if (orbitCamera.CurrentDistanceFromObstruction > newObstructedDistance)
                        {
                            // Move inner
                            orbitCamera.CurrentDistanceFromObstruction = math.lerp(orbitCamera.CurrentDistanceFromObstruction, newObstructedDistance, MathUtilities.GetSharpnessInterpolant(orbitCamera.ObstructionInnerSmoothingSharpness, DeltaTime));
                        }
                    }
                    else
                    {
                        orbitCamera.CurrentDistanceFromObstruction = orbitCamera.CurrentDistanceFromMovement;
                    }

                    // Calculate final camera position from targetposition + rotation + distance
                    //bool switchShoulderView = false;
                    if (cameraControl.SwitchView)
                    {
                        orbitCamera.viewDirectionRate = orbitCamera.viewDirectionRate > 0 ? 0.99f : -0.99f;
                        //switchShoulderView = true;
                    }
                    float shouderSwitchSpeed = 10;
                    bool willReachEndOfShoulderPoint =  //!(orbitCamera.viewDirectionRate > 0.999f|| orbitCamera.viewDirectionRate < -0.999f) 
                        //&&
                        ((orbitCamera.viewDirectionRate < 0.999f && orbitCamera.viewDirectionRate > 0 && orbitCamera.viewDirectionRate - DeltaTime * shouderSwitchSpeed < 0) || 
                        (orbitCamera.viewDirectionRate > -0.999f && orbitCamera.viewDirectionRate < 0 && orbitCamera.viewDirectionRate + DeltaTime * shouderSwitchSpeed > 0));

                    orbitCamera.viewDirectionRate = orbitCamera.viewDirectionRate > 0 ?
                        orbitCamera.viewDirectionRate<0.999f? 
                        (willReachEndOfShoulderPoint ?-1 : orbitCamera.viewDirectionRate-DeltaTime* shouderSwitchSpeed)
                            : orbitCamera.viewDirectionRate
                        :
                        orbitCamera.viewDirectionRate > -0.999f ? 
                        (willReachEndOfShoulderPoint ? 1:orbitCamera.viewDirectionRate + DeltaTime* shouderSwitchSpeed)
                            : orbitCamera.viewDirectionRate
                        ;

                    //UnityEngine.Debug.Log("Orbit Camera view direction rate : " + orbitCamera.viewDirectionRate);

                    float shoulderRate = orbitCamera.viewDirectionRate>0? orbitCamera.viewDirectionRate : orbitCamera.viewDirectionRate+1;
                    //UnityEngine.Debug.Log(shoulderRate);
                    float4x4 camRotTgtPosMat = new float4x4(orbitCamera.ThirdPersonRotation, targetEntityLocalToWorld.Position);
                    float3 thirdPersonPosition = 
                        camRotTgtPosMat.TransformPoint(new float3(
                            math.lerp(orbitCamera.PositionOffset.x * -1, orbitCamera.PositionOffset.x, shoulderRate)
                        , orbitCamera.PositionOffset.y, 0))
                        //(targetEntityLocalToWorld.Position 
                        //+ targetEntityLocalToWorld.Up * orbitCamera.PositionOffset.y +
                        //targetEntityLocalToWorld.Right * math.lerp(orbitCamera.PositionOffset.x*-1, orbitCamera.PositionOffset.x, shoulderRate)
                        //) 
                        + (-cameraForward * orbitCamera.CurrentDistanceFromObstruction);

                    //shoulderRate = //orbitCamera.viewDirectionRate > 0 ? 1-shoulderRate : shoulderRate;

                    //UnityEngine.Debug.Log(shoulderRate);

                    if (scopeFirstPerson && hasSights )//&& SightsLookup.HasComponent(cameraControl.FollowedCharacterEntity))
                    {
                            //AimingSights sights = SightsLookup[cameraControl.FollowedCharacterEntity];
                        bool offsetChanged = false;
                        bool toggleZoom = cameraControl.ToggleZoom;
                        //float3 targetDir = float3.zero;
                        //quaternion targetRotation = quaternion.identity;
                        //if (toggleZoom || switchShoulderView)
                        //{
                        //    targetDir = math.normalizesafe(sights.LaserPointerPosition - targetEntityLocalToWorld.Position);
                        //    targetRotation = quaternion.LookRotation(targetDir, new float3(0, 1, 0));
                        //}
                            if (toggleZoom)
                        {
                            float3 targetDir = math.normalizesafe(sights.LaserPointerPosition - targetEntityLocalToWorld.Position);
                            quaternion targetRotation = quaternion.LookRotation(targetDir, new float3(0, 1, 0));
                            //UnityEngine.Debug.Log("Zoom Toggled");
                            sights.FirstPersonZoomOffset = camRotTgtPosMat.InverseTransformRotation(
                                targetRotation);
                            offsetChanged = true;
                            }
                        //if (switchShoulderView)
                        //{
                        //    float3 targetShoulderViewPoint = camRotTgtPosMat.TransformPoint(new float3(
                        //        orbitCamera.PositionOffset.x * orbitCamera.viewDirectionRate > 0 ? -1 : 1
                        //        , orbitCamera.PositionOffset.y, 0));

                        //    float4x4 targetShoulderViewMatrix = new float4x4(orbitCamera.ThirdPersonRotation, targetShoulderViewPoint);

                        //    float3 targetDir = math.normalizesafe(sights.LaserPointerPosition - targetShoulderViewPoint);
                        //    quaternion targetWorldRotation = quaternion.LookRotation(targetDir, new float3(0, 1, 0));
                        //    sights.TargetSwitchShoulderViewOffset = targetShoulderViewMatrix.InverseTransformRotation(targetWorldRotation);

                        //    //new float4x4(orbitCamera.ThirdPersonRotation, (targetEntityLocalToWorld.Position +
                        //    //targetEntityLocalToWorld.Right * orbitCamera.PositionOffset.x * (orbitCamera.viewDirectionRate > 0 ? 1:-1)) ).InverseTransformRotation(
                        //    //targetRotation);

                        //    //if (orbitCamera.viewDirectionRate > 0)
                        //    //{
                        //    //    sights.LeftShoulderViewOffset = targetShoulderViewMatrix.InverseTransformRotation(targetWorldRotation);
                        //    //    sights.RightShoulderViewOffset = quaternion.identity;
                        //    //}
                        //    //else
                        //    //{
                        //    //    sights.RightShoulderViewOffset = targetShoulderViewMatrix.InverseTransformRotation(targetWorldRotation);
                        //    //    sights.LeftShoulderViewOffset = quaternion.identity;
                        //    //}

                        //    offsetChanged = true;
                        //}
                        

                        //if (offsetChanged)
                        //{
                        //    SightsLookup[cameraControl.FollowedCharacterEntity] = sights; 
                        //}

                        //if (toggleZoom)
                        {
                            //quaternion offsetRot = math.mul(orbitCamera.ThirdPersonRotation, sights.FirstPersonZoomOffset);
                            //float3 fwd = math.mul(orbitCamera.ThirdPersonRotation, new float3(0, 0, 1));
                            //float3 offsetFwd = math.mul(offsetRot, new float3(0, 0, 1));
                            //float3 up = math.mul(orbitCamera.ThirdPersonRotation, new float3(0, 1, 0));
                            //float3 offsetUp = math.mul(offsetRot, new float3(0, 1, 0));
                            //transform.Rotation = quaternion.LookRotation(
                            //    math.normalizesafe(math.lerp(fwd, offsetFwd
                            //    , zoomProgress)),
                            //    math.normalizesafe(math.lerp(up, offsetUp, zoomProgress)));
                        }
                        //if (!willReachEndOfShoulderPoint && orbitCamera.viewDirectionRate < 0.999f && orbitCamera.viewDirectionRate > -0.999f)//switchShoulderView)
                        //{
                        //    //UnityEngine.Debug.Log("Shoulder Rate : " + shoulderRate);
                        //    quaternion offsetRot = math.mul(orbitCamera.ThirdPersonRotation, sights.SwitchShoulderViewOffset);
                        //    float3 fwd = math.mul(orbitCamera.ThirdPersonRotation, new float3(0, 0, 1));
                        //    float3 offsetFwd = math.mul(offsetRot, new float3(0, 0, 1));
                        //    float3 up = math.mul(orbitCamera.ThirdPersonRotation, new float3(0, 1, 0));
                        //    float3 offsetUp = math.mul(offsetRot, new float3(0, 1, 0));
                        //    //transform.Rotation = quaternion.LookRotation(
                        //    //    math.normalizesafe(math.lerp(fwd, offsetFwd
                        //    //    , shoulderRate)),
                        //    //    math.normalizesafe(math.lerp(up, offsetUp, shoulderRate)));
                        //}
                        //UnityEngine.Debug.Log(shoulderRate);

                        transform.Rotation
                            =
                            math.mul(
                            orbitCamera.ThirdPersonRotation,
                            //math.mul(
                           // math.mul(
                                math.slerp(quaternion.identity, sights.FirstPersonZoomOffset, zoomProgress)
                            //, math.slerp(sights.LeftShoulderViewOffset, sights.RightShoulderViewOffset, shoulderRate) 
                            //)
                          //  )
                        //    ,
                            
                        //    //sights.CurrentSwitchShoulderViewOffset
                        //    math.slerp(
                        //        orbitCamera.viewDirectionRate > 0? sights.TargetSwitchShoulderViewOffset : quaternion.identity // sights.LeftShoulderViewOffset
                        //    , orbitCamera.viewDirectionRate<0? sights.TargetSwitchShoulderViewOffset : quaternion.identity //RightShoulderViewOffset
                        //    , shoulderRate)
                        // //math.slerp(sights.LeftShoulderViewOffset, sights.RightShoulderViewOffset, shoulderRate)
                        //)
                            )
                            ;
                        //transform.Rotate(math.slerp(quaternion.identity // sights.LeftShoulderViewOffset
                        //    , sights.SwitchShoulderViewOffset //RightShoulderViewOffset
                        //    , shoulderRate>0?DeltaTime : 0));
                            
                    }
                    else
                    {
                        transform.Rotation = orbitCamera.ThirdPersonRotation; 
                    }
                    transform.Position = scopeFirstPerson ?
                        math.lerp(thirdPersonPosition, targetEntityLocalToWorld.Position, zoomProgress)
                        : thirdPersonPosition;

                    if (hasSights)
                    {

                        SightsLookup[cameraControl.FollowedCharacterEntity] = sights;
                    }

                    // Manually calculate the LocalToWorld since this is updating after the Transform systems, and the LtW is what rendering uses
                    //LocalToWorld cameraLocalToWorld = new LocalToWorld();
                    cameraLocalToWorld.Value = new float4x4(transform.Rotation, transform.Position);
                    LocalToWorldLookup[entity] = cameraLocalToWorld;

                    
                }
            }
        }
    } }