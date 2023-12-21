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
            state.Dependency =// AimingSightsJob job =
                              new AimingSightsJob()
                              {
                                  //DeltaTime = SystemAPI.Time.DeltaTime,
                                  PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                                  entityManager = state.EntityManager,
                                  LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(false),
                                  LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(false),

                              }.Schedule(state.Dependency);
            //job.Schedule();
            state.Dependency.Complete();
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
        public partial struct AimingSightsJob : IJobEntity
        {
            //public float DeltaTime;
            public PhysicsWorld PhysicsWorld;
            public EntityManager entityManager;

            public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            public ComponentLookup<LocalTransform> LocalTransformLookup;

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

                        var buffer = entityManager.GetBuffer<IgnoreHitboxData>(entity);

                        if (PhysicsWorld.CastRay(ray, out Unity.Physics.RaycastHit hit))
                        {
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