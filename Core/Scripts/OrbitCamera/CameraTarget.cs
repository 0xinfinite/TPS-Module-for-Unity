using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ImaginaryReactor
{
    [Serializable]
    public struct CameraTarget : IComponentData
    {
        public Entity TargetEntity;
    }
}
