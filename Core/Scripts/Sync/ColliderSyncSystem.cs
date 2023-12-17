using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ColliderSyncSystem : SystemBase
{
    protected override void OnCreate()
    {
    }

    protected override void OnUpdate()
    {
        if (ColliderSyncManager.instance)
        {
            var manager = ColliderSyncManager.instance;

            //foreach (var (cc, transform, entity) in SystemAPI.Query<ColliderSyncComponent,RefRW<LocalTransform>>().WithEntityAccess())
            foreach (var (cc,transfrom) in SystemAPI.Query<ColliderSyncComponent,RefRW<LocalTransform>>().WithAll<ColliderSyncComponent, LocalTransform>())//.WithEntityAccess())
            {
                if (manager.colliderDict.ContainsKey(cc.ID))
                {
                        transfrom.ValueRW.Position = (float3)manager.colliderDict[cc.ID].transform.position;
                        transfrom.ValueRW.Rotation = manager.colliderDict[cc.ID].transform.rotation;    
                }

               
            }
        }
        }
    }
