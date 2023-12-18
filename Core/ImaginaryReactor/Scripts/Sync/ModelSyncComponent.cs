using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
namespace ImaginaryReactor
{
    public struct ModelSyncComponent : IComponentData
    {
        public int ID;
        public byte rootMotionFlag;
    }
}