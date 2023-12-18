using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

//[UpdateInGroup(typeof(InitializationSystemGroup))]
namespace ImaginaryReactor
{
    public partial class EnemySpawnerSystem : SystemBase
    {
        protected override void OnCreate()
        {

        }

        protected override void OnUpdate()
        {
            if (EnemySpawnerManager.instance)
            {
                var manager = EnemySpawnerManager.instance;

                if (manager.spawnNow)
                {
                    var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

                    foreach (var spawner in SystemAPI.Query<EnemySpawner>())//.WithAll<EnemySpawner>())
                    {
                        if (spawner.ID == manager.targetID)
                        {
                            var characterEntity = ecb.Instantiate(spawner.CharacterEnity);
                            ecb.SetComponent(characterEntity, new LocalTransform() { Position = manager.transform.position, Rotation = manager.transform.rotation, Scale = 1 });



                            //new LocalToWorld() { Value = float4x4.TRS(manager.transform.position, manager.transform.rotation, new float3(1,1,1)) });

                            //var hitboxEntity = ecb.Instantiate(spawner.HitboxEntity);
                            //ecb.AddComponent(hitboxEntity, new Hitbox()
                            //{
                            //    Owner = characterEntity,//GetEntity(authoring.OwnerGO, TransformUsageFlags.Dynamic),
                            //    //BoundSize = boundSize
                            //});
                            ////if (authoring.parent != null)
                            //{
                            //    ecb.AddComponent(hitboxEntity, new SeperatedChild()
                            //    {
                            //        Parent = characterEntity,//GetEntity(authoring.parent, TransformUsageFlags.Dynamic),
                            //        LocalPosition = authoring.parent.InverseTransformPoint(authoring.transform.position),
                            //        LocalForward = authoring.parent.InverseTransformDirection(authoring.transform.forward),
                            //        LocalUp = authoring.parent.InverseTransformDirection(authoring.transform.up),
                            //        LocalRotation = Quaternion.LookRotation(authoring.parent.InverseTransformDirection(authoring.transform.forward), authoring.parent.InverseTransformDirection(authoring.transform.up))
                            //    });
                            //}
                        }
                    }

                    manager.spawnNow = false;
                }
            }
        }
    }
}