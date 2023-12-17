using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class CrosshairSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (CrosshairObject.Target != null && SystemAPI.HasSingleton<PlayerLaserPointer>())
        {
            Entity playerLaserPointerEntity = SystemAPI.GetSingletonEntity<PlayerLaserPointer>();
            LocalToWorld targetLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(playerLaserPointerEntity);
            CrosshairObject.Target.transform.SetPositionAndRotation(Camera.main.WorldToScreenPoint(targetLocalToWorld.Position), Quaternion.identity);
        }
    }
}
