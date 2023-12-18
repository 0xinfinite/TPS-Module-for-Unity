using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
namespace ImaginaryReactor {

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(OrbitCameraSystem))]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]// SimulationSystemGroup))]
                                                                  //[UpdateAfter(typeof(TransformSystemGroup))]
                                                                  //[UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    [BurstCompile]
    public partial struct HeadCamSystem : ISystem
    {
        //public struct CameraObstructionHitsCollector : ICollector<ColliderCastHit>
        //{
        //    public bool EarlyOutOnFirstHit => false;
        //    public float MaxFraction => 1f;
        //    public int NumHits { get; private set; }

        //    public ColliderCastHit ClosestHit;

        //    private float _closestHitFraction;
        //    private float3 _cameraDirection;
        //    private Entity _followedCharacter;
        //    private DynamicBuffer<OrbitCameraIgnoredEntityBufferElement> _ignoredEntitiesBuffer;

        //    public CameraObstructionHitsCollector(Entity followedCharacter, DynamicBuffer<OrbitCameraIgnoredEntityBufferElement> ignoredEntitiesBuffer, float3 cameraDirection)
        //    {
        //        NumHits = 0;
        //        ClosestHit = default;

        //        _closestHitFraction = float.MaxValue;
        //        _cameraDirection = cameraDirection;
        //        _followedCharacter = followedCharacter;
        //        _ignoredEntitiesBuffer = ignoredEntitiesBuffer;
        //    }

        //    public bool AddHit(ColliderCastHit hit)
        //    {
        //        if (_followedCharacter == hit.Entity)
        //        {
        //            return false;
        //        }

        //        if (math.dot(hit.SurfaceNormal, _cameraDirection) < 0f || !PhysicsUtilities.IsCollidable(hit.Material))
        //        {
        //            return false;
        //        }

        //        for (int i = 0; i < _ignoredEntitiesBuffer.Length; i++)
        //        {
        //            if (_ignoredEntitiesBuffer[i].Entity == hit.Entity)
        //            {
        //                return false;
        //            }
        //        }

        //        // Process valid hit
        //        if (hit.Fraction < _closestHitFraction)
        //        {
        //            _closestHitFraction = hit.Fraction;
        //            ClosestHit = hit;
        //        }
        //        NumHits++;

        //        return true;
        //    }
        //}
        //[BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<HeadCam>().Build());
        }

        public void OnDestroy(ref SystemState state)
        {
        }
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            HeadCamJob job = new HeadCamJob
            {
                //DeltaTime = SystemAPI.Time.DeltaTime,
                //PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
                LocalToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(false)//,
                                                                                      //CameraTargetLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),
                                                                                      //KinematicCharacterBodyLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),
            };
            job.Schedule();
        }

        //[BurstCompile]
        [WithAll(typeof(Simulate))]
    public partial struct HeadCamJob : IJobEntity
        {
            //public float DeltaTime;
            //public PhysicsWorld PhysicsWorld;

            public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            //public ComponentLookup<LocalToWorld> CameraTargetLookup;
            //public ComponentLookup<LocalToWorld> KinematicCharacterBodyLookup;

            void Execute(
                Entity entity,
                ref LocalTransform transform,
                ref HeadCam headCam
                //in HeadCamControl cameraControl,
                )
            {
                LocalToWorld targetEntityLocalToWorld = default;
                //LocalToWorld targetCameraEntityLocalToWorld = default;
                quaternion rot = quaternion.identity;
                float3 pos = float3.zero;
                // if there is a followed entity, place the camera relatively to it
                if (LocalToWorldLookup.HasComponent(headCam.FollowedCharacterEntity))// TryGetComponent(headCam.FollowedCharacterEntity, out LocalToWorld characterLTW))
                {
                    // Select the real camera target
                    targetEntityLocalToWorld = LocalToWorldLookup[headCam.FollowedCharacterEntity];//characterLTW;
                                                                                                   //targetCameraEntityLocalToWorld = LocalToWorldLookup[headCam.FollowedCharacterEntity];
                                                                                                   //if (LocalToWorldLookup.HasComponent(headCam.FollowedCameraEntity))//TryGetComponent(headCam.FollowedCameraEntity, out LocalToWorld cameraTarget) )
                    {
                        var targetCameraEntityLocalToWorld = LocalToWorldLookup.GetRefRO(headCam.FollowedCameraEntity);//.[headCam.FollowedCameraEntity];//cameraTarget;

                        rot = quaternion.LookRotation(targetCameraEntityLocalToWorld.ValueRO.Forward,
                        math.abs(targetCameraEntityLocalToWorld.ValueRO.Forward.y) > 0.99 ? targetEntityLocalToWorld.Forward * -1 : new float3(0, 1, 0)
                        //math.cross(math.cross(targetCameraEntityLocalToWorld.Forward, targetCameraEntityLocalToWorld.Up), targetCameraEntityLocalToWorld.Forward)
                        );

                        pos = targetEntityLocalToWorld.Position + targetEntityLocalToWorld.Forward * headCam.HeadCamOffset.z +
                            targetEntityLocalToWorld.Up * headCam.HeadCamOffset.y + targetEntityLocalToWorld.Right * headCam.HeadCamOffset.x;
                        //new float3(0, headCam.HeadCamHeight, 0);

                        transform.Rotation = rot;
                        transform.Position = pos;

                        LocalToWorld cameraLocalToWorld = new LocalToWorld();
                        cameraLocalToWorld.Value = new float4x4(transform.Rotation, transform.Position);
                        LocalToWorldLookup.GetRefRW(entity).ValueRW = cameraLocalToWorld;
                        //ltw.Value = new float4x4(transform.Rotation, transform.Position);
                    }

                }
            }
        }
    } 
}