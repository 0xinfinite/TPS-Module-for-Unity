using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ImaginaryReactor
{
    public class TelescopeAuthoring : MonoBehaviour
    {
        public float fovWhenZoom = 30f;
        public bool firstPerson = false;
        public float zoomSpeed = 10f;

        public class Baker : Baker<TelescopeAuthoring>
        {
           public override void Bake(TelescopeAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Telescope() { 
                    FirstPerson = authoring.firstPerson,
                    FovWhenZoom = authoring.fovWhenZoom,
                    ZoomSpeed = authoring.zoomSpeed,
                    ZoomProgress = 0
                });
            }
        }

    }
}
