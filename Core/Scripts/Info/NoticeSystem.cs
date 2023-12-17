using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial class NoticeSystem : SystemBase
{

    protected override void OnUpdate()
    {
        if(NoticeUIManager.instance != null)
        {
         
            foreach (var (hitInfo, tag) in SystemAPI.Query<TrackInfo, PlayerTag>().WithAll<TrackInfo, PlayerTag>())
            {
         
                NoticeUIManager.instance.ShowHitVector(hitInfo.LastKnownVector, hitInfo.DamageAmount);
            }
        }
    }
}
