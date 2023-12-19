using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ImaginaryReactor
{
    [Serializable]
    public struct MainEntityCamera : IComponentData
    {
        public float BaseFov;
        public float Fov;
    }
}