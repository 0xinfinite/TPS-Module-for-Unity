using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace ImaginaryReactor
{
    [DisallowMultipleComponent]
    public class MainEntityCameraAuthoring : MonoBehaviour
    {
        public float BaseFov = 90f;

        public class Baker : Baker<MainEntityCameraAuthoring>
        {
            public override void Bake(MainEntityCameraAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<MainEntityCamera>(entity, new MainEntityCamera() { BaseFov = authoring.BaseFov, Fov = authoring.BaseFov });
            }
        }
    }
}