using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ModelSyncComponent : IComponentData
{
    public int ID;
    public byte rootMotionFlag;
}
